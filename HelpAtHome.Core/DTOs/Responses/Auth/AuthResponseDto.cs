using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Responses.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

}
