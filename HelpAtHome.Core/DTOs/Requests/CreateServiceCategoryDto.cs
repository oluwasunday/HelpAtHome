using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── CreateServiceCategoryDto ──────────────────────────────────────────
    public class CreateServiceCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
    }

}
