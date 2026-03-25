using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class FamilyAccessRepository : GenericRepository<FamilyAccess>, IFamilyAccessRepository
    {
        private readonly AppDbContext _ctx;
        public FamilyAccessRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
