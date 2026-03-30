using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IAgencyRepository : IGenericRepository<Agency>
    {
        Task<bool> RegistrationNumberExistsAsync(string registrationNumber);
        Task<Agency?> GetWithDetailsAsync(Guid agencyId);
        Task<(IEnumerable<CaregiverProfile> Items, int Total)> GetCaregiversPagedAsync(Guid agencyId, int page, int size);
    }
}
