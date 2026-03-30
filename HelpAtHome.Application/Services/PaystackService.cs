using HelpAtHome.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelpAtHome.Application.Services
{
    public class PaystackService : IPaymentGatewayService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PaystackService> _logger;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        public PaystackService(IHttpClientFactory factory, ILogger<PaystackService> logger)
        {
            _http   = factory.CreateClient("Paystack");
            _logger = logger;
        }

        // ── Initialize ───────────────────────────────────────────────────────
        public async Task<PaystackInitResult> InitializeTransactionAsync(
            string email, decimal amountNaira, string reference,
            string callbackUrl, Dictionary<string, string>? metadata = null)
        {
            try
            {
                var body = new
                {
                    email,
                    amount       = (long)(amountNaira * 100),   // kobo
                    reference,
                    callback_url = callbackUrl,
                    metadata     = metadata ?? new Dictionary<string, string>()
                };

                var resp = await _http.PostAsJsonAsync("transaction/initialize", body, _json);
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);

                if (!json.GetProperty("status").GetBoolean())
                    return new PaystackInitResult(false, null, null, null, json.GetProperty("message").GetString());

                var data = json.GetProperty("data");
                return new PaystackInitResult(
                    true,
                    data.GetProperty("authorization_url").GetString(),
                    data.GetProperty("access_code").GetString(),
                    data.GetProperty("reference").GetString(),
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paystack initialize failed for ref {Ref}", reference);
                return new PaystackInitResult(false, null, null, null, "Payment gateway unavailable.");
            }
        }

        // ── Verify ───────────────────────────────────────────────────────────
        public async Task<PaystackVerifyResult> VerifyTransactionAsync(string reference)
        {
            try
            {
                var resp = await _http.GetAsync($"transaction/verify/{Uri.EscapeDataString(reference)}");
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);

                if (!json.GetProperty("status").GetBoolean())
                    return new PaystackVerifyResult(false, "failed", 0, new(), null,
                        json.GetProperty("message").GetString());

                var data        = json.GetProperty("data");
                var status      = data.GetProperty("status").GetString() ?? "failed";
                var amountKobo  = data.GetProperty("amount").GetDecimal();
                var txId        = data.TryGetProperty("id", out var idEl) ? idEl.GetRawText() : null;

                var meta = new Dictionary<string, string>();
                if (data.TryGetProperty("metadata", out var metaEl) && metaEl.ValueKind == JsonValueKind.Object)
                    foreach (var prop in metaEl.EnumerateObject())
                        meta[prop.Name] = prop.Value.ToString();

                return new PaystackVerifyResult(true, status, amountKobo / 100m, meta, txId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paystack verify failed for ref {Ref}", reference);
                return new PaystackVerifyResult(false, "failed", 0, new(), null, "Payment gateway unavailable.");
            }
        }

        // ── Create transfer recipient ─────────────────────────────────────────
        public async Task<string?> CreateTransferRecipientAsync(
            string accountName, string accountNumber, string bankCode)
        {
            try
            {
                var body = new
                {
                    type           = "nuban",
                    name           = accountName,
                    account_number = accountNumber,
                    bank_code      = bankCode,
                    currency       = "NGN"
                };

                var resp = await _http.PostAsJsonAsync("transferrecipient", body, _json);
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);

                if (!json.GetProperty("status").GetBoolean()) return null;

                return json.GetProperty("data").GetProperty("recipient_code").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paystack create recipient failed for account {Acct}", accountNumber);
                return null;
            }
        }

        // ── Initiate transfer ────────────────────────────────────────────────
        public async Task<PaystackTransferResult> InitiateTransferAsync(
            decimal amountNaira, string recipientCode, string reference, string reason)
        {
            try
            {
                var body = new
                {
                    source         = "balance",
                    amount         = (long)(amountNaira * 100),
                    recipient      = recipientCode,
                    reference,
                    reason
                };

                var resp = await _http.PostAsJsonAsync("transfer", body, _json);
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);

                if (!json.GetProperty("status").GetBoolean())
                    return new PaystackTransferResult(false, null, json.GetProperty("message").GetString());

                var transferCode = json.GetProperty("data").GetProperty("transfer_code").GetString();
                return new PaystackTransferResult(true, transferCode, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paystack transfer failed for ref {Ref}", reference);
                return new PaystackTransferResult(false, null, "Payment gateway unavailable.");
            }
        }
    }
}
