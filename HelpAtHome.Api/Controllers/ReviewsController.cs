using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Reviews and ratings — submit, view, flag, and moderate reviews.</summary>
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    [Produces("application/json")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>Submit a review for a completed booking. Clients review caregivers; caregivers review clients.</summary>
        [HttpPost]
        [Authorize(Roles = "Client,IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _reviewService.CreateReviewAsync(userId, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return StatusCode(StatusCodes.Status201Created, res.Data);
        }

        /// <summary>Get all visible reviews for a caregiver profile. No authentication required.</summary>
        [HttpGet("caregiver/{caregiverProfileId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCaregiverReviews(Guid caregiverProfileId, int page = 1, int size = 10)
        {
            var res = await _reviewService.GetCaregiverReviewsAsync(caregiverProfileId, page, size);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        /// <summary>Get a single review by ID.</summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReview(Guid id)
        {
            var res = await _reviewService.GetReviewAsync(id);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        /// <summary>Flag a review as inappropriate. Any authenticated user can flag.</summary>
        [HttpPost("{id}/flag")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FlagReview(Guid id, [FromBody] FlagReviewDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _reviewService.FlagReviewAsync(userId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Review flagged for moderation." });
        }

        /// <summary>Get all flagged reviews awaiting moderation. Admin only.</summary>
        [HttpGet("flagged")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFlaggedReviews(int page = 1, int size = 20)
        {
            var res = await _reviewService.GetFlaggedReviewsAsync(page, size);
            return Ok(res.Data);
        }

        /// <summary>Hide or unhide a review and add an admin note. Admin only.</summary>
        [HttpPatch("{id}/moderate")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ModerateReview(Guid id, [FromBody] ModerateReviewDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _reviewService.ModerateReviewAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = dto.Hide ? "Review hidden." : "Review made visible." });
        }
    }
}
