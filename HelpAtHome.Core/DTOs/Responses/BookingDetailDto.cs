namespace HelpAtHome.Core.DTOs.Responses
{
    // ── BookingDetailDto (full detail view) ──────────────────────────────
    public class BookingDetailDto : BookingDto
    {
        public decimal PlatformFee { get; set; }
        public decimal CaregiverEarnings { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? ClientAddress { get; set; }
        public decimal? ClientLatitude { get; set; }
        public decimal? ClientLongitude { get; set; }
        public TimeSpan? DailyStartTime { get; set; }
        public TimeSpan? DailyEndTime { get; set; }
        public string? CancellationReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsReviewedByClient { get; set; }
        public bool IsReviewedByCaregiver { get; set; }
        public TransactionDto? Transaction { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

}
