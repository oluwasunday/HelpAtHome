namespace HelpAtHome.Core.DTOs.Common
{
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? City { get; set; }
    }
}
