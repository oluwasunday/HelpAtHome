using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── AdminUserFilterDto ────────────────────────────────────────────────
    public class AdminUserFilterDto
    {
        public UserRole? Role { get; set; }
        public bool? IsSuspended { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
