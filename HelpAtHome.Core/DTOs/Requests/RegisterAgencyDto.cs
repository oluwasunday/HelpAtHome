using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── RegisterAgencyDto (used after admin user already created) ─────────
    public class RegisterAgencyDto
    {
        public string AgencyName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website { get; set; }
        public IFormFile? Logo { get; set; }
    }

}
