using HelpAtHome.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests
{
    public class CreateBookingDto : IValidatableObject
    {
        [Required]
        public Guid CaregiverProfileId { get; set; }

        [Required]
        public Guid ServiceCategoryId { get; set; }

        [Required]
        public FrequencyType Frequency { get; set; }

        [Required]
        public DateTime ScheduledStartDate { get; set; }

        [Required]
        public DateTime ScheduledEndDate { get; set; }

        public TimeSpan? DailyStartTime { get; set; }
        public TimeSpan? DailyEndTime { get; set; }

        [StringLength(1000)]
        public string? SpecialInstructions { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal? Longitude { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ScheduledEndDate <= ScheduledStartDate)
                yield return new ValidationResult(
                    "ScheduledEndDate must be after ScheduledStartDate.",
                    new[] { nameof(ScheduledEndDate) });

            if (ScheduledStartDate < DateTime.UtcNow.Date)
                yield return new ValidationResult(
                    "ScheduledStartDate cannot be in the past.",
                    new[] { nameof(ScheduledStartDate) });

            if (CaregiverProfileId == Guid.Empty)
                yield return new ValidationResult(
                    "CaregiverProfileId is required.",
                    new[] { nameof(CaregiverProfileId) });

            if (ServiceCategoryId == Guid.Empty)
                yield return new ValidationResult(
                    "ServiceCategoryId is required.",
                    new[] { nameof(ServiceCategoryId) });
        }
    }
}
