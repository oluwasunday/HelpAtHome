using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<ClientProfile>, IUserRepository
    {
        private readonly AppDbContext _ctx;
        public UserRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public Task<User?> GetByIdWithProfileAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByPhoneAsync(string phone)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PhoneExistsAsync(string phoneNumber)
        {
            throw new NotImplementedException();
        }
    }
}
