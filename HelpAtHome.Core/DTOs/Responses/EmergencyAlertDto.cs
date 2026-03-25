namespace HelpAtHome.Core.DTOs.Responses
{
    // ── EmergencyAlertDto ─────────────────────────────────────────────────
    public class EmergencyAlertDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? LocationAddress { get; set; }
        public string? Message { get; set; }
        public bool NotifiedFamily { get; set; }
        public bool NotifiedAdmin { get; set; }
        public bool NotifiedCaregiver { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResolutionNote { get; set; }
        public ClientProfileDto? Client { get; set; }
        public BookingDto? ActiveBooking { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
