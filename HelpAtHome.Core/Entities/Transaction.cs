using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class Transaction : BaseEntity
    {
        public string TransactionReference { get; set; }
        public Guid WalletId { get; set; }
        public Guid? BookingId { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public string? PaystackReference { get; set; }
        public string? PaystackTransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }

        public Wallet Wallet { get; set; }
        public Booking? Booking { get; set; }
    }
}
