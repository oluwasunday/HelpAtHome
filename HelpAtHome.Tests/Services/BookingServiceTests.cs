using FluentAssertions;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Application.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace HelpAtHome.Tests.Services
{
    public class BookingServiceTests
    {
        // ── Builder helpers ───────────────────────────────────────────────────

        private static (BookingService svc, FakeUnitOfWork uow, Mock<AutoMapper.IMapper> mapper) Build()
        {
            var uow = new FakeUnitOfWork();
            var mapper = new Mock<AutoMapper.IMapper>();

            // Map any Booking → BookingDto
            mapper.Setup(m => m.Map<BookingDto>(It.IsAny<Booking>()))
                  .Returns((Booking b) => new BookingDto
                  {
                      Id = b.Id,
                      BookingReference = b.BookingReference ?? "REF",
                      Status = b.Status.ToString(),
                      AgreedAmount = b.AgreedAmount
                  });
            mapper.Setup(m => m.Map<List<BookingDto>>(It.IsAny<object>()))
                  .Returns((object src) =>
                  {
                      var bookings = src as List<Booking> ?? new List<Booking>();
                      return bookings.Select(b => new BookingDto { Id = b.Id, BookingReference = b.BookingReference ?? "REF" }).ToList();
                  });

            var notification = new Mock<INotificationService>();
            notification.Setup(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                                                 It.IsAny<string?>(), It.IsAny<string?>(),
                                                 It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                        .Returns(Task.CompletedTask);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Platform:CommissionRate"] = "15",
                    ["Platform:BookingReferencePrefix"] = "HAH"
                })
                .Build();

            var logger = new Mock<ILogger<BookingService>>();
            var badge = new Mock<IBadgeService>();
            var svc = new BookingService(uow, mapper.Object, notification.Object, config, logger.Object, badge.Object);
            return (svc, uow, mapper);
        }

        /// <summary>Creates a connected booking with all required navigation properties set.</summary>
        private static (Booking booking, ClientProfile client, CaregiverProfile caregiver, Wallet clientWallet, Wallet caregiverWallet)
            MakeConnectedBooking(FakeUnitOfWork uow, decimal agreedAmount = 1000m, BookingStatus status = BookingStatus.Pending)
        {
            var clientUserId = Guid.NewGuid();
            var caregiverUserId = Guid.NewGuid();

            var clientProfile = new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId,
                DateOfBirth = new DateTime(1985, 1, 1)
            };
            clientProfile.User = new User
            {
                Id = clientUserId,
                FirstName = "Client",
                LastName = "User",
                Email = "client@test.com"
            };

            var caregiverProfile = new CaregiverProfile
            {
                Id = Guid.NewGuid(),
                UserId = caregiverUserId,
                IsAvailable = true,
                VerificationStatus = VerificationStatus.Approved,
                HourlyRate = 100m,
                DailyRate = 500m,
                WeeklyRate = 2500m,
                MonthlyRate = 8000m,
                Bio = "Experienced caregiver",
                IdNumber = "12345",
                DocumentPhotoUrl = "photo.jpg",
                NextOfKinName = "Relative",
                NextOfKinPhoneNumber = "+2349000000001"
            };
            caregiverProfile.User = new User
            {
                Id = caregiverUserId,
                FirstName = "Caregiver",
                LastName = "One",
                Email = "caregiver@test.com"
            };

            var platformFee = agreedAmount * 0.15m;
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                BookingReference = "HAH-20260101-0001",
                ClientProfileId = clientProfile.Id,
                CaregiverProfileId = caregiverProfile.Id,
                ServiceCategoryId = Guid.NewGuid(),
                Frequency = FrequencyType.Daily,
                ScheduledStartDate = DateTime.UtcNow.AddDays(1),
                ScheduledEndDate = DateTime.UtcNow.AddDays(3),
                AgreedAmount = agreedAmount,
                PlatformFee = platformFee,
                CaregiverEarnings = agreedAmount - platformFee,
                Status = status,
                PaymentStatus = PaymentStatus.Paid
            };
            // Wire navigation properties
            booking.ClientProfile = clientProfile;
            booking.CaregiverProfile = caregiverProfile;

            var clientWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId,
                Balance = 5000m
            };
            var caregiverWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = caregiverUserId,
                Balance = 0m
            };

            // Seed UoW
            uow.ClientProfileRepo.Data.Add(clientProfile);
            uow.CaregiverProfileRepo.Data.Add(caregiverProfile);
            uow.BookingRepo.Data.Add(booking);
            uow.WalletRepo.Data.Add(clientWallet);
            uow.WalletRepo.Data.Add(caregiverWallet);

            return (booking, clientProfile, caregiverProfile, clientWallet, caregiverWallet);
        }

        // ── CreateBookingAsync ────────────────────────────────────────────────

        [Fact]
        public async Task CreateBookingAsync_WithSufficientBalance_CreatesBookingAndDebitsWallet()
        {
            var (svc, uow, mapper) = Build();
            var clientUserId = Guid.NewGuid();
            var caregiverProfile = new CaregiverProfile
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsAvailable = true,
                VerificationStatus = VerificationStatus.Approved,
                HourlyRate = 100m,
                DailyRate = 500m,
                WeeklyRate = 2500m,
                MonthlyRate = 8000m,
                Bio = "Test",
                IdNumber = "12345",
                DocumentPhotoUrl = "photo.jpg",
                NextOfKinName = "KIN",
                NextOfKinPhoneNumber = "+2349000000001"
            };
            caregiverProfile.User = new User
            {
                Id = caregiverProfile.UserId,
                FirstName = "Care",
                LastName = "Giver"
            };

            var clientProfile = new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId,
                RequireVerifiedOnly = false,
                DateOfBirth = new DateTime(1985, 1, 1)
            };
            var clientWallet = new Wallet { Id = Guid.NewGuid(), UserId = clientUserId, Balance = 3000m };

            uow.ClientProfileRepo.Data.Add(clientProfile);
            uow.CaregiverProfileRepo.Data.Add(caregiverProfile);
            uow.WalletRepo.Data.Add(clientWallet);

            var dto = new CreateBookingDto
            {
                CaregiverProfileId = caregiverProfile.Id,
                ServiceCategoryId = Guid.NewGuid(),
                Frequency = FrequencyType.Daily,
                ScheduledStartDate = DateTime.UtcNow.AddDays(1),
                ScheduledEndDate = DateTime.UtcNow.AddDays(3)
            };

            var result = await svc.CreateBookingAsync(clientUserId, dto);

            result.IsSuccess.Should().BeTrue();
            uow.BookingRepo.Data.Should().HaveCount(1);
            uow.TransactionRepo.Data.Should().HaveCount(1);
            uow.TransactionRepo.Data.First().Type.Should().Be(TransactionType.Booking);
            clientWallet.Balance.Should().BeLessThan(3000m); // debited
        }

        [Fact]
        public async Task CreateBookingAsync_WithInsufficientBalance_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var clientUserId = Guid.NewGuid();
            var caregiverProfile = new CaregiverProfile
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsAvailable = true,
                DailyRate = 5000m,
                Bio = "Test",
                IdNumber = "ID",
                DocumentPhotoUrl = "p.jpg",
                NextOfKinName = "K",
                NextOfKinPhoneNumber = "+234"
            };
            caregiverProfile.User = new User { Id = caregiverProfile.UserId };

            uow.ClientProfileRepo.Data.Add(new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId,
                DateOfBirth = new DateTime(1985, 1, 1)
            });
            uow.CaregiverProfileRepo.Data.Add(caregiverProfile);
            uow.WalletRepo.Data.Add(new Wallet { Id = Guid.NewGuid(), UserId = clientUserId, Balance = 100m });

            var result = await svc.CreateBookingAsync(clientUserId, new CreateBookingDto
            {
                CaregiverProfileId = caregiverProfile.Id,
                Frequency = FrequencyType.Daily,
                ScheduledStartDate = DateTime.UtcNow.AddDays(1),
                ScheduledEndDate = DateTime.UtcNow.AddDays(3)
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Insufficient wallet balance");
        }

        [Fact]
        public async Task CreateBookingAsync_WhenClientProfileNotFound_ReturnsFailure()
        {
            var (svc, uow, _) = Build();

            var result = await svc.CreateBookingAsync(Guid.NewGuid(), new CreateBookingDto
            {
                CaregiverProfileId = Guid.NewGuid(),
                Frequency = FrequencyType.OneTime,
                ScheduledStartDate = DateTime.UtcNow.AddDays(1),
                ScheduledEndDate = DateTime.UtcNow.AddDays(2)
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Client profile not found");
        }

        [Fact]
        public async Task CreateBookingAsync_WhenCaregiverNotAvailable_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var clientUserId = Guid.NewGuid();

            uow.ClientProfileRepo.Data.Add(new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId,
                DateOfBirth = new DateTime(1990, 1, 1)
            });

            var caregiver = new CaregiverProfile
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsAvailable = false, // not available
                Bio = "Test",
                IdNumber = "ID",
                DocumentPhotoUrl = "p.jpg",
                NextOfKinName = "K",
                NextOfKinPhoneNumber = "+234"
            };
            uow.CaregiverProfileRepo.Data.Add(caregiver);

            var result = await svc.CreateBookingAsync(clientUserId, new CreateBookingDto
            {
                CaregiverProfileId = caregiver.Id,
                Frequency = FrequencyType.OneTime,
                ScheduledStartDate = DateTime.UtcNow.AddDays(1),
                ScheduledEndDate = DateTime.UtcNow.AddDays(2)
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("not available");
        }

        [Fact]
        public async Task CreateBookingAsync_WhenCaregiverHasConflict_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var clientUserId = Guid.NewGuid();

            var caregiverProfile = new CaregiverProfile
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsAvailable = true,
                DailyRate = 200m,
                Bio = "Test",
                IdNumber = "ID",
                DocumentPhotoUrl = "p.jpg",
                NextOfKinName = "K",
                NextOfKinPhoneNumber = "+234"
            };
            caregiverProfile.User = new User { Id = caregiverProfile.UserId };

            uow.ClientProfileRepo.Data.Add(new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId,
                DateOfBirth = new DateTime(1990, 1, 1)
            });
            uow.CaregiverProfileRepo.Data.Add(caregiverProfile);
            uow.WalletRepo.Data.Add(new Wallet { Id = Guid.NewGuid(), UserId = clientUserId, Balance = 5000m });

            // Existing active booking for same dates
            uow.BookingRepo.Data.Add(new Booking
            {
                Id = Guid.NewGuid(),
                CaregiverProfileId = caregiverProfile.Id,
                Status = BookingStatus.Accepted,
                ScheduledStartDate = DateTime.UtcNow.AddDays(1),
                ScheduledEndDate = DateTime.UtcNow.AddDays(4)
            });

            var result = await svc.CreateBookingAsync(clientUserId, new CreateBookingDto
            {
                CaregiverProfileId = caregiverProfile.Id,
                Frequency = FrequencyType.Daily,
                ScheduledStartDate = DateTime.UtcNow.AddDays(2),
                ScheduledEndDate = DateTime.UtcNow.AddDays(3)
            });

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("conflicting booking");
        }

        // ── AcceptBookingAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AcceptBookingAsync_WithPendingBooking_ChangesStatusToAccepted()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Pending);

            var result = await svc.AcceptBookingAsync(caregiver.UserId, booking.Id);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Accepted);
            booking.AcceptedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task AcceptBookingAsync_WhenBookingNotFound_ReturnsFailure()
        {
            var (svc, _, _) = Build();

            var result = await svc.AcceptBookingAsync(Guid.NewGuid(), Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Booking not found");
        }

        [Fact]
        public async Task AcceptBookingAsync_WhenWrongCaregiver_ReturnsAccessDenied()
        {
            var (svc, uow, _) = Build();
            var (booking, _, _, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Pending);

            // A different caregiver tries to accept
            var impostor = new CaregiverProfile
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Bio = "Other",
                IdNumber = "ID",
                DocumentPhotoUrl = "p.jpg",
                NextOfKinName = "K",
                NextOfKinPhoneNumber = "+234"
            };
            uow.CaregiverProfileRepo.Data.Add(impostor);

            var result = await svc.AcceptBookingAsync(impostor.UserId, booking.Id);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Access denied");
        }

        [Fact]
        public async Task AcceptBookingAsync_WhenBookingAlreadyAccepted_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Accepted);

            var result = await svc.AcceptBookingAsync(caregiver.UserId, booking.Id);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot accept");
        }

        // ── StartBookingAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task StartBookingAsync_WithAcceptedBooking_ChangesStatusToInProgress()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Accepted);

            var result = await svc.StartBookingAsync(caregiver.UserId, booking.Id);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.InProgress);
            booking.StartedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task StartBookingAsync_WhenBookingIsPending_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Pending);

            var result = await svc.StartBookingAsync(caregiver.UserId, booking.Id);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot start");
        }

        // ── CompleteBookingAsync ──────────────────────────────────────────────

        [Fact]
        public async Task CompleteBookingAsync_WithInProgressBooking_CreditsCaregiver()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, _, caregiverWallet) = MakeConnectedBooking(uow, agreedAmount: 1000m, status: BookingStatus.InProgress);
            var expectedEarnings = booking.CaregiverEarnings;

            var result = await svc.CompleteBookingAsync(caregiver.UserId, booking.Id);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Completed);
            booking.CompletedAt.Should().NotBeNull();
            caregiverWallet.Balance.Should().Be(expectedEarnings);
            uow.TransactionRepo.Data.Should().HaveCount(1);
            uow.TransactionRepo.Data.First().Type.Should().Be(TransactionType.Payout);
            caregiver.TotalCompletedBookings.Should().Be(1);
        }

        [Fact]
        public async Task CompleteBookingAsync_WhenBookingNotInProgress_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Accepted);

            var result = await svc.CompleteBookingAsync(caregiver.UserId, booking.Id);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot complete");
        }

        // ── CancelBookingAsync ────────────────────────────────────────────────

        [Fact]
        public async Task CancelBookingAsync_WhenPendingByClient_IssuesFullRefund()
        {
            var (svc, uow, _) = Build();
            var (booking, client, _, clientWallet, _) = MakeConnectedBooking(uow, agreedAmount: 1000m, status: BookingStatus.Pending);
            var balanceBefore = clientWallet.Balance;

            var result = await svc.CancelBookingAsync(client.UserId, booking.Id, "Changed my mind");

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Cancelled);
            booking.CancelledBy.Should().Be("Client");
            clientWallet.Balance.Should().Be(balanceBefore + booking.AgreedAmount);
            uow.TransactionRepo.Data.Should().HaveCount(1);
            uow.TransactionRepo.Data.First().Type.Should().Be(TransactionType.Refund);
        }

        [Fact]
        public async Task CancelBookingAsync_WhenInProgressByClient_NoRefundIssued()
        {
            var (svc, uow, _) = Build();
            var (booking, client, _, clientWallet, _) = MakeConnectedBooking(uow, agreedAmount: 1000m, status: BookingStatus.InProgress);
            var balanceBefore = clientWallet.Balance;

            var result = await svc.CancelBookingAsync(client.UserId, booking.Id, "Emergency");

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Cancelled);
            clientWallet.Balance.Should().Be(balanceBefore); // no refund for in-progress
            uow.TransactionRepo.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task CancelBookingAsync_WhenAlreadyCompleted_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var (booking, client, _, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Completed);

            var result = await svc.CancelBookingAsync(client.UserId, booking.Id, "Too late");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot cancel");
        }

        [Fact]
        public async Task CancelBookingAsync_ByCaregiver_UpdatesCancelledByField()
        {
            var (svc, uow, _) = Build();
            var (booking, _, caregiver, clientWallet, _) = MakeConnectedBooking(uow, agreedAmount: 800m, status: BookingStatus.Accepted);

            var result = await svc.CancelBookingAsync(caregiver.UserId, booking.Id, "Unavailable");

            result.IsSuccess.Should().BeTrue();
            booking.CancelledBy.Should().Be("Caregiver");
        }

        // ── GetClientBookingsAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetClientBookingsAsync_ReturnsPagedBookingsForClient()
        {
            var (svc, uow, _) = Build();
            var clientUserId = Guid.NewGuid();
            var clientProfileId = Guid.NewGuid();

            uow.ClientProfileRepo.Data.Add(new ClientProfile
            {
                Id = clientProfileId,
                UserId = clientUserId,
                DateOfBirth = new DateTime(1990, 1, 1)
            });

            for (int i = 0; i < 5; i++)
            {
                uow.BookingRepo.Data.Add(new Booking
                {
                    Id = Guid.NewGuid(),
                    ClientProfileId = clientProfileId,
                    CaregiverProfileId = Guid.NewGuid(),
                    Status = BookingStatus.Pending,
                    BookingReference = $"REF-{i:D4}"
                });
            }

            var result = await svc.GetClientBookingsAsync(clientUserId, null, 1, 3);

            result.IsSuccess.Should().BeTrue();
            result.Data!.TotalCount.Should().Be(5);
            result.Data.Items.Should().HaveCount(3); // page size 3
        }

        // ── RaiseDisputeAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task RaiseDisputeAsync_OnCompletedBooking_CreatesTicketAndMarksDisputed()
        {
            var (svc, uow, _) = Build();
            var (booking, client, _, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Completed);

            var result = await svc.RaiseDisputeAsync(client.UserId, booking.Id, "Service was poor");

            result.IsSuccess.Should().BeTrue();
            booking.HasDispute.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Disputed);
            uow.SupportTicketRepo.Data.Should().HaveCount(1);
            uow.SupportTicketRepo.Data.First().IsDispute.Should().BeTrue();
        }

        [Fact]
        public async Task RaiseDisputeAsync_WhenDisputeAlreadyOpen_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            // Must be InProgress or Completed to pass the status check; HasDispute blocks the second path
            var (booking, client, _, _, _) = MakeConnectedBooking(uow, status: BookingStatus.InProgress);
            booking.HasDispute = true;

            var result = await svc.RaiseDisputeAsync(client.UserId, booking.Id, "Again");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("dispute is already open");
        }

        [Fact]
        public async Task RaiseDisputeAsync_OnPendingBooking_ReturnsFailure()
        {
            var (svc, uow, _) = Build();
            var (booking, client, _, _, _) = MakeConnectedBooking(uow, status: BookingStatus.Pending);

            var result = await svc.RaiseDisputeAsync(client.UserId, booking.Id, "Dispute too early");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("in-progress or completed");
        }
    }
}
