namespace HelpAtHome.Core.DTOs.Responses
{
    // ── NotificationSummaryDto ────────────────────────────────────────────
    // Lightweight response for notification bell / badge count
    public class NotificationSummaryDto
    {
        public int UnreadCount { get; set; }
        public List<NotificationDto> Recent { get; set; } = new();
    }

}
