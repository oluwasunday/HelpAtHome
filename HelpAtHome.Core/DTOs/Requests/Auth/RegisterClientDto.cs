using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests.Auth
{
    public class RegisterClientDto
    {
        [Required] 
        public string FirstName { get; set; } = string.Empty;
        [Required] 
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required] 
        public string PhoneNumber { get; set; } = string.Empty;
        [Required] 
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public Services ServicesNeeded { get; set; }
        public Gender Gender { get; set; }
        public Frequency Frequency { get; set; } = Frequency.None;
        public PreferedGender CareGiverGenderPreference { get; set; } = PreferedGender.NoPreference;
        public RelationToRecipient RelationToRecipient { get; set; } = RelationToRecipient.Self;
        public string MedicalConditions { get; set; }
        public string SpecialNotes { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool RequireVerifiedOnly { get; set; }

        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhoneNumber { get; set; }

        // ── Nested address (required at registration) ────────────────────
        [Required]
        public AddressUpsertDto Address { get; set; } = new();
    }

}
