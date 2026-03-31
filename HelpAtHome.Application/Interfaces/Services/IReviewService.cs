using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<Result<ReviewDto>> CreateReviewAsync(Guid reviewerUserId, CreateReviewDto dto);
        Task<Result<PagedResult<ReviewDto>>> GetCaregiverReviewsAsync(Guid caregiverProfileId, int page, int size);
        Task<Result<ReviewDto>> GetReviewAsync(Guid reviewId);
        Task<Result> FlagReviewAsync(Guid userId, Guid reviewId, FlagReviewDto dto);
        Task<Result> ModerateReviewAsync(Guid adminId, Guid reviewId, ModerateReviewDto dto);
        Task<Result<PagedResult<ReviewDto>>> GetFlaggedReviewsAsync(int page, int size);
    }
}
