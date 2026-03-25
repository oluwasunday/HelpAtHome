using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IAgencyAddressRepository : IGenericRepository<AgencyAddress>
    {
        Task<AgencyAddress?> GetByAgencyIdAsync(Guid agencyId);
        Task UpsertAsync(Guid agencyId, AgencyAddress address);
    }
}
