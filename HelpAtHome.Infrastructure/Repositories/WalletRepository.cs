using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class WalletRepository(AppDbContext db) : GenericRepository<Wallet>(db), IWalletRepository
    {
        public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                db.Wallets, w => w.UserId == userId, ct);
        }

        public async Task CreditAsync(Guid walletId, decimal amount, CancellationToken ct = default)
            => await db.Database.ExecuteSqlRawAsync(
                "UPDATE wallets SET balance = balance + {0}, updated_at = UTC_TIMESTAMP(6) WHERE id = {1}",
                amount, walletId.ToString(), ct);

        public async Task DebitAsync(Guid walletId, decimal amount, CancellationToken ct = default)
            => await db.Database.ExecuteSqlRawAsync(
                "UPDATE wallets SET balance = balance - {0}, updated_at = UTC_TIMESTAMP(6) WHERE id = {1}",
                amount, walletId.ToString(), ct);

        public async Task MoveToEscrowAsync(Guid walletId, decimal amount, CancellationToken ct = default)
            => await db.Database.ExecuteSqlRawAsync(
                "UPDATE wallets SET balance = balance - {0}, escrow_balance = escrow_balance + {0}, updated_at = UTC_TIMESTAMP(6) WHERE id = {1}",
                amount, walletId.ToString(), ct);
    }
}
