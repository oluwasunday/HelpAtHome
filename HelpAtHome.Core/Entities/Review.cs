namespace HelpAtHome.Core.Entities
{
    public class Review : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid ReviewerUserId { get; set; }
        public Guid RevieweeUserId { get; set; }
        public int Rating { get; set; }           // 1–5 stars
        public string? Comment { get; set; }
        public bool IsByClient { get; set; }      // true=client reviewed caregiver; false=vice versa
        public bool IsVisible { get; set; } = true;
        public string? AdminNote { get; set; }    // admin moderation note
        public bool IsFlagged { get; set; } = false;

        public Booking Booking { get; set; }
        public User Reviewer { get; set; }
        public User Reviewee { get; set; }
    }
}
