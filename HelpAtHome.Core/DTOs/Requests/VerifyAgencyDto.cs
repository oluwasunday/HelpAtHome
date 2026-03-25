namespace HelpAtHome.Core.DTOs.Requests
{
    // ── VerifyAgencyDto (admin) ───────────────────────────────────────────
    public class VerifyAgencyDto
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }

}
