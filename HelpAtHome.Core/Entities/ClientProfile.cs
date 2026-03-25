using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class ClientProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? SpecialNotes { get; set; }   // e.g., "has high BP, needs calm caregiver"
        public string? MedicalConditions { get; set; }
        public int? ClientAge { get; set; }
        public Gender CaregiverGenderPreference { get; set; } = Gender.Male;
        public bool RequireVerifiedOnly { get; set; } = false;
        public decimal WalletBalance { get; set; } = 0;

        // Navigation
        public ClientAddress? Address { get; set; }
        public User User { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<FamilyAccess> FamilyAccesses { get; set; }
        public ICollection<EmergencyAlert> EmergencyAlerts { get; set; }
    }
}
