namespace HelpAtHome.Core.DTOs.Requests
{
    // ── DisputeDto ───────────────────────────────────────────────────────
    public class DisputeDto
    {
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

}
