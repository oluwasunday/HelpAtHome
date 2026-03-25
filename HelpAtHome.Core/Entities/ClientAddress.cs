namespace HelpAtHome.Core.Entities
{
    public class ClientAddress : BaseEntity
    {
        // ── Address lines ────────────────────────────────────────────────
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string Locality { get; set; } = string.Empty;  // e.g. "Ikoyi", "Lekki Phase 1"
        public string? City { get; set; }                   // e.g. "Lagos"
        public string LGA { get; set; } = string.Empty;  // e.g. "Eti-Osa"
        public string State { get; set; } = string.Empty;  // e.g. "Lagos"
        public string Country { get; set; } = "Nigeria";
        public string? PostalCode { get; set; }

        // ── GPS ──────────────────────────────────────────────────────────
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // ── FK ───────────────────────────────────────────────────────────
        public Guid ClientProfileId { get; set; }
        public ClientProfile ClientProfile { get; set; } = null!;
    }

}
