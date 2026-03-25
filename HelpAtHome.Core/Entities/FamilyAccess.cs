using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class FamilyAccess : BaseEntity
    {
        public Guid ClientUserId { get; set; }       // the elderly person
        public Guid FamilyMemberUserId { get; set; }
        public AccessLevel AccessLevel { get; set; } = AccessLevel.ViewOnly;
        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public bool ReceiveEmergencyAlerts { get; set; } = true;
        public bool ReceiveBookingUpdates { get; set; } = true;
        public bool ReceivePaymentAlerts { get; set; } = false;

        public User Client { get; set; }
        public User FamilyMember { get; set; }
    }
}
