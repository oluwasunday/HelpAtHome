namespace HelpAtHome.Core.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; } = 0;
        public decimal TotalEarned { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public decimal TotalWithdrawn { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public string? LockReason { get; set; }

        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
