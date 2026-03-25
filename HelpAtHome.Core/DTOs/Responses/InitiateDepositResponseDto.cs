namespace HelpAtHome.Core.DTOs.Responses
{
    // ── InitiateDepositResponseDto ────────────────────────────────────────
    public class InitiateDepositResponseDto
    {
        public string AuthorizationUrl { get; set; } = string.Empty;  // Paystack payment link
        public string AccessCode { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }

}
