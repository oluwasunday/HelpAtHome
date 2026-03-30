using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IAdminService
    {
        Task<Result<AdminDashboardDto>> GetDashboardAsync();
        Task<Result<PagedResult<AdminUserDto>>> GetUsersAsync(AdminUserFilterDto filter);
        Task<Result<AdminUserDto>> GetUserAsync(Guid userId);
        Task<Result> SuspendUserAsync(Guid adminId, Guid userId, SuspendUserDto dto);
        Task<Result> UnsuspendUserAsync(Guid adminId, Guid userId);
        Task<Result> DeleteUserAsync(Guid adminId, Guid userId);
        Task<Result<PagedResult<VerificationDocumentDto>>> GetPendingDocumentsAsync(int page, int size);
        Task<Result> ReviewDocumentAsync(Guid adminId, Guid documentId, ReviewDocumentDto dto);
        Task<Result> SetAgencyCommissionAsync(Guid adminId, Guid agencyId, SetCommissionDto dto);
        Task<Result<PagedResult<BookingDto>>> GetOpenDisputesAsync(int page, int size);
    }
}
