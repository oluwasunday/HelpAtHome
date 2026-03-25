namespace HelpAtHome.Core.DTOs.Responses
{
    // ── TransactionDto ────────────────────────────────────────────────────
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public string? PaystackReference { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BookingReference { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
