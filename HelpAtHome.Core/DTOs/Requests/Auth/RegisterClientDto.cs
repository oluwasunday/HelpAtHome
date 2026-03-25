using HelpAtHome.Core.DTOs.Common;
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

        // ── Nested address (required at registration) ────────────────────
        [Required]
        public AddressUpsertDto Address { get; set; } = new();
    }

}
