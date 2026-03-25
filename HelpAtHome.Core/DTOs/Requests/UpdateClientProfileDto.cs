using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateClientProfileDto ───────────────────────────────────────────
    public class UpdateClientProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? LGA { get; set; }
        public string? SpecialNotes { get; set; }
        public string? MedicalConditions { get; set; }
        public int? ClientAge { get; set; }
        public Gender? CaregiverGender { get; set; }
        public bool? RequireVerifiedOnly { get; set; }
    }

}
