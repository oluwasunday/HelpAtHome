using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class SupportTicket : BaseEntity
    {
        public string TicketNumber { get; set; }
        public Guid RaisedByUserId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? AssignedToAdminId { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public string Subject { get; set; }
        public string Description { get; set; }
        public bool IsDispute { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }

        public User RaisedBy { get; set; }
        public Booking? Booking { get; set; }
        public ICollection<TicketMessage> Messages { get; set; }
    }
}
