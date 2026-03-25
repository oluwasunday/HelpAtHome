using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── ClientProfileDto ─────────────────────────────────────────────────
    public class ClientProfileDto
    {
        public Guid Id { get; set; }
        public UserSummaryDto User { get; set; } = null!;
        public string? SpecialNotes { get; set; }
        public string? MedicalConditions { get; set; }
        public int? ClientAge { get; set; }
        public string CaregiverGenderPreference { get; set; } = string.Empty;
        public bool RequireVerifiedOnly { get; set; }
        public decimal WalletBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public AddressDto? Address { get; set; }
    }
}
