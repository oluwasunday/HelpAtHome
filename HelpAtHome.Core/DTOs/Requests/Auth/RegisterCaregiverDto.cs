using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests.Auth
{
    // ── RegisterCaregiverDto ─────────────────────────────────────────────
    public class RegisterCaregiverDto
    {
        [Required] 
        public string FirstName { get; set; } = string.Empty;
        [Required] 
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        [Required] 
        public string PhoneNumber { get; set; } = string.Empty;
        [Required] 
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal WeeklyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        public Gender Gender { get; set; }
        public Services ServicesToOffer { get; set; }
        public List<string> LanguagesSpoken { get; set; }
        public List<Guid> ServiceCategoryIds { get; set; } = new();

        // verification and safety
        public DocumentType IdType { get; set; }
        public string IdNumber { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhoneNumber { get; set; }

        [Required]
        public AddressUpsertDto Address { get; set; } = new();
    }

}
