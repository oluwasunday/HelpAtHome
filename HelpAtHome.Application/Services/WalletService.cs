using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.AspNetCore.Identity;

namespace HelpAtHome.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPaymentGatewayService _paystack;
        private readonly INotificationService _notification;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        private const decimal MinDeposit    = 500m;
        private const decimal MinWithdrawal = 2_000m;

        public WalletService(
            IUnitOfWork uow,
            IPaymentGatewayService paystack,
            INotificationService notification,
            IMapper mapper,
            UserManager<User> userManager)
        {
            _uow          = uow;
            _paystack     = paystack;
            _notification = notification;
            _mapper       = mapper;
            _userManager  = userManager;
        }

        // ── Get wallet ───────────────────────────────────────────────────────
        public async Task<Result<WalletDto>> GetWalletAsync(Guid userId)
        {
            var wallet = await _uow.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Result<WalletDto>.Fail("Wallet not found.");

            return Result<WalletDto>.Ok(_mapper.Map<WalletDto>(wallet));
        }

        // ── Initiate deposit (create Paystack payment link) ──────────────────
        public async Task<Result<InitiateDepositResponseDto>> InitiateDepositAsync(
            Guid userId, decimal amount, string callbackUrl)
        {
            if (amount < MinDeposit)
                return Result<InitiateDepositResponseDto>.Fail($"Minimum deposit is ₦{MinDeposit:N0}.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<InitiateDepositResponseDto>.Fail("User not found.");

            var wallet = await _uow.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Result<InitiateDepositResponseDto>.Fail("Wallet not found.");

            var reference = $"HAH-DEP-{Guid.NewGuid():N}";
            var metadata  = new Dictionary<string, string> { ["userId"] = userId.ToString() };

            var init = await _paystack.InitializeTransactionAsync(
                user.Email!, amount, reference, callbackUrl, metadata);

            if (!init.IsSuccess)
                return Result<InitiateDepositResponseDto>.Fail(init.ErrorMessage ?? "Could not initialize payment.");

            // Record pending transaction for idempotency tracking
            await _uow.Transactions.AddAsync(new Transaction
            {
                TransactionReference = reference,
                WalletId             = wallet.Id,
                Type                 = TransactionType.Deposit,
                Status               = TransactionStatus.Pending,
                Amount               = amount,
                BalanceBefore        = wallet.Balance,
                BalanceAfter         = wallet.Balance,   // updated on verify
                PaystackReference    = reference,
                Description          = "Wallet top-up via Paystack"
            });
            await _uow.SaveChangesAsync();

            return Result<InitiateDepositResponseDto>.Ok(new InitiateDepositResponseDto
            {
                AuthorizationUrl = init.AuthorizationUrl!,
                AccessCode       = init.AccessCode!,
                Reference        = init.Reference!
            });
        }

        // ── Verify deposit (called after payment or from webhook) ────────────
        public async Task<Result> VerifyDepositAsync(string paystackReference)
        {
            // Idempotency: skip if already credited
            var alreadyDone = await _uow.Transactions
                .AnyAsync(t => t.PaystackReference == paystackReference
                            && t.Status == TransactionStatus.Success);
            if (alreadyDone) return Result.Ok();

            var verification = await _paystack.VerifyTransactionAsync(paystackReference);
            if (!verification.IsSuccess || verification.Status != "success")
                return Result.Fail("Payment verification failed.");

            var amount = verification.AmountNaira;

            if (!verification.Metadata.TryGetValue("userId", out var userIdStr)
                || !Guid.TryParse(userIdStr, out var userId))
                return Result.Fail("Invalid payment metadata.");

            var wallet = await _uow.Wallets.GetByUserIdAsync(userId);
            if (wallet == null) return Result.Fail("Wallet not found.");

            var balanceBefore = wallet.Balance;
            wallet.Balance     += amount;
            wallet.TotalEarned += amount;
            _uow.Wallets.Update(wallet);

            // Update the pending transaction record if it exists, else create a new one
            var pending = await _uow.Transactions
                .FirstOrDefaultAsync(t => t.PaystackReference == paystackReference
                                       && t.Status == TransactionStatus.Pending);

            if (pending != null)
            {
                pending.Status                 = TransactionStatus.Success;
                pending.BalanceBefore          = balanceBefore;
                pending.BalanceAfter           = wallet.Balance;
                pending.PaystackTransactionId  = verification.PaystackTransactionId;
                _uow.Transactions.Update(pending);
            }
            else
            {
                await _uow.Transactions.AddAsync(new Transaction
                {
                    TransactionReference  = $"HAH-DEP-{Guid.NewGuid():N}",
                    WalletId              = wallet.Id,
                    Type                  = TransactionType.Deposit,
                    Status                = TransactionStatus.Success,
                    Amount                = amount,
                    BalanceBefore         = balanceBefore,
                    BalanceAfter          = wallet.Balance,
                    PaystackReference     = paystackReference,
                    PaystackTransactionId = verification.PaystackTransactionId,
                    Description           = "Wallet top-up via Paystack"
                });
            }

            await _uow.SaveChangesAsync();
            await _notification.SendAsync(userId, "Wallet Funded",
                $"Your wallet has been credited with ₦{amount:N2}.", "payment", null);

            return Result.Ok();
        }

        // ── Initiate withdrawal ──────────────────────────────────────────────
        public async Task<Result> InitiateWithdrawalAsync(Guid userId, WithdrawalDto dto)
        {
            if (dto.Amount < MinWithdrawal)
                return Result.Fail($"Minimum withdrawal is ₦{MinWithdrawal:N0}.");

            var wallet = await _uow.Wallets.GetByUserIdAsync(userId);
            if (wallet == null) return Result.Fail("Wallet not found.");
            if (wallet.IsLocked)  return Result.Fail($"Wallet is locked: {wallet.LockReason}");
            if (wallet.Balance < dto.Amount)
                return Result.Fail("Insufficient wallet balance.");

            var reference = $"HAH-WDR-{Guid.NewGuid():N}";

            // Create transfer recipient (bank account) on Paystack
            // Note: bankCode lookup would normally come from a banks list endpoint.
            // We pass BankName as a stand-in; production should resolve to actual bank code.
            var recipientCode = await _paystack.CreateTransferRecipientAsync(
                dto.AccountName, dto.AccountNumber, dto.BankName);

            if (recipientCode == null)
                return Result.Fail("Could not register bank account for transfer.");

            var transfer = await _paystack.InitiateTransferAsync(
                dto.Amount, recipientCode, reference, "HelpAtHome wallet withdrawal");

            if (!transfer.IsSuccess)
                return Result.Fail(transfer.ErrorMessage ?? "Transfer initiation failed.");

            // Debit wallet and record the transaction
            var balanceBefore = wallet.Balance;
            wallet.Balance        -= dto.Amount;
            wallet.TotalWithdrawn += dto.Amount;
            _uow.Wallets.Update(wallet);

            await _uow.Transactions.AddAsync(new Transaction
            {
                TransactionReference = reference,
                WalletId             = wallet.Id,
                Type                 = TransactionType.Withdrawal,
                Status               = TransactionStatus.Pending,   // confirmed via webhook
                Amount               = dto.Amount,
                BalanceBefore        = balanceBefore,
                BalanceAfter         = wallet.Balance,
                BankName             = dto.BankName,
                AccountNumber        = dto.AccountNumber,
                AccountName          = dto.AccountName,
                Description          = "Wallet withdrawal"
            });

            await _uow.SaveChangesAsync();
            await _notification.SendAsync(userId, "Withdrawal Initiated",
                $"Your withdrawal of ₦{dto.Amount:N2} is being processed.", "payment", null);

            return Result.Ok();
        }

        // ── Get transactions (paged) ─────────────────────────────────────────
        public async Task<Result<PagedResult<TransactionDto>>> GetTransactionsAsync(
            Guid userId, int page, int size)
        {
            var wallet = await _uow.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
                return Result<PagedResult<TransactionDto>>.Fail("Wallet not found.");

            var all = await _uow.Transactions
                .FindAsync(t => t.WalletId == wallet.Id);

            var ordered = all.OrderByDescending(t => t.CreatedAt).ToList();
            var total   = ordered.Count;
            var items   = ordered.Skip((page - 1) * size).Take(size).ToList();
            var dtos    = _mapper.Map<List<TransactionDto>>(items);

            return Result<PagedResult<TransactionDto>>.Ok(
                new PagedResult<TransactionDto>(dtos, total, page, size));
        }
    }
}
