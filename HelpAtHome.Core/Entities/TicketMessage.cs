namespace HelpAtHome.Core.Entities
{
    public class TicketMessage : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Guid SenderUserId { get; set; }
        public string Message { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsInternal { get; set; } = false;  // admin-only notes
        public SupportTicket Ticket { get; set; }
        public User Sender { get; set; }
    }
}
