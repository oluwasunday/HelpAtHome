using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly AppDbContext _ctx;
        public ReviewRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<(List<Review> Items, int Total)> GetForCaregiverAsync(Guid caregiverUserId, int page, int size)
        {
            var query = _ctx.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Booking)
                .Where(r => r.RevieweeUserId == caregiverUserId && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (items, total);
        }

        public async Task<(List<Review> Items, int Total)> GetFlaggedAsync(int page, int size)
        {
            var query = _ctx.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Booking)
                .Where(r => r.IsFlagged)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (items, total);
        }

        public async Task<List<Review>> GetByBookingAsync(Guid bookingId)
        {
            return await _ctx.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<(decimal AverageRating, int TotalReviews)> GetRatingStatsAsync(Guid caregiverUserId)
        {
            var reviews = await _ctx.Reviews
                .Where(r => r.RevieweeUserId == caregiverUserId && r.IsByClient && r.IsVisible)
                .ToListAsync();

            if (reviews.Count == 0)
                return (0m, 0);

            var avg = Math.Round((decimal)reviews.Average(r => r.Rating), 1);
            return (avg, reviews.Count);
        }
    }
}
