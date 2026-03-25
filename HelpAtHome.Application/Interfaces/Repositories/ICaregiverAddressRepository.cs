using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface ICaregiverAddressRepository : IGenericRepository<CaregiverAddress>
    {
        Task<CaregiverAddress?> GetByCaregiverProfileIdAsync(Guid caregiverProfileId);
        Task UpsertAsync(Guid caregiverProfileId, CaregiverAddress address);
    }
}
