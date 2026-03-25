namespace HelpAtHome.Core.DTOs.Requests
{
    // ── ReviewDocumentDto (admin) ─────────────────────────────────────────
    public class ReviewDocumentDto
    {
        public bool IsApproved { get; set; }
        public string? ReviewNote { get; set; }
    }

}
