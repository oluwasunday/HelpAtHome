using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── CreateBookingDto ─────────────────────────────────────────────────
    public class CreateBookingDto
    {
        public Guid CaregiverProfileId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public FrequencyType Frequency { get; set; }
        public DateTime ScheduledStartDate { get; set; }
        public DateTime ScheduledEndDate { get; set; }
        public TimeSpan? DailyStartTime { get; set; }
        public TimeSpan? DailyEndTime { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

}
