using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HelpAtHome.Application.Services
{
    public class CaregiverProfileService : ICaregiverService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        private static readonly DistributedCacheEntryOptions SearchCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        private static readonly DistributedCacheEntryOptions ProfileCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        public CaregiverProfileService(IUnitOfWork uow, IMapper mapper, IDistributedCache cache)
        {
            _uow   = uow;
            _mapper = mapper;
            _cache  = cache;
        }

        // ── Search ───────────────────────────────────────────────────────────
        public async Task<Result<PagedResult<CaregiverSummaryDto>>> SearchAsync(CaregiverSearchDto filter)
        {
            var cacheKey = $"caregiver:search:{JsonSerializer.Serialize(filter)}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var cachedResult = JsonSerializer.Deserialize<PagedResult<CaregiverSummaryDto>>(cached);
                return Result<PagedResult<CaregiverSummaryDto>>.Ok(cachedResult!);
            }

            var (items, total) = await _uow.CaregiverProfiles.SearchAsync(filter);
            var dtos    = _mapper.Map<List<CaregiverSummaryDto>>(items);
            var paged   = new PagedResult<CaregiverSummaryDto>(dtos, total, filter.Page, filter.PageSize);

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(paged), SearchCacheOptions);

            return Result<PagedResult<CaregiverSummaryDto>>.Ok(paged);
        }

        // ── Get full profile ─────────────────────────────────────────────────
        public async Task<Result<CaregiverProfileDto>> GetProfileAsync(Guid caregiverId)
        {
            var cacheKey = $"caregiver:profile:{caregiverId}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var cachedDto = JsonSerializer.Deserialize<CaregiverProfileDto>(cached);
                return Result<CaregiverProfileDto>.Ok(cachedDto!);
            }

            var profile = await _uow.CaregiverProfiles.GetWithDocumentsAsync(caregiverId);
            if (profile == null)
                return Result<CaregiverProfileDto>.Fail("Caregiver not found.");

            var dto = _mapper.Map<CaregiverProfileDto>(profile);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), ProfileCacheOptions);

            return Result<CaregiverProfileDto>.Ok(dto);
        }

        // ── Update profile ───────────────────────────────────────────────────
        public async Task<Result<CaregiverProfileDto>> UpdateProfileAsync(Guid userId, UpdateCaregiverProfileDto dto)
        {
            var profile = await _uow.CaregiverProfiles.GetByUserIdAsync(userId);
            if (profile == null)
                return Result<CaregiverProfileDto>.Fail("Caregiver profile not found.");

            if (dto.Bio               != null) profile.Bio               = dto.Bio;
            if (dto.YearsOfExperience != null) profile.YearsOfExperience = dto.YearsOfExperience.Value;
            if (dto.HourlyRate        != null) profile.HourlyRate        = dto.HourlyRate.Value;
            if (dto.DailyRate         != null) profile.DailyRate         = dto.DailyRate.Value;
            if (dto.MonthlyRate       != null) profile.MonthlyRate       = dto.MonthlyRate.Value;
            if (dto.Gender            != null) profile.GenderProvided    = dto.Gender.Value;
            if (dto.LanguagesSpoken   != null) profile.LanguagesSpoken   = dto.LanguagesSpoken;
            if (dto.WorkingHours      != null) profile.WorkingHours      = dto.WorkingHours;

            if (dto.Address != null)
            {
                if (profile.Address == null)
                    profile.Address = _mapper.Map<CaregiverAddress>(dto.Address);
                else
                    _mapper.Map(dto.Address, profile.Address);
            }

            _uow.CaregiverProfiles.Update(profile);
            await _uow.SaveChangesAsync();

            await _cache.RemoveAsync($"caregiver:profile:{profile.Id}");

            return Result<CaregiverProfileDto>.Ok(_mapper.Map<CaregiverProfileDto>(profile));
        }

        // ── Pending verification (admin) ─────────────────────────────────────
        public async Task<Result<PagedResult<CaregiverSummaryDto>>> GetPendingVerificationAsync(int page, int pageSize)
        {
            var (items, total) = await _uow.CaregiverProfiles.GetPendingVerificationAsync(page, pageSize);
            var dtos  = _mapper.Map<List<CaregiverSummaryDto>>(items);
            var paged = new PagedResult<CaregiverSummaryDto>(dtos, total, page, pageSize);
            return Result<PagedResult<CaregiverSummaryDto>>.Ok(paged);
        }

        // ── Verify caregiver (admin) ─────────────────────────────────────────
        public async Task<Result> VerifyCaregiverAsync(Guid adminId, Guid caregiverId, VerifyCaregiverDto dto)
        {
            var profile = await _uow.CaregiverProfiles.GetByIdAsync(caregiverId);
            if (profile == null)
                return Result.Fail("Caregiver not found.");

            if (dto.IsApproved)
            {
                profile.VerificationStatus = VerificationStatus.Approved;
                profile.VerifiedAt         = DateTime.UtcNow;
                profile.RejectionReason    = null;
            }
            else
            {
                profile.VerificationStatus = VerificationStatus.Rejected;
                profile.RejectionReason    = dto.RejectionReason;
            }

            profile.VerifiedByAdminId = adminId.ToString();
            _uow.CaregiverProfiles.Update(profile);
            await _uow.SaveChangesAsync();

            await _cache.RemoveAsync($"caregiver:profile:{profile.Id}");

            return Result.Ok();
        }

        // ── Toggle availability ──────────────────────────────────────────────
        public async Task<Result<bool>> ToggleAvailabilityAsync(Guid userId)
        {
            var profile = await _uow.CaregiverProfiles.GetByUserIdAsync(userId);
            if (profile == null)
                return Result<bool>.Fail("Caregiver profile not found.");

            profile.IsAvailable = !profile.IsAvailable;
            _uow.CaregiverProfiles.Update(profile);
            await _uow.SaveChangesAsync();

            await _cache.RemoveAsync($"caregiver:profile:{profile.Id}");

            return Result<bool>.Ok(profile.IsAvailable);
        }
    }
}
