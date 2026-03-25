namespace HelpAtHome.Core.DTOs.Responses
{
    // ── AdminUserDto (admin view of any user) ─────────────────────────────
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSuspended { get; set; }
        public string? SuspensionReason { get; set; }
        public DateTime? SuspendedUntil { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public DateTime CreatedAt { get; set; }
        public CaregiverSummaryDto? CaregiverProfile { get; set; }
        public ClientProfileDto? ClientProfile { get; set; }
    }

}
