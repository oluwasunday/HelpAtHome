using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;
        private readonly IAuditLogService _audit;

        public AdminService(
            IUnitOfWork uow,
            UserManager<User> userManager,
            IMapper mapper,
            INotificationService notification,
            IAuditLogService audit)
        {
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
            _notification = notification;
            _audit = audit;
        }

        public async Task<Result<AdminDashboardDto>> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var (revenueAll, revenueMonth, revenueWeek) = await _uow.Bookings.GetRevenueStatsAsync();

            var dashboard = new AdminDashboardDto
            {
                TotalClients = await _userManager.Users
                    .CountAsync(u => !u.IsDeleted && u.Role == UserRole.Client),
                TotalCaregivers = await _userManager.Users
                    .CountAsync(u => !u.IsDeleted && (u.Role == UserRole.IndividualCaregiver || u.Role == UserRole.AgencyCaregiver)),
                TotalAgencies = await _uow.Agencies.CountAsync(a => !a.IsDeleted),
                PendingVerifications = await _uow.CaregiverProfiles
                    .CountAsync(c => c.VerificationStatus == VerificationStatus.Pending),
                SuspendedUsers = await _userManager.Users
                    .CountAsync(u => !u.IsDeleted && u.IsSuspended),

                TotalBookings = await _uow.Bookings.CountAsync(b => true),
                ActiveBookings = await _uow.Bookings.CountAsync(b =>
                    b.Status == BookingStatus.Accepted || b.Status == BookingStatus.InProgress),
                CompletedThisMonth = await _uow.Bookings.CountAsync(b =>
                    b.Status == BookingStatus.Completed && b.CompletedAt >= monthStart),
                OpenDisputes = await _uow.Bookings.CountAsync(b => b.HasDispute),

                RevenueThisWeek = revenueWeek,
                RevenueThisMonth = revenueMonth,
                RevenueTotalAllTime = revenueAll,

                OpenSupportTickets = await _uow.SupportTickets.CountAsync(t =>
                    !t.IsDeleted && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress)),
                ActiveEmergencyAlerts = await _uow.EmergencyAlerts.CountAsync(a =>
                    !a.IsDeleted && a.Status == AlertStatus.Active)
            };

            return Result<AdminDashboardDto>.Ok(dashboard);
        }

        public async Task<Result<PagedResult<AdminUserDto>>> GetUsersAsync(AdminUserFilterDto filter)
        {
            var query = _userManager.Users.Where(u => !u.IsDeleted);

            if (filter.Role.HasValue)
                query = query.Where(u => u.Role == filter.Role.Value);
            if (filter.IsSuspended.HasValue)
                query = query.Where(u => u.IsSuspended == filter.IsSuspended.Value);
            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    u.Email!.ToLower().Contains(term));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<AdminUserDto>>(users);
            return Result<PagedResult<AdminUserDto>>.Ok(
                new PagedResult<AdminUserDto>(dtos, total, filter.Page, filter.PageSize));
        }

        public async Task<Result<AdminUserDto>> GetUserAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.CaregiverProfile).ThenInclude(c => c!.Address)
                .Include(u => u.CaregiverProfile).ThenInclude(c => c!.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Include(u => u.ClientProfile)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
                return Result<AdminUserDto>.Fail("User not found");

            return Result<AdminUserDto>.Ok(_mapper.Map<AdminUserDto>(user));
        }

        public async Task<Result> SuspendUserAsync(Guid adminId, Guid userId, SuspendUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                return Result.Fail("User not found");
            if (user.IsSuspended)
                return Result.Fail("User is already suspended");
            if (user.Role is UserRole.SuperAdmin or UserRole.Admin)
                return Result.Fail("Cannot suspend an admin user");

            user.IsSuspended = true;
            user.SuspensionReason = dto.Reason;
            user.SuspendedUntil = dto.SuspendedUntil;
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _notification.SendAsync(userId, "Account Suspended",
                $"Your account has been suspended. Reason: {dto.Reason}", "system", null);

            await _audit.LogAsync(adminId, "Admin", AuditAction.Suspend, "User", userId.ToString(),
                $"Suspended. Reason: {dto.Reason}");

            return Result.Ok();
        }

        public async Task<Result> UnsuspendUserAsync(Guid adminId, Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                return Result.Fail("User not found");
            if (!user.IsSuspended)
                return Result.Fail("User is not suspended");

            user.IsSuspended = false;
            user.SuspensionReason = null;
            user.SuspendedUntil = null;
            user.IsActive = true;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _notification.SendAsync(userId, "Account Restored",
                "Your account suspension has been lifted. You can now log in.", "system", null);

            await _audit.LogAsync(adminId, "Admin", AuditAction.Update, "User", userId.ToString(),
                "Suspension lifted");

            return Result.Ok();
        }

        public async Task<Result> DeleteUserAsync(Guid adminId, Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                return Result.Fail("User not found");
            if (user.Role is UserRole.SuperAdmin or UserRole.Admin)
                return Result.Fail("Cannot delete an admin user");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = adminId.ToString();
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _audit.LogAsync(adminId, "Admin", AuditAction.Delete, "User", userId.ToString());

            return Result.Ok();
        }

        public async Task<Result<PagedResult<VerificationDocumentDto>>> GetPendingDocumentsAsync(int page, int size)
        {
            var (items, total) = await _uow.VerificationDocuments.GetPendingPagedAsync(page, size);
            var dtos = _mapper.Map<List<VerificationDocumentDto>>(items);
            return Result<PagedResult<VerificationDocumentDto>>.Ok(
                new PagedResult<VerificationDocumentDto>(dtos, total, page, size));
        }

        public async Task<Result> ReviewDocumentAsync(Guid adminId, Guid documentId, ReviewDocumentDto dto)
        {
            var doc = await _uow.VerificationDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);
            if (doc == null)
                return Result.Fail("Document not found");
            if (doc.Status != VerificationStatus.Pending)
                return Result.Fail("Document has already been reviewed");

            doc.Status = dto.IsApproved ? VerificationStatus.Approved : VerificationStatus.Rejected;
            doc.ReviewNote = dto.ReviewNote;
            doc.ReviewedByAdminId = adminId;
            doc.ReviewedAt = DateTime.UtcNow;
            _uow.VerificationDocuments.Update(doc);
            await _uow.SaveChangesAsync();

            if (doc.CaregiverProfileId.HasValue)
            {
                var profile = await _uow.CaregiverProfiles.GetByIdAsync(doc.CaregiverProfileId.Value);
                if (profile != null)
                    await _notification.SendAsync(profile.UserId, "Document Review",
                        dto.IsApproved
                            ? "Your document has been approved."
                            : $"Your document was rejected. {dto.ReviewNote}",
                        "system", null);
            }

            var auditAction = dto.IsApproved ? AuditAction.Approve : AuditAction.Reject;
            await _audit.LogAsync(adminId, "Admin", auditAction, "VerificationDocument", documentId.ToString(),
                dto.ReviewNote);

            return Result.Ok();
        }

        public async Task<Result> SetAgencyCommissionAsync(Guid adminId, Guid agencyId, SetCommissionDto dto)
        {
            var agency = await _uow.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId && !a.IsDeleted);
            if (agency == null)
                return Result.Fail("Agency not found");

            agency.CommissionRate = dto.CommissionRate;
            agency.AgencyCommissionRate = dto.AgencyCommissionRate;
            _uow.Agencies.Update(agency);
            await _uow.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result<PagedResult<BookingDto>>> GetOpenDisputesAsync(int page, int size)
        {
            var all = await _uow.Bookings.FindAsync(b => b.HasDispute && b.Status == BookingStatus.Disputed);
            var ordered = all.OrderByDescending(b => b.CreatedAt).ToList();
            var total = ordered.Count;
            var items = ordered.Skip((page - 1) * size).Take(size).ToList();
            var dtos = _mapper.Map<List<BookingDto>>(items);
            return Result<PagedResult<BookingDto>>.Ok(new PagedResult<BookingDto>(dtos, total, page, size));
        }
    }
}
