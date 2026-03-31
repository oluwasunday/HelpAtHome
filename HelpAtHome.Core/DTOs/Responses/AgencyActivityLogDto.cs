namespace HelpAtHome.Core.DTOs.Responses
{
    public class AgencyActivityLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string AgencyId { get; set; } = string.Empty;
        public string AgencyAdminUserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? CaregiverId { get; set; }
        public string? BookingId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
