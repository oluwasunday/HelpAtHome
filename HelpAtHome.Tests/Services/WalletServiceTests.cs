using FluentAssertions;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Application.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Tests.Fakes;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace HelpAtHome.Tests.Services
{
    public class WalletServiceTests
    {
        // ── Builder helpers ───────────────────────────────────────────────────

        private static (WalletService svc, FakeUnitOfWork uow, Mock<IPaymentGatewayService> paystack,
                        Mock<AutoMapper.IMapper> mapper, Mock<UserManager<User>> um)
            Build()
        {
            var uow = new FakeUnitOfWork();
            var paystack = new Mock<IPaymentGatewayService>();
            var notification = new Mock<INotificationService>();
            notification.Setup(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                                                 It.IsAny<string?>(), It.IsAny<string?>(),
                                                 It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                        .Returns(Task.CompletedTask);

            var mapper = new Mock<AutoMapper.IMapper>();
            mapper.Setup(m => m.Map<WalletDto>(It.IsAny<Wallet>()))
                  .Returns((Wallet w) => new WalletDto
                  {
                      Id = w.Id,
                      UserId = w.UserId,
                      Balance = w.Balance,
                      TotalEarned = w.TotalEarned,
                      TotalSpent = w.TotalSpent,
                      TotalWithdrawn = w.TotalWithdrawn,
                      IsLocked = w.IsLocked
                  });
            mapper.Setup(m => m.Map<List<TransactionDto>>(It.IsAny<object>()))
                  .Returns((object src) =>
                  {
                      var list = src as List<Transaction> ?? new List<Transaction>();
                      return list.Select(t => new TransactionDto { Id = t.Id, Amount = t.Amount }).ToList();
                  });

            var store = new Mock<IUserStore<User>>();
            var um = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var svc = new WalletService(uow, paystack.Object, notification.Object, mapper.Object, um.Object);
            return (svc, uow, paystack, mapper, um);
        }

        private static Wallet MakeWallet(Guid userId, decimal balance = 5000m, bool locked = false) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = balance,
            IsLocked = locked,
            LockReason = locked ? "Under review" : null
        };

        // ── GetWalletAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetWalletAsync_WithExistingWallet_ReturnsWalletDto()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();
            var wallet = MakeWallet(userId, 1234.56m);
            uow.WalletRepo.Data.Add(wallet);

            var result = await svc.GetWalletAsync(userId);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Balance.Should().Be(1234.56m);
            result.Data.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetWalletAsync_WhenWalletNotFound_ReturnsFailure()
        {
            var (svc, _, _, _, _) = Build();

            var result = await svc.GetWalletAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Wallet not found");
        }

        // ── InitiateDepositAsync ──────────────────────────────────────────────

        [Fact]
        public async Task InitiateDepositAsync_WhenAmountBelowMinimum_ReturnsFailure()
        {
            var (svc, _, _, _, _) = Build();

            var result = await svc.InitiateDepositAsync(Guid.NewGuid(), 499m, "https://callback.test");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Minimum deposit");
        }

        [Fact]
        public async Task InitiateDepositAsync_WithValidAmount_RecordsPendingTransactionAndReturnsUrl()
        {
            var (svc, uow, paystack, _, um) = Build();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "pay@test.com" };

            uow.WalletRepo.Data.Add(MakeWallet(userId, 1000m));
            um.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            paystack.Setup(p => p.InitializeTransactionAsync(
                user.Email, 1000m, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>()))
                .ReturnsAsync(new PaystackInitResult(true, "https://paystack.com/pay/abc", "acc123", "HAH-DEP-ref", null));

            var result = await svc.InitiateDepositAsync(userId, 1000m, "https://callback.test");

            result.IsSuccess.Should().BeTrue();
            result.Data!.AuthorizationUrl.Should().Be("https://paystack.com/pay/abc");
            uow.TransactionRepo.Data.Should().HaveCount(1);
            uow.TransactionRepo.Data.First().Status.Should().Be(TransactionStatus.Pending);
            uow.TransactionRepo.Data.First().Type.Should().Be(TransactionType.Deposit);
        }

        [Fact]
        public async Task InitiateDepositAsync_WhenPaystackFails_ReturnsFailure()
        {
            var (svc, uow, paystack, _, um) = Build();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "fail@test.com" };

            uow.WalletRepo.Data.Add(MakeWallet(userId));
            um.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            paystack.Setup(p => p.InitializeTransactionAsync(
                It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>()))
                .ReturnsAsync(new PaystackInitResult(false, null, null, null, null)); // null message → fallback message

            var result = await svc.InitiateDepositAsync(userId, 1000m, "https://callback");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Could not initialize payment");
        }

        [Fact]
        public async Task InitiateDepositAsync_WhenUserNotFound_ReturnsFailure()
        {
            var (svc, uow, _, _, um) = Build();
            var userId = Guid.NewGuid();

            uow.WalletRepo.Data.Add(MakeWallet(userId));
            um.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

            var result = await svc.InitiateDepositAsync(userId, 1000m, "https://callback");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("User not found");
        }

        // ── VerifyDepositAsync ────────────────────────────────────────────────

        [Fact]
        public async Task VerifyDepositAsync_WhenPaymentSucceeds_CreditsWalletAndUpdatesPendingTransaction()
        {
            var (svc, uow, paystack, _, _) = Build();
            var userId = Guid.NewGuid();
            var reference = "HAH-DEP-testref";
            var wallet = MakeWallet(userId, 500m);
            uow.WalletRepo.Data.Add(wallet);

            // Pending transaction
            uow.TransactionRepo.Data.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionReference = reference,
                WalletId = wallet.Id,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Pending,
                Amount = 2000m,
                PaystackReference = reference
            });

            paystack.Setup(p => p.VerifyTransactionAsync(reference))
                    .ReturnsAsync(new PaystackVerifyResult(
                        true, "success", 2000m,
                        new Dictionary<string, string> { ["userId"] = userId.ToString() },
                        "ps-txn-001", null));

            var result = await svc.VerifyDepositAsync(reference);

            result.IsSuccess.Should().BeTrue();
            wallet.Balance.Should().Be(2500m); // 500 + 2000
            uow.TransactionRepo.Data.First().Status.Should().Be(TransactionStatus.Success);
        }

        [Fact]
        public async Task VerifyDepositAsync_WhenAlreadyProcessed_ReturnsOkIdempotently()
        {
            var (svc, uow, paystack, _, _) = Build();
            var reference = "HAH-DEP-already";

            // Already succeeded
            uow.TransactionRepo.Data.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionReference = reference,
                PaystackReference = reference,
                Status = TransactionStatus.Success
            });

            var result = await svc.VerifyDepositAsync(reference);

            result.IsSuccess.Should().BeTrue();
            paystack.Verify(p => p.VerifyTransactionAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task VerifyDepositAsync_WhenPaystackReturnsFailure_ReturnsFailure()
        {
            var (svc, uow, paystack, _, _) = Build();
            var reference = "HAH-DEP-bad";

            paystack.Setup(p => p.VerifyTransactionAsync(reference))
                    .ReturnsAsync(new PaystackVerifyResult(false, "failed", 0m, new(), null, "Charge failed"));

            var result = await svc.VerifyDepositAsync(reference);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("verification failed");
        }

        // ── InitiateWithdrawalAsync ───────────────────────────────────────────

        [Fact]
        public async Task InitiateWithdrawalAsync_WhenAmountBelowMinimum_ReturnsFailure()
        {
            var (svc, _, _, _, _) = Build();

            var result = await svc.InitiateWithdrawalAsync(Guid.NewGuid(), new WithdrawalDto
            {
                Amount = 1999m,
                BankName = "GTBank",
                AccountNumber = "0123456789",
                AccountName = "Ada Obi"
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Minimum withdrawal");
        }

        [Fact]
        public async Task InitiateWithdrawalAsync_WhenWalletIsLocked_ReturnsFailure()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();
            uow.WalletRepo.Data.Add(MakeWallet(userId, 10000m, locked: true));

            var result = await svc.InitiateWithdrawalAsync(userId, new WithdrawalDto
            {
                Amount = 3000m,
                BankName = "GTBank",
                AccountNumber = "0123456789",
                AccountName = "Ada Obi"
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("locked");
        }

        [Fact]
        public async Task InitiateWithdrawalAsync_WhenBalanceInsufficient_ReturnsFailure()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();
            uow.WalletRepo.Data.Add(MakeWallet(userId, 1000m)); // only 1000 but wants to withdraw 3000

            var result = await svc.InitiateWithdrawalAsync(userId, new WithdrawalDto
            {
                Amount = 3000m,
                BankName = "GTBank",
                AccountNumber = "0123456789",
                AccountName = "Ada Obi"
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Insufficient wallet balance");
        }

        [Fact]
        public async Task InitiateWithdrawalAsync_WithValidDetails_DebitsWalletAndRecordsTransaction()
        {
            var (svc, uow, paystack, _, _) = Build();
            var userId = Guid.NewGuid();
            var wallet = MakeWallet(userId, 10000m);
            uow.WalletRepo.Data.Add(wallet);

            paystack.Setup(p => p.CreateTransferRecipientAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("RCP_abc123");

            paystack.Setup(p => p.InitiateTransferAsync(
                It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PaystackTransferResult(true, "TRF_001", null));

            var result = await svc.InitiateWithdrawalAsync(userId, new WithdrawalDto
            {
                Amount = 5000m,
                BankName = "GTBank",
                AccountNumber = "0123456789",
                AccountName = "Ada Obi"
            });

            result.IsSuccess.Should().BeTrue();
            wallet.Balance.Should().Be(5000m);     // 10000 - 5000
            wallet.TotalWithdrawn.Should().Be(5000m);
            uow.TransactionRepo.Data.Should().HaveCount(1);
            uow.TransactionRepo.Data.First().Type.Should().Be(TransactionType.Withdrawal);
            uow.TransactionRepo.Data.First().Status.Should().Be(TransactionStatus.Pending);
        }

        [Fact]
        public async Task InitiateWithdrawalAsync_WhenRecipientCreationFails_ReturnsFailure()
        {
            var (svc, uow, paystack, _, _) = Build();
            var userId = Guid.NewGuid();
            uow.WalletRepo.Data.Add(MakeWallet(userId, 10000m));

            paystack.Setup(p => p.CreateTransferRecipientAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var result = await svc.InitiateWithdrawalAsync(userId, new WithdrawalDto
            {
                Amount = 5000m,
                BankName = "Zenith",
                AccountNumber = "0987654321",
                AccountName = "Ada Obi"
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Could not register bank account");
        }

        // ── GetTransactionsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetTransactionsAsync_ReturnsPagedTransactionsOrderedByDate()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();
            var wallet = MakeWallet(userId);
            uow.WalletRepo.Data.Add(wallet);

            for (int i = 0; i < 7; i++)
            {
                uow.TransactionRepo.Data.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    TransactionReference = $"REF-{i}",
                    Type = TransactionType.Deposit,
                    Status = TransactionStatus.Success,
                    Amount = 100m * (i + 1),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i) // most recent first
                });
            }

            var result = await svc.GetTransactionsAsync(userId, 1, 5);

            result.IsSuccess.Should().BeTrue();
            result.Data!.TotalCount.Should().Be(7);
            result.Data.Items.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetTransactionsAsync_WhenWalletNotFound_ReturnsFailure()
        {
            var (svc, _, _, _, _) = Build();

            var result = await svc.GetTransactionsAsync(Guid.NewGuid(), 1, 10);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Wallet not found");
        }
    }
}
