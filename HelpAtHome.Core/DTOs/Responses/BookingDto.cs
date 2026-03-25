using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── BookingDto (list / summary view) ─────────────────────────────────
    public class BookingDto
    {
        public Guid Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime ScheduledStartDate { get; set; }
        public DateTime ScheduledEndDate { get; set; }
        public decimal AgreedAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool HasDispute { get; set; }
        public string ServiceCategory { get; set; } = string.Empty;
        public CaregiverSummaryDto Caregiver { get; set; } = null!;
        public UserSummaryDto Client { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}
