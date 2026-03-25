using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class Agency : BaseEntity
    {
        public Guid AgencyAdminUserId { get; set; }
        public string AgencyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public DateTime? VerifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal CommissionRate { get; set; } = 15;  // % platform takes from agency
        public decimal AgencyCommissionRate { get; set; } = 10; // % agency takes from caregiver
        public decimal WalletBalance { get; set; } = 0;
        public int TotalCaregiversCount { get; set; } = 0;

        // Navigation
        public Guid AgencyAddressId { get; set; }
        public AgencyAddress? AgencyAddress { get; set; }
        public User AgencyAdmin { get; set; }
        public ICollection<CaregiverProfile> Caregivers { get; set; }
        public ICollection<VerificationDocument> Documents { get; set; }
    }
}
