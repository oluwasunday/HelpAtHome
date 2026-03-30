using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Wallet — balances, deposits via Paystack, withdrawals, and the Paystack webhook.</summary>
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    [Produces("application/json")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IConfiguration _config;

        public WalletController(IWalletService walletService, IConfiguration config)
        {
            _walletService = walletService;
            _config        = config;
        }

        /// <summary>Get the authenticated user's wallet balance and details.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWallet()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.GetWalletAsync(userId);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }

        /// <summary>Get a paginated transaction history for the authenticated user's wallet.</summary>
        [HttpGet("transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.GetTransactionsAsync(userId, page, size);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        /// <summary>Initiate a wallet top-up via Paystack. Returns a payment authorization URL.</summary>
        /// <remarks>Redirect the user to the returned URL to complete payment. On success, Paystack calls the webhook.</remarks>
        [HttpPost("deposit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitiateDeposit([FromBody] InitiateDepositDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.InitiateDepositAsync(userId, dto.Amount, dto.CallbackUrl);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        /// <summary>Manually verify a deposit by its Paystack reference. Called after the user returns from the payment page.</summary>
        [HttpPost("deposit/verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyDeposit([FromQuery] string reference)
        {
            var result = await _walletService.VerifyDepositAsync(reference);
            return result.IsSuccess ? Ok(new { Message = "Deposit verified successfully." }) : BadRequest(result.ErrorMessage);
        }

        /// <summary>Initiate a withdrawal from the authenticated user's wallet to their bank account.</summary>
        [HttpPost("withdraw")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.InitiateWithdrawalAsync(userId, dto);
            return result.IsSuccess
                ? Ok(new { Message = "Withdrawal initiated. Funds will arrive within 1–2 business days." })
                : BadRequest(result.ErrorMessage);
        }

        /// <summary>
        /// Paystack webhook receiver. Verifies the HMAC-SHA512 signature before processing events.
        /// </summary>
        /// <remarks>
        /// This endpoint is called by Paystack, not by clients. Configure the URL in your Paystack dashboard.
        /// Currently handles: `charge.success` (auto-credits the wallet on successful payment).
        /// </remarks>
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Webhook()
        {
            // 1. Read raw body (needed for signature verification)
            Request.EnableBuffering();
            using var reader  = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // 2. Verify HMAC-SHA512 signature
            var secret    = _config["Paystack:WebhookSecret"];
            var signature = Request.Headers["x-paystack-signature"].FirstOrDefault();

            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature)
                || !IsValidSignature(rawBody, signature, secret))
            {
                return Unauthorized();
            }

            // 3. Parse event
            JsonElement root;
            try   { root = JsonSerializer.Deserialize<JsonElement>(rawBody); }
            catch { return BadRequest(); }

            if (!root.TryGetProperty("event", out var evtEl)) return Ok();
            var eventType = evtEl.GetString();

            switch (eventType)
            {
                case "charge.success":
                {
                    var reference = root
                        .GetProperty("data")
                        .GetProperty("reference")
                        .GetString();
                    if (!string.IsNullOrEmpty(reference))
                        await _walletService.VerifyDepositAsync(reference);
                    break;
                }
                // transfer.success / transfer.failed are handled by the Paystack dashboard
                // or can be expanded here when the withdrawal webhook flow is needed.
            }

            return Ok();
        }

        // ── helpers ─────────────────────────────────────────────────────────
        private static bool IsValidSignature(string rawBody, string signature, string secret)
        {
            var key  = Encoding.UTF8.GetBytes(secret);
            var data = Encoding.UTF8.GetBytes(rawBody);
            var hash = Convert.ToHexString(HMACSHA512.HashData(key, data)).ToLower();
            return hash == signature;
        }
    }
}
