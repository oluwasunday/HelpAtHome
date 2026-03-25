using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Responses
{
    // ── RespondAlertDto ───────────────────────────────────────────────────
    public class RespondAlertDto
    {
        public string? ResolutionNote { get; set; }
        public AlertStatus NewStatus { get; set; }
    }

}
