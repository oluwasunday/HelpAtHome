namespace HelpAtHome.Core.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string? RevokedReason { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public User User { get; set; }
    }
}
