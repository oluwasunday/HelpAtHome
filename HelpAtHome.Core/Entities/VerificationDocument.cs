using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class VerificationDocument : BaseEntity
    {
        public Guid? CaregiverProfileId { get; set; }
        public Guid? AgencyId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentUrl { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string? ReviewNote { get; set; }
        public Guid? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public CaregiverProfile? CaregiverProfile { get; set; }
        public Agency? Agency { get; set; }
    }
}
