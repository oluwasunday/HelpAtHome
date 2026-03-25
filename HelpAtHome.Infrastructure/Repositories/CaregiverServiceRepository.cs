using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class CaregiverServiceRepository : GenericRepository<CaregiverService>, ICaregiverServiceRepository
    {
        private readonly AppDbContext _ctx;
        public CaregiverServiceRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
