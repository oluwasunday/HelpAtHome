using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface ICaregiverProfileRepository : IGenericRepository<CaregiverProfile>
    {
        Task<(IEnumerable<CaregiverProfile> Items, int Total)> SearchAsync(CaregiverSearchDto filter);
        Task<(IEnumerable<CaregiverProfile> Items, int Total)> GetPendingVerificationAsync(int page, int pageSize);
        Task<CaregiverProfile?> GetWithDocumentsAsync(Guid id);
        Task<CaregiverProfile?> GetByUserIdAsync(Guid userId);
        Task UpdateBadgeAsync(Guid caregiverProfileId);
    }
}
