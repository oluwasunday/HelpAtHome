using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class CaregiverProfileRepository : GenericRepository<CaregiverProfile>, ICaregiverProfileRepository
    {
        private readonly AppDbContext _ctx;
        public CaregiverProfileRepository(AppDbContext context) : base(context) 
        {
            _ctx = context;
        }

        public Task<CaregiverProfile?> GetByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<CaregiverProfile?> GetWithDocumentsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateBadgeAsync(Guid caregiverProfileId)
        {
            throw new NotImplementedException();
        }

        public async Task GetFullProfileAsync(Guid id, VerificationStatus verificationStatus = VerificationStatus.Pending, string state = null, string city = null, string lga = null)
        {
            var result = await _ctx.CaregiverProfiles
                        .Include(cp => cp.User)
                        .Include(cp => cp.Address)    // <-- ADD THIS
                        .Include(cp => cp.CaregiverServices)
                            .ThenInclude(cs => cs.ServiceCategory)
                        .Where(cp => !cp.IsDeleted
                            && cp.IsAvailable
                            && cp.VerificationStatus == verificationStatus
                            // address-based filters now use cp.Address.State etc.
                            && (state == null || cp.Address!.State == state)
                            && (city == null || cp.Address!.City == city)
                            && (lga == null || cp.Address!.LGA == lga)
                        // ... rest of filters unchanged
                        )
                        .OrderByDescending(cp => cp.AverageRating).FirstOrDefaultAsync();
                        //.ToPagedResultAsync(page, pageSize);

        }
    }

}
