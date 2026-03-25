using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── AuditLogFilterDto ─────────────────────────────────────────────────
    public class AuditLogFilterDto
    {
        public string? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public string? EntityName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

}
