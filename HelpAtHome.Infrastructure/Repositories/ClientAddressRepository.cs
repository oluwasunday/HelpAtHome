using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class ClientAddressRepository : GenericRepository<ClientAddress>, IClientAddressRepository
    {
        private readonly AppDbContext _ctx;

        public ClientAddressRepository(AppDbContext ctx) : base(ctx) 
        {
            _ctx = ctx;
        }

        public async Task<ClientAddress?> GetByClientProfileIdAsync(Guid clientProfileId)
            => await _ctx.ClientAddresses
                .FirstOrDefaultAsync(a => a.ClientProfileId == clientProfileId);

        // Creates if none exists, updates in-place if one does.
        public async Task UpsertAsync(Guid clientProfileId, ClientAddress incoming)
        {
            var existing = await GetByClientProfileIdAsync(clientProfileId);
            if (existing == null)
            {
                incoming.ClientProfileId = clientProfileId;
                await _ctx.ClientAddresses.AddAsync(incoming);
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
                existing.Latitude = incoming.Latitude;
                existing.Longitude = incoming.Longitude;
                existing.UpdatedAt = DateTime.UtcNow;
                _ctx.ClientAddresses.Update(existing);
            }
        }
    }

}
