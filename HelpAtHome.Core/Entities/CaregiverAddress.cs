namespace HelpAtHome.Core.Entities
{
    public class CaregiverAddress : BaseEntity
    {
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string Locality { get; set; } = string.Empty;
        public string? City { get; set; }
        public string LGA { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "Nigeria";
        public string? PostalCode { get; set; }

        // ── FK ───────────────────────────────────────────────────────────
        // AgencyId is null for individual caregivers
        public Guid? AgencyId { get; set; }    // null for individual caregivers
        public Agency? Agency { get; set; }
        public Guid CaregiverProfileId { get; set; }
        public CaregiverProfile CaregiverProfile { get; set; } = null!;
    }

}
