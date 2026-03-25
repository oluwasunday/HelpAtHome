using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Responses
{
    public class CaregiverSummaryDto
    {
        public Guid Id { get; set; }
        public UserSummaryDto User { get; set; } = null!;
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Badge { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCompletedBookings { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        public bool IsAvailable { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string GenderProvided { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public string? AgencyName { get; set; }
        public AddressDto? Address { get; set; }
    }

}
