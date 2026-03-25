using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class EmergencyAlert : BaseEntity
    {
        public Guid ClientProfileId { get; set; }
        public Guid? ActiveBookingId { get; set; }
        public AlertStatus Status { get; set; } = AlertStatus.Active;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? LocationAddress { get; set; }
        public string? Message { get; set; }
        public DateTime? RespondedAt { get; set; }
        public Guid? RespondedByUserId { get; set; }
        public string? ResolutionNote { get; set; }
        public bool NotifiedFamily { get; set; } = false;
        public bool NotifiedAdmin { get; set; } = false;
        public bool NotifiedCaregiver { get; set; } = false;

        public ClientProfile ClientProfile { get; set; }
        public Booking? ActiveBooking { get; set; }
    }
}
