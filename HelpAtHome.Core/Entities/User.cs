using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace HelpAtHome.Core.Entities
{
    /// <summary>
    /// Central identity entity for ALL platform roles.
    /// Inherits IdentityUser<Guid> — do not redeclare Id, Email,
    /// PasswordHash, PhoneNumber, LockoutEnd, AccessFailedCount, etc.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        // ── Custom profile fields (not on IdentityUser) ───────────────
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? ProfilePhotoUrl { get; set; }

        // ── Suspension (beyond Identity lockout) ──────────────────────
        // LockoutEnd / LockoutEnabled / AccessFailedCount come from IdentityUser.
        // IsSuspended is an admin-level permanent/semi-permanent ban.
        public bool IsActive { get; set; } = true;
        public bool IsSuspended { get; set; } = false;
        public string? SuspensionReason { get; set; }
        public DateTime? SuspendedUntil { get; set; }

        // ── Push & session tracking ───────────────────────────────────
        public string? FcmDeviceToken { get; set; }  // Firebase Cloud Messaging
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }

        // ── Soft-delete (Identity has no built-in soft-delete) ────────
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // ── Audit timestamps ──────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Computed helper ───────────────────────────────────────────
        public string FullName => $"{FirstName} {LastName}".Trim();

        // ── Navigation properties ─────────────────────────────────────
        public CaregiverProfile? CaregiverProfile { get; set; }
        public ClientProfile? ClientProfile { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
        public ICollection<FamilyAccess> FamilyAccessGrants { get; set; } = new List<FamilyAccess>();
        public ICollection<FamilyAccess> FamilyAccessReceived { get; set; } = new List<FamilyAccess>();
    }
}
