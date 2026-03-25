using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class ClientProfileRepository : GenericRepository<ClientProfile>, IClientProfileRepository
    {
        private readonly AppDbContext _ctx;
        public ClientProfileRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public Task<ClientProfile> GetByUserIdAsync(Guid userId)
        {
            return Task.FromResult(_ctx.ClientProfiles.FirstOrDefault(cp => cp.UserId == userId));
        }
    }
}
