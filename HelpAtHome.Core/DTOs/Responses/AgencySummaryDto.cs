using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── AgencySummaryDto ─────────────────────────────────────────────────
    public class AgencySummaryDto
    {
        public Guid Id { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public int TotalCaregiversCount { get; set; }
        public bool IsActive { get; set; }
        public AddressDto? Address { get; set; }
    }
}
