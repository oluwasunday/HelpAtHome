namespace HelpAtHome.Core.Entities
{
    public class AgencyAddress : BaseEntity
    {
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string Locality { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string LGA { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "Nigeria";
        public string? PostalCode { get; set; }

        // ── FK ───────────────────────────────────────────────────────────
        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; } = null!;
    }

}
