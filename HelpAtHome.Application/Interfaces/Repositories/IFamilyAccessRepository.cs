using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IFamilyAccessRepository : IGenericRepository<FamilyAccess>
    {
        Task<FamilyAccess?> GetWithUsersAsync(Guid accessId);
        Task<List<FamilyAccess>> GetByClientUserIdAsync(Guid clientUserId);
        Task<List<FamilyAccess>> GetByFamilyMemberUserIdAsync(Guid familyMemberUserId);
        Task<FamilyAccess?> GetByPairAsync(Guid clientUserId, Guid familyMemberUserId);
    }
}
