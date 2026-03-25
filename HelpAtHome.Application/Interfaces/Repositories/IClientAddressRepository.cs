using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IClientAddressRepository : IGenericRepository<ClientAddress>
    {
        Task<ClientAddress?> GetByClientProfileIdAsync(Guid clientProfileId);
        Task UpsertAsync(Guid clientProfileId, ClientAddress address);
    }
}
