namespace HelpAtHome.Core.DTOs.Responses
{
    // ── WalletDto ────────────────────────────────────────────────────────
    public class WalletDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
    }

}
