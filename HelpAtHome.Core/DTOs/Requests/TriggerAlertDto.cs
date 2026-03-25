namespace HelpAtHome.Core.DTOs.Requests
{
    // ── TriggerAlertDto ───────────────────────────────────────────────────
    public class TriggerAlertDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        public string? Message { get; set; }   // e.g., "Chest pain"
    }

}
