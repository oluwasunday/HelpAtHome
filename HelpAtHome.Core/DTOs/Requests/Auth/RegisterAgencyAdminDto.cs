using HelpAtHome.Core.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests.Auth
{
    // ── RegisterAgencyAdminDto ───────────────────────────────────────────
    public class RegisterAgencyAdminDto
    {
        // Admin personal info
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

        // Agency details
        [Required] 
        public string AgencyName { get; set; } = string.Empty;
        [Required] 
        public string RegistrationNumber { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string AgencyEmail { get; set; } = string.Empty;
        [Required] public string AgencyPhone { get; set; } = string.Empty;
        public string? AgencyDescription { get; set; }
        public string? Website { get; set; }

        // ── Agency address (replaces flat fields) ────────────────────────
        [Required]
        public AddressUpsertDto AgencyAddress { get; set; } = new();
    }

}
