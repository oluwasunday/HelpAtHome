using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.DTOs.Requests;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── AgencyDto (full detail) ───────────────────────────────────────────
    public class AgencyDto : AgencySummaryDto
    {
        public string? Description { get; set; }
        public string? Website { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal AgencyCommissionRate { get; set; }
        public decimal WalletBalance { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserSummaryDto AgencyAdmin { get; set; } = null!;
        public List<CaregiverSummaryDto> Caregivers { get; set; } = new();
        public List<VerificationDocumentDto> Documents { get; set; } = new();
        public AddressDto? Address { get; set; }
    }
}
