using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class CaregiverProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? AgencyId { get; set; }        // null = individual
        public string Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public BadgeLevel Badge { get; set; } = BadgeLevel.New;
        public int TotalCompletedBookings { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        public Gender GenderProvided { get; set; }
        public Services Services { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByAdminId { get; set; }
        public string? RejectionReason { get; set; }
        public string? LanguagesSpoken { get; set; }  // JSON array string
        public string? WorkingHours { get; set; }     // JSON object: { Mon:"08:00-18:00", ... }
        public bool IsBackgroundChecked { get; set; } = false;
        public DateTime? BackgroundCheckDate { get; set; }

        // verification and safety
        public DocumentType IdType { get; set; }
        public string IdNumber { get; set; }
        public string DocumentPhotoUrl { get; set; }
        public string NextOfKinName { get; set; }
        public string NextOfKinPhoneNumber { get; set; }

        // Navigation
        public CaregiverAddress? Address { get; set; }
        public User User { get; set; }
        public Agency? Agency { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> ReviewsReceived { get; set; }
        public ICollection<VerificationDocument> Documents { get; set; }
        public ICollection<CaregiverService> CaregiverServices { get; set; }
    }
}
