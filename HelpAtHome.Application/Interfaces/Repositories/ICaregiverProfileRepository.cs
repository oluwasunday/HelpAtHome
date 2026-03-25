using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface ICaregiverProfileRepository : IGenericRepository<CaregiverProfile>
    {
        //Task<PagedResult<CaregiverProfile>> SearchCaregiversAsync(
        //    string? city, string? state, Guid? serviceCategoryId,
        //    BadgeLevel? minBadge, decimal? maxRate,
        //    GenderPreference? gender, int page, int pageSize);
        Task<CaregiverProfile?> GetWithDocumentsAsync(Guid id);
        Task<CaregiverProfile?> GetByUserIdAsync(Guid userId);
        Task UpdateBadgeAsync(Guid caregiverProfileId);
    }
}
