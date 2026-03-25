using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UploadDocumentDto ────────────────────────────────────────────────
    public class UploadDocumentDto
    {
        public DocumentType DocumentType { get; set; }
        public IFormFile File { get; set; } = null!;
        public string? DocumentNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
