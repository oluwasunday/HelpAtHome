using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
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

        public async Task<(IEnumerable<CaregiverProfile> Items, int Total)> SearchAsync(CaregiverSearchDto filter)
        {
            var query = _ctx.CaregiverProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Address)
                .Include(cp => cp.Agency)
                .Include(cp => cp.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Where(cp => !cp.IsDeleted && cp.VerificationStatus == VerificationStatus.Approved);

            if (!string.IsNullOrWhiteSpace(filter.State))
                query = query.Where(cp => cp.Address != null && cp.Address.State == filter.State);

            if (!string.IsNullOrWhiteSpace(filter.City))
                query = query.Where(cp => cp.Address != null && cp.Address.City == filter.City);

            if (!string.IsNullOrWhiteSpace(filter.LGA))
                query = query.Where(cp => cp.Address != null && cp.Address.LGA == filter.LGA);

            if (filter.ServiceCategoryId.HasValue)
                query = query.Where(cp =>
                    cp.CaregiverServices.Any(cs => cs.ServiceCategoryId == filter.ServiceCategoryId.Value));

            if (filter.MinBadge.HasValue)
                query = query.Where(cp => cp.Badge >= filter.MinBadge.Value);

            if (filter.MaxHourlyRate.HasValue)
                query = query.Where(cp => cp.HourlyRate <= filter.MaxHourlyRate.Value);

            if (filter.Gender.HasValue)
                query = query.Where(cp => cp.GenderProvided == filter.Gender.Value);

            if (filter.IsAvailable.HasValue)
                query = query.Where(cp => cp.IsAvailable == filter.IsAvailable.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(cp => cp.AverageRating)
                .ThenByDescending(cp => cp.TotalCompletedBookings)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<(IEnumerable<CaregiverProfile> Items, int Total)> GetPendingVerificationAsync(int page, int pageSize)
        {
            var query = _ctx.CaregiverProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Address)
                .Include(cp => cp.Documents)
                .Where(cp => !cp.IsDeleted && cp.VerificationStatus == VerificationStatus.Pending)
                .OrderBy(cp => cp.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<CaregiverProfile?> GetWithDocumentsAsync(Guid id)
        {
            return await _ctx.CaregiverProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Address)
                .Include(cp => cp.Agency)
                .Include(cp => cp.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Include(cp => cp.Documents)
                .Include(cp => cp.ReviewsReceived.OrderByDescending(r => r.CreatedAt).Take(5))
                .FirstOrDefaultAsync(cp => cp.Id == id && !cp.IsDeleted);
        }

        public async Task<CaregiverProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _ctx.CaregiverProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Address)
                .Include(cp => cp.Agency)
                .Include(cp => cp.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .FirstOrDefaultAsync(cp => cp.UserId == userId && !cp.IsDeleted);
        }

        public async Task UpdateBadgeAsync(Guid caregiverProfileId)
        {
            var profile = await _ctx.CaregiverProfiles.FindAsync(caregiverProfileId);
            if (profile == null) return;

            profile.Badge = profile.TotalCompletedBookings switch
            {
                >= 50 when profile.AverageRating >= 4.8m => BadgeLevel.Champion,
                >= 20 when profile.AverageRating >= 4.5m => BadgeLevel.Elite,
                >= 5                                     => BadgeLevel.Verified,
                _                                        => BadgeLevel.New
            };
        }
    }
}
