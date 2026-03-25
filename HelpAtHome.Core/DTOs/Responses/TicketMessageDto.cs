using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── TicketMessageDto ──────────────────────────────────────────────────
    public class TicketMessageDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public bool IsInternal { get; set; }
        public UserSummaryDto Sender { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}
