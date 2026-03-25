using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class Booking : BaseEntity
    {
        public string BookingReference { get; set; }   // e.g., HAH-20240901-0042
        public Guid ClientProfileId { get; set; }
        public Guid CaregiverProfileId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public FrequencyType Frequency { get; set; }
        public DateTime ScheduledStartDate { get; set; }
        public DateTime ScheduledEndDate { get; set; }
        public TimeSpan? DailyStartTime { get; set; }
        public TimeSpan? DailyEndTime { get; set; }
        public string? SpecialInstructions { get; set; }
        public decimal AgreedAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal CaregiverEarnings { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? CancellationReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsReviewedByClient { get; set; } = false;
        public bool IsReviewedByCaregiver { get; set; } = false;
        public bool HasDispute { get; set; } = false;
        public string? ClientAddress { get; set; }
        public decimal? ClientLatitude { get; set; }
        public decimal? ClientLongitude { get; set; }

        // Navigation
        public ClientProfile ClientProfile { get; set; }
        public CaregiverProfile CaregiverProfile { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<SupportTicket> Disputes { get; set; }
        public Transaction? Transaction { get; set; }
    }
}
