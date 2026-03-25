using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IClientProfileRepository : IGenericRepository<ClientProfile>
    {
        Task<ClientProfile> GetByUserIdAsync(Guid userId);
    }
}
