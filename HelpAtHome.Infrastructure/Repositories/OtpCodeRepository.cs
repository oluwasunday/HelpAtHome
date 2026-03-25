using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class OtpCodeRepository : GenericRepository<OtpCode>, IOtpCodeRepository
    {
        private readonly AppDbContext _ctx;
        public OtpCodeRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
