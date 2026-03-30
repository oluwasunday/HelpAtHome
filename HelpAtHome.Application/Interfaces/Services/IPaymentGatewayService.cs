namespace HelpAtHome.Application.Interfaces.Services
{
    // ── Paystack result types ────────────────────────────────────────────────
    public sealed record PaystackInitResult(
        bool IsSuccess,
        string? AuthorizationUrl,
        string? AccessCode,
        string? Reference,
        string? ErrorMessage);

    public sealed record PaystackVerifyResult(
        bool IsSuccess,
        string Status,           // "success" | "failed" | "abandoned"
        decimal AmountNaira,
        Dictionary<string, string> Metadata,
        string? PaystackTransactionId,
        string? ErrorMessage);

    public sealed record PaystackTransferResult(
        bool IsSuccess,
        string? TransferCode,
        string? ErrorMessage);

    // ────────────────────────────────────────────────────────────────────────
    public interface IPaymentGatewayService
    {
        /// <summary>Initialize a Paystack payment (returns authorization URL).</summary>
        Task<PaystackInitResult> InitializeTransactionAsync(
            string email,
            decimal amountNaira,
            string reference,
            string callbackUrl,
            Dictionary<string, string>? metadata = null);

        /// <summary>Verify a completed Paystack payment by reference.</summary>
        Task<PaystackVerifyResult> VerifyTransactionAsync(string reference);

        /// <summary>Create a Paystack transfer recipient and return the recipient code.</summary>
        Task<string?> CreateTransferRecipientAsync(
            string accountName,
            string accountNumber,
            string bankCode);

        /// <summary>Initiate a Paystack transfer to a recipient.</summary>
        Task<PaystackTransferResult> InitiateTransferAsync(
            decimal amountNaira,
            string recipientCode,
            string reference,
            string reason);
    }
}
