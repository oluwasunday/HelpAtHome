using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── CaregiverSearchDto (query parameters) ────────────────────────────
    public class CaregiverSearchDto
    {
        public string? State { get; set; }
        public string? City { get; set; }
        public string? LGA { get; set; }
        public Guid? ServiceCategoryId { get; set; }
        public BadgeLevel? MinBadge { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public Gender? Gender { get; set; }
        public bool? IsAvailable { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
