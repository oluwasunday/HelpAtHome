using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class AgencyAddressRepository : GenericRepository<AgencyAddress>, IAgencyAddressRepository
    {
        private readonly AppDbContext _ctx;
        public AgencyAddressRepository(AppDbContext ctx) : base(ctx) 
        {
            _ctx = ctx;
        }

        public async Task<AgencyAddress?> GetByAgencyIdAsync(Guid agencyId)
        {
            return await _ctx.AgencyAddresses
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId);
        }

        public async Task UpsertAsync(Guid agencyId, AgencyAddress incoming)
        {
            var existing = await GetByAgencyIdAsync(agencyId);
            if (existing == null)
            {
                incoming.AgencyId = agencyId;
                await _ctx.AgencyAddresses.AddAsync(incoming);
            }
            else
            {
                existing.Line1 = incoming.Line1;
                existing.Line2 = incoming.Line2;
                existing.Locality = incoming.Locality;
                existing.City = incoming.City;
                existing.LGA = incoming.LGA;
                existing.State = incoming.State;
                existing.Country = incoming.Country;
                existing.PostalCode = incoming.PostalCode;
                existing.UpdatedAt = DateTime.UtcNow;
                _ctx.AgencyAddresses.Update(existing);
            }
        }
    }
}
