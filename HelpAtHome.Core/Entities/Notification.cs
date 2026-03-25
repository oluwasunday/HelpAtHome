namespace HelpAtHome.Core.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string? Type { get; set; }        // "booking", "payment", "alert", "system"
        public string? ReferenceId { get; set; } // related entity ID
        public bool IsRead { get; set; } = false;
        public bool IsSentPush { get; set; } = false;
        public bool IsSentEmail { get; set; } = false;
        public bool IsSentSms { get; set; } = false;

        public User User { get; set; }
    }
}
