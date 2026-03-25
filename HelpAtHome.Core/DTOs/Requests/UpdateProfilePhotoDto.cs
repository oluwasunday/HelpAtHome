using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateProfilePhotoDto ────────────────────────────────────────────
    public class UpdateProfilePhotoDto
    {
        public IFormFile Photo { get; set; } = null!;
    }

}
