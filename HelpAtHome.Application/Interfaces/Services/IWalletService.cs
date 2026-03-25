using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<Result<WalletDto>> GetWalletAsync(Guid userId);
        Task<Result> InitiateDepositAsync(Guid userId, decimal amount, string callbackUrl);
        Task<Result> VerifyDepositAsync(string paystackReference);
        Task<Result> InitiateWithdrawalAsync(Guid userId, WithdrawalDto dto);
        Task<Result<PagedResult<TransactionDto>>> GetTransactionsAsync(Guid userId, int page, int size);
    }

}
