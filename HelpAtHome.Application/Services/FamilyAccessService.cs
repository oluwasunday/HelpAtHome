using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.AspNetCore.Identity;

namespace HelpAtHome.Application.Services
{
    public class FamilyAccessService : IFamilyAccessService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notifications;
        private readonly UserManager<User> _userManager;

        public FamilyAccessService(
            IUnitOfWork uow,
            IMapper mapper,
            INotificationService notifications,
            UserManager<User> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _notifications = notifications;
            _userManager = userManager;
        }

        public async Task<Result<FamilyAccessDto>> InviteFamilyMemberAsync(Guid clientUserId, InviteFamilyMemberDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneOrEmail))
                return Result<FamilyAccessDto>.Fail("Phone number or email is required.");

            // Verify the inviting user is a client
            var clientUser = await _uow.Users.GetByIdWithProfileAsync(clientUserId);
            if (clientUser == null || clientUser.Role != UserRole.Client)
                return Result<FamilyAccessDto>.Fail("Only clients can invite family members.");

            // Resolve the family member user by email or phone
            User? familyMemberUser;
            var input = dto.PhoneOrEmail.Trim();
            if (input.Contains('@'))
                familyMemberUser = await _userManager.FindByEmailAsync(input);
            else
                familyMemberUser = await _uow.Users.GetByPhoneAsync(input);

            if (familyMemberUser == null)
                return Result<FamilyAccessDto>.Fail("No user found with that phone number or email.");

            if (familyMemberUser.Id == clientUserId)
                return Result<FamilyAccessDto>.Fail("You cannot invite yourself.");

            if (familyMemberUser.Role != UserRole.FamilyMember)
                return Result<FamilyAccessDto>.Fail("The invited user must have the FamilyMember role.");

            // Check for existing access record
            var existing = await _uow.FamilyAccesses.GetByPairAsync(clientUserId, familyMemberUser.Id);
            if (existing != null)
                return Result<FamilyAccessDto>.Fail("This family member has already been invited.");

            var access = new FamilyAccess
            {
                Id = Guid.NewGuid(),
                ClientUserId = clientUserId,
                FamilyMemberUserId = familyMemberUser.Id,
                AccessLevel = dto.AccessLevel,
                IsApproved = false,
                ReceiveEmergencyAlerts = dto.ReceiveEmergencyAlerts,
                ReceiveBookingUpdates = dto.ReceiveBookingUpdates,
                ReceivePaymentAlerts = dto.ReceivePaymentAlerts
            };

            await _uow.FamilyAccesses.AddAsync(access);
            await _uow.SaveChangesAsync();

            // Notify the family member
            await _notifications.SendAsync(
                familyMemberUser.Id,
                "Family access invitation",
                $"{clientUser.FirstName} {clientUser.LastName} has invited you to monitor their care. Please approve or decline.",
                "FamilyInvite", access.Id.ToString());

            var saved = await _uow.FamilyAccesses.GetWithUsersAsync(access.Id);
            return Result<FamilyAccessDto>.Ok(_mapper.Map<FamilyAccessDto>(saved));
        }

        public async Task<Result> ApproveAccessAsync(Guid familyMemberUserId, Guid accessId)
        {
            var access = await _uow.FamilyAccesses.GetWithUsersAsync(accessId);
            if (access == null)
                return Result.Fail("Access record not found.");

            if (access.FamilyMemberUserId != familyMemberUserId)
                return Result.Fail("Access denied.");

            if (access.IsApproved)
                return Result.Fail("Access is already approved.");

            access.IsApproved = true;
            access.ApprovedAt = DateTime.UtcNow;
            _uow.FamilyAccesses.Update(access);
            await _uow.SaveChangesAsync();

            // Notify the client
            await _notifications.SendAsync(
                access.ClientUserId,
                "Family access approved",
                $"{access.FamilyMember.FirstName} {access.FamilyMember.LastName} has accepted your invitation.",
                "FamilyAccessApproved", access.Id.ToString());

            return Result.Ok();
        }

        public async Task<Result> RevokeAccessAsync(Guid clientUserId, Guid accessId)
        {
            var access = await _uow.FamilyAccesses.GetWithUsersAsync(accessId);
            if (access == null)
                return Result.Fail("Access record not found.");

            if (access.ClientUserId != clientUserId)
                return Result.Fail("Access denied.");

            _uow.FamilyAccesses.SoftDelete(access);
            await _uow.SaveChangesAsync();

            // Notify the family member
            await _notifications.SendAsync(
                access.FamilyMemberUserId,
                "Family access revoked",
                "Your access to a client's care details has been revoked.",
                "FamilyAccessRevoked", access.Id.ToString());

            return Result.Ok();
        }

        public async Task<Result<FamilyAccessDto>> UpdateAccessAsync(Guid clientUserId, Guid accessId, UpdateFamilyAccessDto dto)
        {
            var access = await _uow.FamilyAccesses.GetWithUsersAsync(accessId);
            if (access == null)
                return Result<FamilyAccessDto>.Fail("Access record not found.");

            if (access.ClientUserId != clientUserId)
                return Result<FamilyAccessDto>.Fail("Access denied.");

            if (dto.AccessLevel.HasValue)
                access.AccessLevel = dto.AccessLevel.Value;
            if (dto.ReceiveEmergencyAlerts.HasValue)
                access.ReceiveEmergencyAlerts = dto.ReceiveEmergencyAlerts.Value;
            if (dto.ReceiveBookingUpdates.HasValue)
                access.ReceiveBookingUpdates = dto.ReceiveBookingUpdates.Value;
            if (dto.ReceivePaymentAlerts.HasValue)
                access.ReceivePaymentAlerts = dto.ReceivePaymentAlerts.Value;

            _uow.FamilyAccesses.Update(access);
            await _uow.SaveChangesAsync();

            return Result<FamilyAccessDto>.Ok(_mapper.Map<FamilyAccessDto>(access));
        }

        public async Task<Result<List<FamilyAccessDto>>> GetClientFamilyAccessesAsync(Guid clientUserId)
        {
            var accesses = await _uow.FamilyAccesses.GetByClientUserIdAsync(clientUserId);
            return Result<List<FamilyAccessDto>>.Ok(_mapper.Map<List<FamilyAccessDto>>(accesses));
        }

        public async Task<Result<List<FamilyAccessDto>>> GetMyAccessesAsync(Guid familyMemberUserId)
        {
            var accesses = await _uow.FamilyAccesses.GetByFamilyMemberUserIdAsync(familyMemberUserId);
            return Result<List<FamilyAccessDto>>.Ok(_mapper.Map<List<FamilyAccessDto>>(accesses));
        }

        public async Task<Result<FamilyClientViewDto>> GetClientViewAsync(Guid familyMemberUserId, Guid clientUserId)
        {
            // Verify approved, non-revoked access
            var access = await _uow.FamilyAccesses.GetByPairAsync(clientUserId, familyMemberUserId);
            if (access == null || !access.IsApproved || access.IsDeleted)
                return Result<FamilyClientViewDto>.Fail("You do not have approved access to this client.");

            var clientProfile = await _uow.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
            if (clientProfile == null)
                return Result<FamilyClientViewDto>.Fail("Client profile not found.");

            // Load User nav for mapping
            var clientUser = await _uow.Users.GetByIdWithProfileAsync(clientUserId);
            clientProfile.User = clientUser!;

            // Active booking
            var allBookings = (await _uow.Bookings.GetClientBookingsAsync(clientProfile.Id, null)).ToList();
            var activeBooking = allBookings.FirstOrDefault(b =>
                b.Status == BookingStatus.Accepted || b.Status == BookingStatus.InProgress);
            var recentBookings = allBookings
                .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Cancelled)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToList();

            // Recent emergency alerts
            var (alerts, _) = await _uow.EmergencyAlerts.GetPagedAsync(clientProfile.Id, null, 1, 5);

            var clientProfileDto = _mapper.Map<ClientProfileDto>(clientProfile);

            var view = new FamilyClientViewDto
            {
                Client = clientProfileDto,
                ActiveBooking = activeBooking != null ? _mapper.Map<BookingDto>(activeBooking) : null,
                RecentBookings = _mapper.Map<List<BookingDto>>(recentBookings),
                RecentAlerts = _mapper.Map<List<EmergencyAlertDto>>(alerts),
                AccessLevel = access.AccessLevel.ToString()
            };

            return Result<FamilyClientViewDto>.Ok(view);
        }
    }
}
