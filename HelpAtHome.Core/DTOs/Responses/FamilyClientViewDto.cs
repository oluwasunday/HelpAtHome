namespace HelpAtHome.Core.DTOs.Responses
{
    // ── FamilyClientViewDto ───────────────────────────────────────────────
    // What a family member sees when monitoring their relative
    public class FamilyClientViewDto
    {
        public ClientProfileDto Client { get; set; } = null!;
        public BookingDto? ActiveBooking { get; set; }
        public List<BookingDto> RecentBookings { get; set; } = new();
        public List<EmergencyAlertDto> RecentAlerts { get; set; } = new();
        public string AccessLevel { get; set; } = string.Empty;
    }

}
