using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── InviteFamilyMemberDto ─────────────────────────────────────────────
    public class InviteFamilyMemberDto
    {
        public string PhoneOrEmail { get; set; } = string.Empty;
        public AccessLevel AccessLevel { get; set; } = AccessLevel.ViewOnly;
        public bool ReceiveEmergencyAlerts { get; set; } = true;
        public bool ReceiveBookingUpdates { get; set; } = true;
        public bool ReceivePaymentAlerts { get; set; } = false;
    }

}
