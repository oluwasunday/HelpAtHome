using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Services
{
    public class EmergencyService : IEmergencyService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notifications;

        public EmergencyService(IUnitOfWork uow, IMapper mapper, INotificationService notifications)
        {
            _uow = uow;
            _mapper = mapper;
            _notifications = notifications;
        }

        public async Task<Result<EmergencyAlertDto>> TriggerAlertAsync(Guid clientUserId, TriggerAlertDto dto)
        {
            var clientProfile = await _uow.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
            if (clientProfile == null)
                return Result<EmergencyAlertDto>.Fail("Client profile not found.");

            // Find the active booking for context if any
            var activeBooking = await _uow.Bookings.FirstOrDefaultAsync(b =>
                b.ClientProfileId == clientProfile.Id &&
                (b.Status == BookingStatus.Accepted || b.Status == BookingStatus.InProgress));

            var alert = new EmergencyAlert
            {
                Id = Guid.NewGuid(),
                ClientProfileId = clientProfile.Id,
                ActiveBookingId = activeBooking?.Id,
                Status = AlertStatus.Active,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                LocationAddress = dto.Address?.Trim(),
                Message = dto.Message?.Trim(),
                NotifiedFamily = false,
                NotifiedAdmin = false,
                NotifiedCaregiver = false
            };

            await _uow.EmergencyAlerts.AddAsync(alert);
            await _uow.SaveChangesAsync();

            // Notify family members who opted in for emergency alerts
            var familyAccesses = await _uow.FamilyAccesses.GetByClientUserIdAsync(clientUserId);
            var notifiedFamily = false;
            foreach (var access in familyAccesses.Where(f => f.IsApproved && !f.IsDeleted && f.ReceiveEmergencyAlerts))
            {
                await _notifications.SendAsync(
                    access.FamilyMemberUserId,
                    "EMERGENCY ALERT",
                    $"Your family member has triggered an emergency alert. Location: {dto.Address ?? $"{dto.Latitude},{dto.Longitude}"}",
                    "EmergencyAlert", alert.Id.ToString(),
                    sendPush: true, sendSms: true);
                notifiedFamily = true;
            }

            // Notify active caregiver if booking is in progress
            var notifiedCaregiver = false;
            if (activeBooking != null)
            {
                var caregiverProfile = await _uow.CaregiverProfiles.GetByIdAsync(activeBooking.CaregiverProfileId);
                if (caregiverProfile != null)
                {
                    await _notifications.SendAsync(
                        caregiverProfile.UserId,
                        "EMERGENCY ALERT",
                        "Your current client has triggered an emergency alert. Please check on them immediately.",
                        "EmergencyAlert", alert.Id.ToString(),
                        sendPush: true);
                    notifiedCaregiver = true;
                }
            }

            // Update notification flags and persist
            alert.NotifiedFamily = notifiedFamily;
            alert.NotifiedCaregiver = notifiedCaregiver;
            _uow.EmergencyAlerts.Update(alert);
            await _uow.SaveChangesAsync();

            var saved = await _uow.EmergencyAlerts.GetWithDetailsAsync(alert.Id);
            return Result<EmergencyAlertDto>.Ok(_mapper.Map<EmergencyAlertDto>(saved));
        }

        public async Task<Result<EmergencyAlertDto>> GetAlertAsync(Guid userId, Guid alertId)
        {
            var alert = await _uow.EmergencyAlerts.GetWithDetailsAsync(alertId);
            if (alert == null)
                return Result<EmergencyAlertDto>.Fail("Alert not found.");

            // Access: the client who triggered, their family members, or admin
            var user = await _uow.Users.GetByIdWithProfileAsync(userId);
            if (user == null)
                return Result<EmergencyAlertDto>.Fail("User not found.");

            bool isAdmin = user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin;
            bool isClient = alert.ClientProfile.UserId == userId;

            if (!isAdmin && !isClient)
            {
                // Check if user is an approved, non-revoked family member
                var access = await _uow.FamilyAccesses.GetByPairAsync(alert.ClientProfile.UserId, userId);
                if (access == null || !access.IsApproved || access.IsDeleted)
                    return Result<EmergencyAlertDto>.Fail("Access denied.");
            }

            return Result<EmergencyAlertDto>.Ok(_mapper.Map<EmergencyAlertDto>(alert));
        }

        public async Task<Result<PagedResult<EmergencyAlertDto>>> GetMyAlertsAsync(Guid clientUserId, int page, int size)
        {
            var clientProfile = await _uow.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == clientUserId);
            if (clientProfile == null)
                return Result<PagedResult<EmergencyAlertDto>>.Fail("Client profile not found.");

            var (items, total) = await _uow.EmergencyAlerts.GetPagedAsync(clientProfile.Id, null, page, size);
            var dtos = _mapper.Map<List<EmergencyAlertDto>>(items);
            return Result<PagedResult<EmergencyAlertDto>>.Ok(new PagedResult<EmergencyAlertDto>(dtos, total, page, size));
        }

        public async Task<Result<PagedResult<EmergencyAlertDto>>> GetActiveAlertsAsync(AlertStatus? status, int page, int size)
        {
            var (items, total) = await _uow.EmergencyAlerts.GetPagedAsync(null, status, page, size);
            var dtos = _mapper.Map<List<EmergencyAlertDto>>(items);
            return Result<PagedResult<EmergencyAlertDto>>.Ok(new PagedResult<EmergencyAlertDto>(dtos, total, page, size));
        }

        public async Task<Result<EmergencyAlertDto>> RespondToAlertAsync(Guid responderId, Guid alertId, RespondAlertDto dto)
        {
            // Only admins may respond to / close emergency alerts
            var responder = await _uow.Users.GetByIdWithProfileAsync(responderId);
            if (responder == null || (responder.Role != UserRole.Admin && responder.Role != UserRole.SuperAdmin))
                return Result<EmergencyAlertDto>.Fail("Only administrators can respond to emergency alerts.");

            var alert = await _uow.EmergencyAlerts.GetWithDetailsAsync(alertId);
            if (alert == null)
                return Result<EmergencyAlertDto>.Fail("Alert not found.");

            if (alert.Status == AlertStatus.Resolved || alert.Status == AlertStatus.FalseAlarm)
                return Result<EmergencyAlertDto>.Fail("Alert has already been closed.");

            alert.Status = dto.NewStatus;
            alert.RespondedAt = DateTime.UtcNow;
            alert.RespondedByUserId = responderId;
            alert.ResolutionNote = dto.ResolutionNote?.Trim();
            alert.NotifiedAdmin = true;
            _uow.EmergencyAlerts.Update(alert);
            await _uow.SaveChangesAsync();

            // Notify the client of the response
            await _notifications.SendAsync(
                alert.ClientProfile.UserId,
                "Emergency alert updated",
                $"Your emergency alert status has been updated to: {dto.NewStatus}",
                "EmergencyAlert", alert.Id.ToString());

            // Notify family members
            var familyAccesses = await _uow.FamilyAccesses.GetByClientUserIdAsync(alert.ClientProfile.UserId);
            foreach (var access in familyAccesses.Where(f => f.IsApproved && !f.IsDeleted && f.ReceiveEmergencyAlerts))
            {
                await _notifications.SendAsync(
                    access.FamilyMemberUserId,
                    "Emergency alert updated",
                    $"The emergency alert for your family member has been updated to: {dto.NewStatus}",
                    "EmergencyAlert", alert.Id.ToString());
            }

            return Result<EmergencyAlertDto>.Ok(_mapper.Map<EmergencyAlertDto>(alert));
        }
    }
}
