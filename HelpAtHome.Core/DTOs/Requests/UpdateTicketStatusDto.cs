using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateTicketStatusDto (admin) ─────────────────────────────────────
    public class UpdateTicketStatusDto
    {
        public TicketStatus Status { get; set; }
    }

}
