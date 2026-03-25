using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateFamilyAccessDto ─────────────────────────────────────────────
    public class UpdateFamilyAccessDto
    {
        public AccessLevel? AccessLevel { get; set; }
        public bool? ReceiveEmergencyAlerts { get; set; }
        public bool? ReceiveBookingUpdates { get; set; }
        public bool? ReceivePaymentAlerts { get; set; }
    }

}
