namespace HelpAtHome.Core.DTOs.Responses
{
    // ── NotificationDto ──────────────────────────────────────────────────
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? ReferenceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
