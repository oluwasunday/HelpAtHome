using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class CaregiverAddressRepository : GenericRepository<CaregiverAddress>, ICaregiverAddressRepository
    {
        private readonly AppDbContext _ctx;
        public CaregiverAddressRepository(AppDbContext ctx) : base(ctx) 
        {
            _ctx = ctx;
        }

        public async Task<CaregiverAddress?> GetByCaregiverProfileIdAsync(Guid caregiverProfileId)
            => await _ctx.CaregiverAddresses
                .FirstOrDefaultAsync(a => a.CaregiverProfileId == caregiverProfileId);

        public async Task UpsertAsync(Guid caregiverProfileId, CaregiverAddress incoming)
        {
            var existing = await GetByCaregiverProfileIdAsync(caregiverProfileId);
            if (existing == null)
            {
                incoming.CaregiverProfileId = caregiverProfileId;
                await _ctx.CaregiverAddresses.AddAsync(incoming);
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
                existing.AgencyId = incoming.AgencyId;
                existing.UpdatedAt = DateTime.UtcNow;
                _ctx.CaregiverAddresses.Update(existing);
            }
        }
    }

}
