namespace HelpAtHome.Core.DTOs.Requests
{
    // ── VerifyCaregiverDto (admin action) ────────────────────────────────
    public class VerifyCaregiverDto
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }

}
