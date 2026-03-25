namespace HelpAtHome.Core.Entities
{
    public class ServiceCategory : BaseEntity
    {
        public string Name { get; set; }           // e.g., "Elderly Care", "House Cleaning"
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public ICollection<CaregiverService> CaregiverServices { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
