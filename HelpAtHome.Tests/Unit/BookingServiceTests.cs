using AutoMapper;
using FluentAssertions;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Application.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace HelpAtHome.Tests.Unit;

public class BookingServiceTests
{
    // ── Shared mocks ───────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<INotificationService> _notification = new();

    private readonly Mock<IClientProfileRepository> _clientProfiles = new();
    private readonly Mock<ICaregiverProfileRepository> _caregiverProfiles = new();
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<ITransactionRepository> _transactions = new();

    private static IConfiguration MakeConfig(decimal commissionRate = 15m) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Platform:CommissionRate"] = commissionRate.ToString(),
                ["Platform:BookingReferencePrefix"] = "HAH",
            })
            .Build();

    public BookingServiceTests()
    {
        _uow.Setup(u => u.ClientProfiles).Returns(_clientProfiles.Object);
        _uow.Setup(u => u.CaregiverProfiles).Returns(_caregiverProfiles.Object);
        _uow.Setup(u => u.Bookings).Returns(_bookings.Object);
        _uow.Setup(u => u.Wallets).Returns(_wallets.Object);
        _uow.Setup(u => u.Transactions).Returns(_transactions.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _uow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        _uow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        _bookings.Setup(b => b.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
        _transactions.Setup(t => t.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
        _wallets.Setup(w => w.Update(It.IsAny<Wallet>()));
        _notification.Setup(n => n.SendAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
    }

    private BookingService CreateService(IConfiguration? config = null) =>
        new(_uow.Object, _mapper.Object, _notification.Object,
            config ?? MakeConfig(), NullLogger<BookingService>.Instance);

    // ── Fixture builders ───────────────────────────────────────────────────

    private static ClientProfile MakeClientProfile(Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        RequireVerifiedOnly = false,
    };

    private static CaregiverProfile MakeCaregiverProfile(
        bool isAvailable = true,
        VerificationStatus status = VerificationStatus.Approved,
        decimal hourlyRate = 2000,
        decimal dailyRate = 15000) => new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsAvailable = isAvailable,
            VerificationStatus = status,
            HourlyRate = hourlyRate,
            DailyRate = dailyRate,
            MonthlyRate = 200_000,
        };

    private static Booking MakeBookingEntity(
        Guid caregiverProfileId,
        ClientProfile clientProfile,
        BookingStatus status = BookingStatus.Pending) => new()
        {
            Id = Guid.NewGuid(),
            BookingReference = "HAH-TEST-001",
            Status = status,
            CaregiverProfileId = caregiverProfileId,
            ClientProfileId = clientProfile.Id,
            ClientProfile = clientProfile,   // navigation property needed for notifications
            AgreedAmount = 15_000,
            PlatformFee = 2_250,
            CaregiverEarnings = 12_750,
        };

    private static Wallet MakeWallet(Guid userId, decimal balance = 50_000) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Balance = balance,
        TotalSpent = 0,
    };

    private static CreateBookingDto MakeBookingDto(Guid caregiverId, Guid categoryId) => new()
    {
        CaregiverProfileId = caregiverId,
        ServiceCategoryId = categoryId,
        Frequency = FrequencyType.Daily,
        ScheduledStartDate = DateTime.UtcNow.AddDays(3),
        ScheduledEndDate = DateTime.UtcNow.AddDays(4),
    };

    private void SetupBookingMapper()
    {
        _mapper.Setup(m => m.Map<BookingDto>(It.IsAny<Booking>()))
            .Returns((Booking b) => new BookingDto
            {
                Id = b.Id,
                Status = b.Status.ToString(),
                AgreedAmount = b.AgreedAmount,
            });
    }

    // ── CreateBookingAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_ValidRequest_ReturnsOkWithPendingStatus()
    {
        var clientProfile = MakeClientProfile();
        var caregiver = MakeCaregiverProfile();
        var wallet = MakeWallet(clientProfile.UserId, 100_000);

        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);
        _bookings.Setup(b => b.HasActiveBookingAsync(caregiver.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _wallets.Setup(w => w.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(wallet);
        // Service calls GetWithDetailsAsync after saving to load navigations for mapping
        _bookings.Setup(b => b.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => MakeBookingEntity(caregiver.Id, clientProfile, BookingStatus.Pending));
        SetupBookingMapper();

        var svc = CreateService();

        var result = await svc.CreateBookingAsync(
            clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(BookingStatus.Pending.ToString());
    }

    [Fact]
    public async Task CreateBooking_ClientProfileMissing_ReturnsFail()
    {
        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync((ClientProfile?)null);

        var svc = CreateService();

        var result = await svc.CreateBookingAsync(Guid.NewGuid(), MakeBookingDto(Guid.NewGuid(), Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Client profile not found");
    }

    [Fact]
    public async Task CreateBooking_CaregiverUnavailable_ReturnsFail()
    {
        var clientProfile = MakeClientProfile();
        var caregiver = MakeCaregiverProfile(isAvailable: false);

        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);

        var svc = CreateService();

        var result = await svc.CreateBookingAsync(
            clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not available");
    }

    [Fact]
    public async Task CreateBooking_ClientRequiresVerified_UnverifiedCaregiver_ReturnsFail()
    {
        var clientProfile = MakeClientProfile();
        clientProfile.RequireVerifiedOnly = true;
        var caregiver = MakeCaregiverProfile(status: VerificationStatus.Pending);

        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);

        var svc = CreateService();

        var result = await svc.CreateBookingAsync(
            clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("verified");
    }

    [Fact]
    public async Task CreateBooking_CaregiverHasConflict_ReturnsFail()
    {
        var clientProfile = MakeClientProfile();
        var caregiver = MakeCaregiverProfile();

        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);
        _bookings.Setup(b => b.HasActiveBookingAsync(caregiver.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);  // conflict!

        var svc = CreateService();

        var result = await svc.CreateBookingAsync(
            clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("conflicting booking");
    }

    [Fact]
    public async Task CreateBooking_InsufficientBalance_ReturnsFail()
    {
        var clientProfile = MakeClientProfile();
        var caregiver = MakeCaregiverProfile(dailyRate: 50_000);
        var wallet = MakeWallet(clientProfile.UserId, balance: 1_000); // too low

        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);
        _bookings.Setup(b => b.HasActiveBookingAsync(caregiver.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _wallets.Setup(w => w.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(wallet);

        var svc = CreateService();

        var result = await svc.CreateBookingAsync(
            clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Insufficient");
    }

    [Fact]
    public async Task CreateBooking_DeductsWalletBalance()
    {
        var clientProfile = MakeClientProfile();
        var caregiver = MakeCaregiverProfile(dailyRate: 15_000);
        var wallet = MakeWallet(clientProfile.UserId, balance: 100_000);
        var initialBalance = wallet.Balance;

        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);
        _bookings.Setup(b => b.HasActiveBookingAsync(caregiver.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _wallets.Setup(w => w.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(wallet);
        _bookings.Setup(b => b.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => MakeBookingEntity(caregiver.Id, clientProfile));
        SetupBookingMapper();

        var svc = CreateService();

        await svc.CreateBookingAsync(clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        wallet.Balance.Should().BeLessThan(initialBalance, "payment should be deducted on booking creation");
    }

    [Fact]
    public async Task CreateBooking_CommissionCalculatedFromConfig()
    {
        var clientProfile = MakeClientProfile();
        var caregiver = MakeCaregiverProfile(dailyRate: 10_000);
        var wallet = MakeWallet(clientProfile.UserId, balance: 100_000);

        Booking? captured = null;
        _clientProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>()))
            .ReturnsAsync(clientProfile);
        _caregiverProfiles.Setup(r => r.GetByIdAsync(caregiver.Id)).ReturnsAsync(caregiver);
        _bookings.Setup(b => b.HasActiveBookingAsync(caregiver.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _wallets.Setup(w => w.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(wallet);
        _bookings.Setup(b => b.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => captured = b)
            .Returns(Task.CompletedTask);
        _bookings.Setup(b => b.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => MakeBookingEntity(caregiver.Id, clientProfile));
        SetupBookingMapper();

        var svc = CreateService(MakeConfig(commissionRate: 15));

        await svc.CreateBookingAsync(clientProfile.UserId, MakeBookingDto(caregiver.Id, Guid.NewGuid()));

        captured.Should().NotBeNull();
        captured!.PlatformFee.Should().Be(captured.AgreedAmount * 0.15m);
        captured.CaregiverEarnings.Should().Be(captured.AgreedAmount - captured.PlatformFee);
    }

    // ── AcceptBookingAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AcceptBooking_PendingBooking_ChangesStatusToAccepted()
    {
        var caregiverUserId = Guid.NewGuid();
        var clientProfile = MakeClientProfile();
        var caregiverProfile = MakeCaregiverProfile();
        caregiverProfile.UserId = caregiverUserId;

        var booking = MakeBookingEntity(caregiverProfile.Id, clientProfile, BookingStatus.Pending);

        // AcceptBookingAsync calls GetWithDetailsAsync (not GetByIdAsync)
        _bookings.Setup(b => b.GetWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
        _caregiverProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<CaregiverProfile, bool>>>()))
            .ReturnsAsync(caregiverProfile);
        _bookings.Setup(b => b.Update(It.IsAny<Booking>()));
        SetupBookingMapper();

        var svc = CreateService();

        var result = await svc.AcceptBookingAsync(caregiverUserId, booking.Id);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Accepted);
        booking.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AcceptBooking_AlreadyAccepted_ReturnsFail()
    {
        var caregiverUserId = Guid.NewGuid();
        var clientProfile = MakeClientProfile();
        var caregiverProfile = MakeCaregiverProfile();
        caregiverProfile.UserId = caregiverUserId;

        var booking = MakeBookingEntity(caregiverProfile.Id, clientProfile, BookingStatus.Accepted);

        _bookings.Setup(b => b.GetWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
        _caregiverProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<CaregiverProfile, bool>>>()))
            .ReturnsAsync(caregiverProfile);

        var svc = CreateService();

        var result = await svc.AcceptBookingAsync(caregiverUserId, booking.Id);

        result.IsSuccess.Should().BeFalse();
    }

    // ── StartBookingAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task StartBooking_AcceptedBooking_ChangesStatusToInProgress()
    {
        var caregiverUserId = Guid.NewGuid();
        var clientProfile = MakeClientProfile();
        var caregiverProfile = MakeCaregiverProfile();
        caregiverProfile.UserId = caregiverUserId;

        var booking = MakeBookingEntity(caregiverProfile.Id, clientProfile, BookingStatus.Accepted);

        _bookings.Setup(b => b.GetWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
        _caregiverProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<CaregiverProfile, bool>>>()))
            .ReturnsAsync(caregiverProfile);
        _bookings.Setup(b => b.Update(It.IsAny<Booking>()));
        SetupBookingMapper();

        var svc = CreateService();

        var result = await svc.StartBookingAsync(caregiverUserId, booking.Id);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.InProgress);
        booking.StartedAt.Should().NotBeNull();
    }

    // ── CompleteBookingAsync ───────────────────────────────────────────────

    [Fact]
    public async Task CompleteBooking_InProgressBooking_ChangesStatusToCompleted()
    {
        var caregiverUserId = Guid.NewGuid();
        var clientProfile = MakeClientProfile();
        var caregiverProfile = MakeCaregiverProfile();
        caregiverProfile.UserId = caregiverUserId;

        var caregiverWallet = new Wallet { Id = Guid.NewGuid(), UserId = caregiverUserId, Balance = 0 };
        var booking = MakeBookingEntity(caregiverProfile.Id, clientProfile, BookingStatus.InProgress);
        booking.CaregiverEarnings = 8_500;

        _bookings.Setup(b => b.GetWithDetailsAsync(booking.Id)).ReturnsAsync(booking);
        _caregiverProfiles.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<CaregiverProfile, bool>>>()))
            .ReturnsAsync(caregiverProfile);
        _caregiverProfiles.Setup(r => r.Update(It.IsAny<CaregiverProfile>()));
        _bookings.Setup(b => b.Update(It.IsAny<Booking>()));
        _wallets.Setup(w => w.FirstOrDefaultAsync(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync(caregiverWallet);
        _wallets.Setup(w => w.Update(It.IsAny<Wallet>()));
        SetupBookingMapper();

        var svc = CreateService();

        var result = await svc.CompleteBookingAsync(caregiverUserId, booking.Id);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Completed);
        booking.CompletedAt.Should().NotBeNull();
    }
}
