using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public Guid? AgencyId { get; set; }

        public decimal Amount { get; set; } // always positive
        public CreditDebitType TransactionType { get; set; } // Credit / Debit

        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }

        public DateOnly TransactionDate { get; set; }

        public string Reference { get; set; } // idempotency key
        public string Description { get; set; }
    }

    /*KEEP for later
     * public async Task DebitWallet(Guid walletId, decimal amount)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            var wallet = await _db.Wallets
                .Where(x => x.Id == walletId)
                .FirstOrDefaultAsync();

            if (wallet.Balance < amount)
                throw new Exception("Insufficient balance");

            var before = wallet.Balance;
            var after = before - amount;

            wallet.Balance = after;

            _db.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                Type = TransactionType.Debit,
                BalanceBefore = before,
                BalanceAfter = after,
                Reference = Guid.NewGuid().ToString()
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
     * 
     */
}
