namespace HelpAtHome.Core.DTOs.Requests
{
    // ── WithdrawalDto ────────────────────────────────────────────────────
    public class WithdrawalDto
    {
        public decimal Amount { get; set; }  // min ₦2,000
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }

}
