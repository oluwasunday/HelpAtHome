namespace HelpAtHome.Core.DTOs.Responses
{
    // ── AuditLogDto (from MongoDB) ────────────────────────────────────────
    public class AuditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string PerformedByUserId { get; set; } = string.Empty;
        public string PerformedByRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
