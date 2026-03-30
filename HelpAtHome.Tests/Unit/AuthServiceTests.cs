using FluentAssertions;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Application.Services;
using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HelpAtHome.Tests.Unit;

public class AuthServiceTests
{
    // ── Mocks & helpers ────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<UserManager<User>> _userMgr;
    private readonly Mock<SignInManager<User>> _signInMgr;
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<INotificationService> _notification = new();
    private readonly Mock<AutoMapper.IMapper> _mapper = new();

    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IClientProfileRepository> _clientProfileRepo = new();
    private readonly Mock<IWalletRepository> _walletRepo = new();
    private readonly Mock<IClientAddressRepository> _clientAddressRepo = new();
    private readonly Mock<ICaregiverProfileRepository> _caregiverProfileRepo = new();
    private readonly Mock<ICaregiverAddressRepository> _caregiverAddressRepo = new();

    public AuthServiceTests()
    {
        // UserManager requires IUserStore; everything else can be null
        var storeMock = new Mock<IUserStore<User>>();
        _userMgr = new Mock<UserManager<User>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInMgr = new Mock<SignInManager<User>>(
            _userMgr.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        // Wire UoW repository properties
        _uow.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepo.Object);
        _uow.Setup(u => u.ClientProfiles).Returns(_clientProfileRepo.Object);
        _uow.Setup(u => u.Wallets).Returns(_walletRepo.Object);
        _uow.Setup(u => u.ClientAddresses).Returns(_clientAddressRepo.Object);
        _uow.Setup(u => u.CaregiverProfiles).Returns(_caregiverProfileRepo.Object);
        _uow.Setup(u => u.CaregiverAddresses).Returns(_caregiverAddressRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Notification is fire-and-forget; always succeed regardless of which method is called
        _notification
            .Setup(n => n.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _notification
            .Setup(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        _notification
            .Setup(n => n.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private AuthService CreateService() =>
        new(_uow.Object, _userMgr.Object, _signInMgr.Object,
            _jwt.Object, _notification.Object, _mapper.Object);

    private static User MakeUser(string email = "test@test.com", bool isDeleted = false) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Client,
            IsDeleted = isDeleted,
            IsSuspended = false,
            SecurityStamp = "stamp"
        };

    // ── LoginAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsOkWithTokens()
    {
        var user = MakeUser("valid@test.com");

        _userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, "Pass@1234", true))
            .ReturnsAsync(SignInResult.Success);
        _userMgr.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userMgr.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "Client" });
        _userMgr.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(Array.Empty<Claim>());
        _refreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        _jwt.Setup(j => j.GenerateAccessToken(user, It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
            .Returns("access.token.here");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token-here");

        var svc = CreateService();

        var result = await svc.LoginAsync(new LoginDto { Email = user.Email!, Password = "Pass@1234" }, "127.0.0.1");

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("access.token.here");
        result.Data.RefreshToken.Should().Be("refresh-token-here");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailure()
    {
        _userMgr.Setup(m => m.FindByEmailAsync("ghost@test.com")).ReturnsAsync((User?)null);

        var svc = CreateService();

        var result = await svc.LoginAsync(
            new LoginDto { Email = "ghost@test.com", Password = "anything" }, "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_SoftDeletedUser_ReturnsFailure()
    {
        var user = MakeUser("deleted@test.com", isDeleted: true);
        _userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var svc = CreateService();

        var result = await svc.LoginAsync(
            new LoginDto { Email = user.Email!, Password = "Pass@1234" }, "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        var user = MakeUser("wrongpass@test.com");

        _userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
            .ReturnsAsync(SignInResult.Failed);

        var svc = CreateService();

        var result = await svc.LoginAsync(
            new LoginDto { Email = user.Email!, Password = "BadPass1!" }, "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_LockedOutUser_ReturnsFailureWithLockoutMessage()
    {
        var user = MakeUser("locked@test.com");
        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(25);

        _userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
            .ReturnsAsync(SignInResult.LockedOut);

        var svc = CreateService();

        var result = await svc.LoginAsync(
            new LoginDto { Email = user.Email!, Password = "Pass@1234" }, "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked");
    }

    [Fact]
    public async Task LoginAsync_SuspendedUser_ReturnsFailure()
    {
        var user = MakeUser("suspended@test.com");
        user.IsSuspended = true;
        user.SuspensionReason = "Policy violation";

        _userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var svc = CreateService();

        var result = await svc.LoginAsync(
            new LoginDto { Email = user.Email!, Password = "Pass@1234" }, "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("suspended");
    }

    // ── LogoutAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ValidToken_MarksTokenRevoked()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            UserId = userId,
            Token = "my-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            IsRevoked = false
        };

        var refreshTokenRepo = new Mock<IRefreshTokenRepository>();
        refreshTokenRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(token);
        refreshTokenRepo.Setup(r => r.Update(It.IsAny<RefreshToken>()));
        _uow.Setup(u => u.RefreshTokens).Returns(refreshTokenRepo.Object);

        var svc = CreateService();

        var result = await svc.LogoutAsync(userId, "my-refresh-token");

        result.IsSuccess.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
        token.RevokedReason.Should().Be("Logout");
    }

    // ── ForgotPasswordAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_UnknownEmail_ReturnsOk_DoesNotLeakInfo()
    {
        _userMgr.Setup(m => m.FindByEmailAsync("unknown@test.com")).ReturnsAsync((User?)null);

        var svc = CreateService();

        var result = await svc.ForgotPasswordAsync("unknown@test.com");

        // Must not reveal whether the email exists — always succeed
        result.IsSuccess.Should().BeTrue();
    }
}
