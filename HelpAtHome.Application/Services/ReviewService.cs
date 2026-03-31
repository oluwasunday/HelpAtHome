using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<ReviewDto>> CreateReviewAsync(Guid reviewerUserId, CreateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return Result<ReviewDto>.Fail("Rating must be between 1 and 5.");

            var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId);
            if (booking == null)
                return Result<ReviewDto>.Fail("Booking not found.");

            if (booking.Status != BookingStatus.Completed)
                return Result<ReviewDto>.Fail("Reviews can only be submitted for completed bookings.");

            // Determine who is reviewing whom
            var clientProfile = await _uow.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == reviewerUserId);
            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == reviewerUserId);

            bool isByClient;
            Guid revieweeUserId;

            if (clientProfile != null && booking.ClientProfileId == clientProfile.Id)
            {
                // Client reviewing caregiver
                isByClient = true;
                if (booking.IsReviewedByClient)
                    return Result<ReviewDto>.Fail("You have already reviewed this booking.");

                var caregiverOfBooking = await _uow.CaregiverProfiles.GetByIdAsync(booking.CaregiverProfileId);
                if (caregiverOfBooking == null)
                    return Result<ReviewDto>.Fail("Caregiver not found.");

                revieweeUserId = caregiverOfBooking.UserId;
            }
            else if (caregiverProfile != null && booking.CaregiverProfileId == caregiverProfile.Id)
            {
                // Caregiver reviewing client
                isByClient = false;
                if (booking.IsReviewedByCaregiver)
                    return Result<ReviewDto>.Fail("You have already reviewed this booking.");

                var clientOfBooking = await _uow.ClientProfiles.GetByIdAsync(booking.ClientProfileId);
                if (clientOfBooking == null)
                    return Result<ReviewDto>.Fail("Client not found.");

                revieweeUserId = clientOfBooking.UserId;
            }
            else
            {
                return Result<ReviewDto>.Fail("You are not a participant in this booking.");
            }

            var review = new Review
            {
                Id = Guid.NewGuid(),
                BookingId = dto.BookingId,
                ReviewerUserId = reviewerUserId,
                RevieweeUserId = revieweeUserId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                IsByClient = isByClient,
                IsVisible = true,
                IsFlagged = false
            };

            await _uow.Reviews.AddAsync(review);

            // Mark booking as reviewed
            if (isByClient)
                booking.IsReviewedByClient = true;
            else
                booking.IsReviewedByCaregiver = true;
            _uow.Bookings.Update(booking);

            // Update caregiver average rating (only client→caregiver reviews count)
            if (isByClient)
            {
                var stats = await _uow.Reviews.GetRatingStatsAsync(revieweeUserId);
                // Include the new review in the calc (not yet saved, so compute inline)
                var existingCount = stats.TotalReviews;
                var existingSum = stats.AverageRating * existingCount;
                var newTotal = existingCount + 1;
                var newAvg = Math.Round((existingSum + dto.Rating) / newTotal, 1);

                var cp = await _uow.CaregiverProfiles.FirstOrDefaultAsync(c => c.UserId == revieweeUserId);
                if (cp != null)
                {
                    cp.AverageRating = newAvg;
                    cp.TotalReviews = (int)newTotal;
                    _uow.CaregiverProfiles.Update(cp);
                }
            }

            await _uow.SaveChangesAsync();

            var saved = await _uow.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id);
            // Load navigation for mapping
            var bookingNav = await _uow.Bookings.GetByIdAsync(review.BookingId);
            saved!.Booking = bookingNav!;
            var reviewer = await _uow.Users.GetByIdWithProfileAsync(reviewerUserId);
            saved.Reviewer = reviewer!;

            return Result<ReviewDto>.Ok(_mapper.Map<ReviewDto>(saved));
        }

        public async Task<Result<PagedResult<ReviewDto>>> GetCaregiverReviewsAsync(
            Guid caregiverProfileId, int page, int size)
        {
            var profile = await _uow.CaregiverProfiles.GetByIdAsync(caregiverProfileId);
            if (profile == null)
                return Result<PagedResult<ReviewDto>>.Fail("Caregiver not found.");

            var (items, total) = await _uow.Reviews.GetForCaregiverAsync(profile.UserId, page, size);
            var dtos = _mapper.Map<List<ReviewDto>>(items);
            return Result<PagedResult<ReviewDto>>.Ok(new PagedResult<ReviewDto>(dtos, total, page, size));
        }

        public async Task<Result<ReviewDto>> GetReviewAsync(Guid reviewId)
        {
            var review = await _uow.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
                return Result<ReviewDto>.Fail("Review not found.");
            if (!review.IsVisible)
                return Result<ReviewDto>.Fail("This review is not visible.");

            var booking = await _uow.Bookings.GetByIdAsync(review.BookingId);
            review.Booking = booking!;
            var reviewer = await _uow.Users.GetByIdWithProfileAsync(review.ReviewerUserId);
            review.Reviewer = reviewer!;

            return Result<ReviewDto>.Ok(_mapper.Map<ReviewDto>(review));
        }

        public async Task<Result> FlagReviewAsync(Guid userId, Guid reviewId, FlagReviewDto dto)
        {
            var review = await _uow.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
                return Result.Fail("Review not found.");
            if (!review.IsVisible)
                return Result.Fail("Review is not visible.");
            if (review.IsFlagged)
                return Result.Fail("Review has already been flagged.");

            review.IsFlagged = true;
            review.AdminNote = $"Flagged by user {userId}: {dto.Reason}";
            _uow.Reviews.Update(review);
            await _uow.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> ModerateReviewAsync(Guid adminId, Guid reviewId, ModerateReviewDto dto)
        {
            var review = await _uow.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
                return Result.Fail("Review not found.");

            review.IsVisible = !dto.Hide;
            review.IsFlagged = false;
            if (!string.IsNullOrWhiteSpace(dto.AdminNote))
                review.AdminNote = dto.AdminNote;
            _uow.Reviews.Update(review);

            // If hiding a client→caregiver review, recalculate caregiver rating
            if (dto.Hide && review.IsByClient)
            {
                var stats = await _uow.Reviews.GetRatingStatsAsync(review.RevieweeUserId);
                var cp = await _uow.CaregiverProfiles.FirstOrDefaultAsync(c => c.UserId == review.RevieweeUserId);
                if (cp != null)
                {
                    cp.AverageRating = stats.AverageRating;
                    cp.TotalReviews = stats.TotalReviews;
                    _uow.CaregiverProfiles.Update(cp);
                }
            }

            await _uow.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result<PagedResult<ReviewDto>>> GetFlaggedReviewsAsync(int page, int size)
        {
            var (items, total) = await _uow.Reviews.GetFlaggedAsync(page, size);
            var dtos = _mapper.Map<List<ReviewDto>>(items);
            return Result<PagedResult<ReviewDto>>.Ok(new PagedResult<ReviewDto>(dtos, total, page, size));
        }
    }
}
