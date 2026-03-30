using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IAgencyService
    {
        Task<Result<AgencyDto>> RegisterAgencyAsync(RegisterAgencyDto dto, Guid agencyAdminUserId);
        Task<Result<AgencyDto>> GetAgencyAsync(Guid agencyId);
        Task<Result<AgencyDto>> UpdateAgencyAsync(Guid agencyId, UpdateAgencyDto dto, Guid requestingUserId);
        Task<Result<PagedResult<CaregiverSummaryDto>>> GetAgencyCaregiversAsync(Guid agencyId, int page, int size);
        Task<Result> RemoveCaregiverAsync(Guid agencyId, Guid caregiverUserId, Guid requestingUserId);
        Task<Result<PagedResult<BookingDto>>> GetAgencyBookingsAsync(Guid agencyId, int page, int size);
        Task<Result> VerifyAgencyAsync(Guid agencyId, VerifyAgencyDto dto, Guid adminUserId);
    }
}
