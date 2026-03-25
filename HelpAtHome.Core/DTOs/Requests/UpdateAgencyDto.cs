using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateAgencyDto ──────────────────────────────────────────────────
    public class UpdateAgencyDto
    {
        public string? AgencyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public IFormFile? Logo { get; set; }
    }

}
