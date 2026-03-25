using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IAgencyRepository : IGenericRepository<Agency>
    {

        public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber)
        {
            return false;
        }
    }
}
