using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── ReviewDto ────────────────────────────────────────────────────────
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string BookingRef { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsByClient { get; set; }
        public UserSummaryDto Reviewer { get; set; } = null!;
        public bool IsFlagged { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
