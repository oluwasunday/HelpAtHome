namespace HelpAtHome.Core.DTOs.Responses
{
    // ── TicketDetailDto (full view with messages) ─────────────────────────
    public class TicketDetailDto : TicketDto
    {
        public string? Description { get; set; }
        public string? ResolutionNote { get; set; }
        public string? AssignedToAdminId { get; set; }
        public BookingDto? Booking { get; set; }
        public List<TicketMessageDto> Messages { get; set; } = new();
    }

}
