using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class AgencyRepository : GenericRepository<Agency>, IAgencyRepository
    {
        private readonly AppDbContext _ctx;
        public AgencyRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
