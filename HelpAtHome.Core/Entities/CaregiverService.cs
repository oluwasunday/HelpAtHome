namespace HelpAtHome.Core.Entities
{
    // Junction table linking caregivers to the categories they offer
    public class CaregiverService : BaseEntity
    {
        public Guid CaregiverProfileId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public decimal? CustomRate { get; set; }  // override caregiver default rate
        public CaregiverProfile CaregiverProfile { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
    }
}
