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
        public decimal MonthlyRate { get; set; }
        public Gender Gender { get; set; }
        public List<Guid> ServiceCategoryIds { get; set; } = new();

        [Required]
        public AddressUpsertDto Address { get; set; } = new();
    }

}
