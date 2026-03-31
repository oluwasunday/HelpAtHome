using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<(List<Review> Items, int Total)> GetForCaregiverAsync(Guid caregiverUserId, int page, int size);
        Task<(List<Review> Items, int Total)> GetFlaggedAsync(int page, int size);
        Task<List<Review>> GetByBookingAsync(Guid bookingId);
        Task<(decimal AverageRating, int TotalReviews)> GetRatingStatsAsync(Guid caregiverUserId);
    }
}
