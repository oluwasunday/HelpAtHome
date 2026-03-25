using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.DTOs.Requests;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── CaregiverProfileDto (full detail view) ───────────────────────────
    public class CaregiverProfileDto : CaregiverSummaryDto
    {
        public bool CanCook { get; set; }
        public bool CanDriveClient { get; set; }
        public bool CanAdministerMedication { get; set; }
        public bool CanDoHeavyCleaning { get; set; }
        public bool CanDoErrands { get; set; }
        public bool CanProvideCompanionship { get; set; }
        public bool CanCareForBedridden { get; set; }
        public bool HasFirstAidCertificate { get; set; }
        public bool IsBackgroundChecked { get; set; }
        public string? LanguagesSpoken { get; set; }
        public string? WorkingHours { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VerificationDocumentDto> Documents { get; set; } = new();
        public List<ReviewDto> RecentReviews { get; set; } = new();
        public AddressDto? Address { get; set; }
    }
}
