using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class ClientProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? SpecialNotes { get; set; }   // e.g., "has high BP, needs calm caregiver"
        public string? MedicalConditions { get; set; }
        public Gender Gender { get; set; } = Gender.Male;
        public bool RequireVerifiedOnly { get; set; } = false;
        public decimal WalletBalance { get; set; } = 0;
        public Services ServicesNeeded { get; set; }
        public Frequency Frequency { get; set; } = Frequency.None;
        public PreferedGender CareGiverGenderPreference { get; set; } = PreferedGender.NoPreference;
        public RelationToRecipient RelationToRecipient { get; set; } = RelationToRecipient.Self;
        public DateTime DateOfBirth { get; set; }

        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhoneNumber { get; set; }

        // Navigation
        public ClientAddress? Address { get; set; }
        public User User { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<FamilyAccess> FamilyAccesses { get; set; }
        public ICollection<EmergencyAlert> EmergencyAlerts { get; set; }
    }
}
