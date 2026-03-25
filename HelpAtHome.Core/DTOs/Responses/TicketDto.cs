using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── TicketDto (list view) ─────────────────────────────────────────────
    public class TicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsDispute { get; set; }
        public UserSummaryDto RaisedBy { get; set; } = null!;
        public string? BookingRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

}
