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
    public class AgencyService : IAgencyService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;

        public AgencyService(IUnitOfWork uow, IMapper mapper, INotificationService notification)
        {
            _uow = uow;
            _mapper = mapper;
            _notification = notification;
        }

        public async Task<Result<AgencyDto>> RegisterAgencyAsync(RegisterAgencyDto dto, Guid agencyAdminUserId)
        {
            if (await _uow.Agencies.AnyAsync(a => a.AgencyAdminUserId == agencyAdminUserId && !a.IsDeleted))
                return Result<AgencyDto>.Fail("You have already registered an agency");

            if (await _uow.Agencies.RegistrationNumberExistsAsync(dto.RegistrationNumber))
                return Result<AgencyDto>.Fail("Registration number already in use");

            if (await _uow.Agencies.AnyAsync(a => a.Email == dto.Email && !a.IsDeleted))
                return Result<AgencyDto>.Fail("Email already in use by another agency");

            var agencyId = Guid.NewGuid();
            var address = new AgencyAddress
            {
                Id = Guid.NewGuid(),
                AgencyId = agencyId,
                Line1 = dto.Address.Line1,
                Line2 = dto.Address.Line2,
                Locality = dto.Address.Locality,
                City = dto.Address.City ?? string.Empty,
                LGA = dto.Address.LGA,
                State = dto.Address.State,
                Country = dto.Address.Country,
                PostalCode = dto.Address.PostalCode
            };

            var agency = new Agency
            {
                Id = agencyId,
                AgencyAdminUserId = agencyAdminUserId,
                AgencyName = dto.AgencyName.Trim(),
                RegistrationNumber = dto.RegistrationNumber.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PhoneNumber = dto.PhoneNumber,
                Description = dto.Description,
                Website = dto.Website,
                AgencyAddressId = address.Id,
                AgencyAddress = address   // EF inserts Agency first, then AgencyAddress
            };

            await _uow.Agencies.AddAsync(agency);
            await _uow.SaveChangesAsync();

            var saved = await _uow.Agencies.GetWithDetailsAsync(agencyId);
            return Result<AgencyDto>.Ok(ToDto(saved!));
        }

        public async Task<Result<AgencyDto>> GetAgencyAsync(Guid agencyId)
        {
            var agency = await _uow.Agencies.GetWithDetailsAsync(agencyId);
            if (agency == null)
                return Result<AgencyDto>.Fail("Agency not found");
            return Result<AgencyDto>.Ok(ToDto(agency));
        }

        public async Task<Result<AgencyDto>> UpdateAgencyAsync(Guid agencyId, UpdateAgencyDto dto, Guid requestingUserId)
        {
            var agency = await _uow.Agencies.GetWithDetailsAsync(agencyId);
            if (agency == null)
                return Result<AgencyDto>.Fail("Agency not found");
            if (agency.AgencyAdminUserId != requestingUserId)
                return Result<AgencyDto>.Fail("Unauthorized");

            if (dto.AgencyName != null) agency.AgencyName = dto.AgencyName.Trim();
            if (dto.Email != null) agency.Email = dto.Email.Trim().ToLowerInvariant();
            if (dto.PhoneNumber != null) agency.PhoneNumber = dto.PhoneNumber;
            if (dto.Description != null) agency.Description = dto.Description;
            if (dto.Website != null) agency.Website = dto.Website;

            _uow.Agencies.Update(agency);

            if (dto.Address != null)
            {
                var incoming = new AgencyAddress
                {
                    Line1 = dto.Address.Line1,
                    Line2 = dto.Address.Line2,
                    Locality = dto.Address.Locality,
                    City = dto.Address.City ?? string.Empty,
                    LGA = dto.Address.LGA,
                    State = dto.Address.State,
                    Country = dto.Address.Country,
                    PostalCode = dto.Address.PostalCode
                };
                await _uow.AgencyAddresses.UpsertAsync(agencyId, incoming);
            }

            await _uow.SaveChangesAsync();
            var updated = await _uow.Agencies.GetWithDetailsAsync(agencyId);
            return Result<AgencyDto>.Ok(ToDto(updated!));
        }

        public async Task<Result<PagedResult<CaregiverSummaryDto>>> GetAgencyCaregiversAsync(
            Guid agencyId, int page, int size)
        {
            if (!await _uow.Agencies.AnyAsync(a => a.Id == agencyId && !a.IsDeleted))
                return Result<PagedResult<CaregiverSummaryDto>>.Fail("Agency not found");

            var (items, total) = await _uow.Agencies.GetCaregiversPagedAsync(agencyId, page, size);
            var dtos = _mapper.Map<List<CaregiverSummaryDto>>(items);
            return Result<PagedResult<CaregiverSummaryDto>>.Ok(
                new PagedResult<CaregiverSummaryDto>(dtos, total, page, size));
        }

        public async Task<Result> RemoveCaregiverAsync(Guid agencyId, Guid caregiverUserId, Guid requestingUserId)
        {
            var agency = await _uow.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId && !a.IsDeleted);
            if (agency == null)
                return Result.Fail("Agency not found");
            if (agency.AgencyAdminUserId != requestingUserId)
                return Result.Fail("Unauthorized");

            var profile = await _uow.CaregiverProfiles.FirstOrDefaultAsync(
                p => p.UserId == caregiverUserId && p.AgencyId == agencyId && !p.IsDeleted);
            if (profile == null)
                return Result.Fail("Caregiver not found in this agency");

            var hasActive = await _uow.Bookings.AnyAsync(b =>
                b.CaregiverProfileId == profile.Id &&
                (b.Status == BookingStatus.Accepted || b.Status == BookingStatus.InProgress));
            if (hasActive)
                return Result.Fail("Caregiver has active bookings and cannot be removed");

            _uow.CaregiverProfiles.SoftDelete(profile);
            await _uow.SaveChangesAsync();

            await _notification.SendAsync(caregiverUserId, "Agency Membership",
                $"You have been removed from {agency.AgencyName}.", "system", null);

            return Result.Ok();
        }

        public async Task<Result<PagedResult<BookingDto>>> GetAgencyBookingsAsync(
            Guid agencyId, int page, int size)
        {
            if (!await _uow.Agencies.AnyAsync(a => a.Id == agencyId && !a.IsDeleted))
                return Result<PagedResult<BookingDto>>.Fail("Agency not found");

            var (items, total) = await _uow.Bookings.GetAgencyBookingsAsync(agencyId, page, size);
            var dtos = _mapper.Map<List<BookingDto>>(items);
            return Result<PagedResult<BookingDto>>.Ok(
                new PagedResult<BookingDto>(dtos, total, page, size));
        }

        public async Task<Result> VerifyAgencyAsync(Guid agencyId, VerifyAgencyDto dto, Guid adminUserId)
        {
            var agency = await _uow.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId && !a.IsDeleted);
            if (agency == null)
                return Result.Fail("Agency not found");

            if (dto.IsApproved)
            {
                if (agency.VerificationStatus == VerificationStatus.Approved)
                    return Result.Fail("Agency is already approved");

                agency.VerificationStatus = VerificationStatus.Approved;
                agency.VerifiedAt = DateTime.UtcNow;
                _uow.Agencies.Update(agency);
                await _uow.SaveChangesAsync();

                await _notification.SendAsync(agency.AgencyAdminUserId, "Agency Approved",
                    $"Your agency '{agency.AgencyName}' has been approved. You can now add caregivers.",
                    "system", null);
            }
            else
            {
                if (agency.VerificationStatus == VerificationStatus.Rejected)
                    return Result.Fail("Agency is already rejected");
                if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                    return Result.Fail("Rejection reason is required");

                agency.VerificationStatus = VerificationStatus.Rejected;
                _uow.Agencies.Update(agency);
                await _uow.SaveChangesAsync();

                await _notification.SendAsync(agency.AgencyAdminUserId, "Agency Verification",
                    $"Your agency '{agency.AgencyName}' was not approved. Reason: {dto.RejectionReason}",
                    "system", null);
            }

            return Result.Ok();
        }

        // Strips the caregivers collection — callers use the dedicated /caregivers endpoint for paged results.
        private AgencyDto ToDto(Agency agency)
        {
            var dto = _mapper.Map<AgencyDto>(agency);
            dto.Caregivers = new();
            return dto;
        }
    }
}
