using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notification;

        public WalletService(IUnitOfWork uow, INotificationService notification)
        {
            _uow = uow;
            _notification = notification;
        }

        public Task<Result<PagedResult<TransactionDto>>> GetTransactionsAsync(Guid userId, int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<Result<WalletDto>> GetWalletAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Result> InitiateDepositAsync(Guid userId, decimal amount, string callbackUrl)
        {
            throw new NotImplementedException();
        }

        public Task<Result> InitiateWithdrawalAsync(Guid userId, WithdrawalDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> VerifyDepositAsync(string paystackReference)
        {
            var verification = new { IsSuccess = true, Data = new { Status = "", Amount = 0.0m, Metadata = new Dictionary<string,string>() } };// await _paystackService.VerifyTransactionAsync(paystackReference);
            if (!verification.IsSuccess || verification.Data.Status != "success")
                return Result.Fail("Payment verification failed");

            var amount = verification.Data.Amount / 100m;  // Paystack uses kobo
            var userId = Guid.Parse(verification.Data.Metadata["userId"].ToString());

            var wallet = await _uow.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null) return Result.Fail("Wallet not found");

            var alreadyProcessed = await _uow.Transactions
                .AnyAsync(t => t.PaystackReference == paystackReference
                    && t.Status == TransactionStatus.Success);
            if (alreadyProcessed) return Result.Ok();  // idempotent

            wallet.Balance += amount;
            wallet.TotalEarned += amount;
            _uow.Wallets.Update(wallet);

            await _uow.Transactions.AddAsync(new Transaction
            {
                TransactionReference = Guid.NewGuid().ToString("N"),
                WalletId = wallet.Id,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Success,
                Amount = amount,
                BalanceBefore = wallet.Balance - amount,
                BalanceAfter = wallet.Balance,
                PaystackReference = paystackReference,
                Description = "Wallet top-up via Paystack"
            });
            await _uow.SaveChangesAsync();
            await _notification.SendAsync(userId, "Wallet Funded", $"Your wallet has been credited with ₦{amount:N2}", "payment", null);
            return Result.Ok();
        }

    }
}
