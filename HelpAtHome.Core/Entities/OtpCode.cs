namespace HelpAtHome.Core.Entities
{
    public class OtpCode : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public string Purpose { get; set; }  // "EmailVerify","PhoneVerify","PasswordReset","Login2FA"
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public int Attempts { get; set; } = 0;
        public User User { get; set; }
    }
}
