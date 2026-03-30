using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface ICaregiverService
    {
        Task<Result<PagedResult<CaregiverSummaryDto>>> SearchAsync(CaregiverSearchDto filter);
        Task<Result<CaregiverProfileDto>> GetProfileAsync(Guid caregiverId);
        Task<Result<CaregiverProfileDto>> UpdateProfileAsync(Guid userId, UpdateCaregiverProfileDto dto);
        Task<Result<PagedResult<CaregiverSummaryDto>>> GetPendingVerificationAsync(int page, int pageSize);
        Task<Result> VerifyCaregiverAsync(Guid adminId, Guid caregiverId, VerifyCaregiverDto dto);
        Task<Result<bool>> ToggleAvailabilityAsync(Guid userId);
    }
}
