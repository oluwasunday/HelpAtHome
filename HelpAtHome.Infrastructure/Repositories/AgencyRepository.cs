using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class AgencyRepository : GenericRepository<Agency>, IAgencyRepository
    {
        private readonly AppDbContext _ctx;

        public AgencyRepository(AppDbContext context) : base(context) => _ctx = context;

        public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber)
            => await _ctx.Agencies.AnyAsync(a => a.RegistrationNumber == registrationNumber && !a.IsDeleted);

        public async Task<Agency?> GetWithDetailsAsync(Guid agencyId)
            => await _ctx.Agencies
                .Include(a => a.AgencyAddress)
                .Include(a => a.AgencyAdmin)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == agencyId && !a.IsDeleted);

        public async Task<(IEnumerable<CaregiverProfile> Items, int Total)> GetCaregiversPagedAsync(
            Guid agencyId, int page, int size)
        {
            var query = _ctx.CaregiverProfiles
                .Where(c => c.AgencyId == agencyId)
                .Include(c => c.User)
                .Include(c => c.Address)
                .Include(c => c.CaregiverServices).ThenInclude(cs => cs.ServiceCategory);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, total);
        }
    }
}
