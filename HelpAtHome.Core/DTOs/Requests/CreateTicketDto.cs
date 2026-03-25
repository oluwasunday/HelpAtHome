using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── CreateTicketDto ──────────────────────────────────────────────────
    public class CreateTicketDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? BookingId { get; set; }
        public bool IsDispute { get; set; } = false;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }

}
