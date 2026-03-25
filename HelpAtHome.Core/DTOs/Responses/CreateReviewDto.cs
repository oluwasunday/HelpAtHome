namespace HelpAtHome.Core.DTOs.Responses
{
    // ── CreateReviewDto ──────────────────────────────────────────────────
    public class CreateReviewDto
    {
        public Guid BookingId { get; set; }
        public int Rating { get; set; }   // 1–5
        public string? Comment { get; set; }
    }

}
