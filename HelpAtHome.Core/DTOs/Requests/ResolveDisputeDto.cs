namespace HelpAtHome.Core.DTOs.Requests
{
    // ── ResolveDisputeDto (admin) ─────────────────────────────────────────
    public class ResolveDisputeDto
    {
        public string Resolution { get; set; } = string.Empty;
        public bool RefundClient { get; set; }
    }

}
