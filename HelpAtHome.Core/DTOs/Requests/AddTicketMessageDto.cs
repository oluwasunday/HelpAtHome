using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── AddTicketMessageDto ───────────────────────────────────────────────
    public class AddTicketMessageDto
    {
        public string Message { get; set; } = string.Empty;
        public IFormFile? Attachment { get; set; }
        public bool IsInternal { get; set; } = false;  // admin-only note
    }
}
