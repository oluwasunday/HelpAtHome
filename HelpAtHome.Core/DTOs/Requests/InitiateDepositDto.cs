namespace HelpAtHome.Core.DTOs.Requests
{
    // ── InitiateDepositDto ───────────────────────────────────────────────
    public class InitiateDepositDto
    {
        public decimal Amount { get; set; }  // in Naira (min 500)
        public string CallbackUrl { get; set; } = string.Empty;
    }

}
