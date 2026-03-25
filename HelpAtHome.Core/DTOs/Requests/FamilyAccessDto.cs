using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── FamilyAccessDto ───────────────────────────────────────────────────
    public class FamilyAccessDto
    {
        public Guid Id { get; set; }
        public UserSummaryDto Client { get; set; } = null!;
        public UserSummaryDto FamilyMember { get; set; } = null!;
        public string AccessLevel { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public bool ReceiveEmergencyAlerts { get; set; }
        public bool ReceiveBookingUpdates { get; set; }
        public bool ReceivePaymentAlerts { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
