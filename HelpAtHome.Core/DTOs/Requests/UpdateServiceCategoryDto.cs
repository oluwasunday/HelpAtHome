using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateServiceCategoryDto ──────────────────────────────────────────
    public class UpdateServiceCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Icon { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
    }

}
