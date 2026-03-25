using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class VerificationDocumentRepository : GenericRepository<VerificationDocument>, IVerificationDocumentRepository
    {
        private readonly AppDbContext _ctx;
        public VerificationDocumentRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
