using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests.Auth
{
    // ── RegisterAgencyCaregiverDto ───────────────────────────────────────
    public class RegisterAgencyCaregiverDto
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
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "HourlyRate must be greater than zero")]
        public decimal HourlyRate { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "DailyRate must be greater than zero")]
        public decimal DailyRate { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "MonthlyRate must be greater than zero")]
        public decimal MonthlyRate { get; set; }
        public Gender Gender { get; set; }
        public List<Guid> ServiceCategoryIds { get; set; } = new();

        [Required]
        public AddressUpsertDto Address { get; set; } = new();
    }

}
