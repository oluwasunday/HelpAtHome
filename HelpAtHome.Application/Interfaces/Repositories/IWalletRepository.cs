using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task CreditAsync(Guid walletId, decimal amount, CancellationToken ct = default);
        Task DebitAsync(Guid walletId, decimal amount, CancellationToken ct = default);
    }
}
