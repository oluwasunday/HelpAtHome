using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── UpdateCaregiverProfileDto ────────────────────────────────────────
    public class UpdateCaregiverProfileDto
    {
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? MonthlyRate { get; set; }
        public Gender? Gender { get; set; }
        public bool? CanCook { get; set; }
        public bool? CanDriveClient { get; set; }
        public bool? CanAdministerMedication { get; set; }
        public bool? CanDoHeavyCleaning { get; set; }
        public bool? CanDoErrands { get; set; }
        public bool? CanProvideCompanionship { get; set; }
        public bool? CanCareForBedridden { get; set; }
        public bool? HasFirstAidCertificate { get; set; }
        public string? LanguagesSpoken { get; set; }  // JSON array string
        public string? WorkingHours { get; set; }  // JSON object
        public List<Guid>? ServiceCategoryIds { get; set; }
        public AddressDto? Address { get; set; }
    }

}
