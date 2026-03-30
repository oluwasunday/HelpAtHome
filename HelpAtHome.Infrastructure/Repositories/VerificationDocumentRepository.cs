using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class VerificationDocumentRepository : GenericRepository<VerificationDocument>, IVerificationDocumentRepository
    {
        private readonly AppDbContext _ctx;

        public VerificationDocumentRepository(AppDbContext context) : base(context) => _ctx = context;

        public async Task<(IEnumerable<VerificationDocument> Items, int Total)> GetPendingPagedAsync(int page, int size)
        {
            var query = _ctx.VerificationDocuments
                .Where(d => d.Status == VerificationStatus.Pending && !d.IsDeleted)
                .Include(d => d.CaregiverProfile).ThenInclude(c => c!.User)
                .Include(d => d.Agency);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, total);
        }
    }
}
