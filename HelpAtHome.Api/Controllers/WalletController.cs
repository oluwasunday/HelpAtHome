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
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IConfiguration _config;

        public WalletController(IWalletService walletService, IConfiguration config)
        {
            _walletService = walletService;
            _config        = config;
        }

        // GET /api/wallet
        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.GetWalletAsync(userId);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }

        // GET /api/wallet/transactions?page=1&size=20
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.GetTransactionsAsync(userId, page, size);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        // POST /api/wallet/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> InitiateDeposit([FromBody] InitiateDepositDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.InitiateDepositAsync(userId, dto.Amount, dto.CallbackUrl);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        // POST /api/wallet/deposit/verify?reference=xxx
        [HttpPost("deposit/verify")]
        public async Task<IActionResult> VerifyDeposit([FromQuery] string reference)
        {
            var result = await _walletService.VerifyDepositAsync(reference);
            return result.IsSuccess ? Ok(new { Message = "Deposit verified successfully." }) : BadRequest(result.ErrorMessage);
        }

        // POST /api/wallet/withdraw
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _walletService.InitiateWithdrawalAsync(userId, dto);
            return result.IsSuccess ? Ok(new { Message = "Withdrawal initiated. Funds will arrive within 1–2 business days." }) : BadRequest(result.ErrorMessage);
        }

        // ── Paystack webhook ────────────────────────────────────────────────
        // POST /api/wallet/webhook
        // Paystack sends signed events here; we verify HMAC-SHA512 before processing.
        [HttpPost("webhook")]
        [AllowAnonymous]
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
