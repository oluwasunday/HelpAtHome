using FluentAssertions;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Application.Services;
using HelpAtHome.Core.DTOs.Common;
using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace HelpAtHome.Tests.Services
{
    public class AuthServiceTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────

        private static Mock<UserManager<User>> BuildUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var mgr = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            mgr.Setup(m => m.UpdateAsync(It.IsAny<User>()))
               .ReturnsAsync(IdentityResult.Success);
            mgr.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
               .ReturnsAsync(new List<string> { "Client" });
            mgr.Setup(m => m.GetClaimsAsync(It.IsAny<User>()))
               .ReturnsAsync(new List<Claim>());
            return mgr;
        }

        private static Mock<SignInManager<User>> BuildSignInManager(Mock<UserManager<User>> um)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(
                um.Object, contextAccessor.Object, claimsFactory.Object,
                null, null, null, null);
        }

        private static (AuthService svc, FakeUnitOfWork uow, Mock<UserManager<User>> um, Mock<IJwtService> jwt, Mock<INotificationService> notification) Build()
        {
            var uow = new FakeUnitOfWork();
            var um = BuildUserManager();
            var sim = BuildSignInManager(um);
            var jwt = new Mock<IJwtService>();
            jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
               .Returns("access-token");
            jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

            var notification = new Mock<INotificationService>();
            notification.Setup(n => n.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);
            notification.Setup(n => n.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);
            notification.Setup(n => n.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            var mapper = new Mock<AutoMapper.IMapper>();
            var config = new ConfigurationBuilder().Build();
            var svc = new AuthService(uow, um.Object, sim.Object, jwt.Object, notification.Object, mapper.Object, config);
            return (svc, uow, um, jwt, notification);
        }

        private static RegisterClientDto ValidClientDto(string email = "client@test.com", string phone = "+2348000000001") => new()
        {
            FirstName = "Ada",
            LastName = "Obi",
            Email = email,
            PhoneNumber = phone,
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Gender.Female,
            Address = new AddressUpsertDto
            {
                Line1 = "12 Lagos Road",
                Locality = "Ikeja",
                City = "Lagos",
                State = "Lagos",
                Country = "Nigeria"
            }
        };

        private static User MakeUser(string email, UserRole role = UserRole.Client) => new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+2348000000001",
            Role = role,
            IsActive = true
        };

        // ── Login ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAccessAndRefreshTokens()
        {
            var (svc, uow, um, jwt, _) = Build();
            var user = MakeUser("user@example.com");

            um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Client" });
            um.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

            var sim = BuildSignInManager(um);
            sim.Setup(s => s.CheckPasswordSignInAsync(user, "Password1!", true))
               .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Rebuild service with updated sign-in manager
            var mapper = new Mock<AutoMapper.IMapper>();
            var notification = new Mock<INotificationService>();
            var loginSvc = new AuthService(uow, um.Object, sim.Object, jwt.Object, notification.Object, mapper.Object, new ConfigurationBuilder().Build());

            var result = await loginSvc.LoginAsync(new LoginDto { Email = user.Email!, Password = "Password1!" }, "127.0.0.1");

            result.IsSuccess.Should().BeTrue();
            result.Data!.AccessToken.Should().Be("access-token");
            result.Data.RefreshToken.Should().Be("refresh-token");
            result.Data.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ReturnsFailure()
        {
            var (svc, _, um, _, _) = Build();
            um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await svc.LoginAsync(new LoginDto { Email = "nobody@test.com", Password = "x" }, "127.0.0.1");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task LoginAsync_WhenAccountIsSuspended_ReturnsFailure()
        {
            var (svc, _, um, _, _) = Build();
            var user = MakeUser("sus@test.com");
            user.IsSuspended = true;
            user.SuspensionReason = "Policy violation";

            um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

            var result = await svc.LoginAsync(new LoginDto { Email = user.Email!, Password = "pass" }, "ip");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("suspended");
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsWrong_ReturnsFailure()
        {
            var (svc, uow, um, jwt, notification) = Build();
            var user = MakeUser("user@example.com");

            var sim = BuildSignInManager(um);
            sim.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
               .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);
            um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

            var mapper = new Mock<AutoMapper.IMapper>();
            var loginSvc = new AuthService(uow, um.Object, sim.Object, jwt.Object, notification.Object, mapper.Object, new ConfigurationBuilder().Build());

            var result = await loginSvc.LoginAsync(new LoginDto { Email = user.Email!, Password = "wrong" }, "ip");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task LoginAsync_WhenAccountIsLockedOut_ReturnsFailure()
        {
            var (svc, uow, um, jwt, notification) = Build();
            var user = MakeUser("user@example.com");
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10);

            var sim = BuildSignInManager(um);
            sim.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
               .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);
            um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

            var mapper = new Mock<AutoMapper.IMapper>();
            var loginSvc = new AuthService(uow, um.Object, sim.Object, jwt.Object, notification.Object, mapper.Object, new ConfigurationBuilder().Build());

            var result = await loginSvc.LoginAsync(new LoginDto { Email = user.Email!, Password = "pass" }, "ip");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("locked");
        }

        // ── Register client ───────────────────────────────────────────────────

        [Fact]
        public async Task RegisterClientAsync_WithNewEmailAndPhone_ReturnsTokens()
        {
            var (svc, uow, um, jwt, _) = Build();
            var dto = ValidClientDto();

            um.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            um.Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            um.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            um.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
              .ReturnsAsync((string id) => new User
              {
                  Id = Guid.Parse(id),
                  Email = dto.Email,
                  FirstName = dto.FirstName,
                  LastName = dto.LastName,
                  PhoneNumber = dto.PhoneNumber
              });

            var result = await svc.RegisterClientAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Data!.AccessToken.Should().Be("access-token");
            result.Data.RefreshToken.Should().Be("refresh-token");
            uow.WalletRepo.Data.Should().HaveCount(1);
            uow.RefreshTokenRepo.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task RegisterClientAsync_WithDuplicateEmail_ReturnsFailure()
        {
            var (svc, uow, um, _, _) = Build();
            var dto = ValidClientDto();
            var existing = MakeUser(dto.Email);

            um.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(existing);

            var result = await svc.RegisterClientAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Email is already registered");
        }

        [Fact]
        public async Task RegisterClientAsync_WithDuplicatePhone_ReturnsFailure()
        {
            var (svc, uow, um, _, _) = Build();
            var dto = ValidClientDto();

            um.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            uow.UserRepo.Data.Add(new User { PhoneNumber = dto.PhoneNumber, Id = Guid.NewGuid() });

            var result = await svc.RegisterClientAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Phone number is already registered");
        }

        [Fact]
        public async Task RegisterClientAsync_WhenIdentityCreateFails_ReturnsFailure()
        {
            var (svc, uow, um, _, _) = Build();
            var dto = ValidClientDto();

            um.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            um.Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
              .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            var result = await svc.RegisterClientAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Password too weak");
        }

        // ── Refresh token ─────────────────────────────────────────────────────

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokenPair()
        {
            var (svc, uow, um, jwt, _) = Build();
            var user = MakeUser("user@test.com");
            var tokenValue = "valid-refresh-token";

            uow.RefreshTokenRepo.Data.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = tokenValue,
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Client" });
            um.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

            var result = await svc.RefreshTokenAsync(tokenValue, "127.0.0.1");

            result.IsSuccess.Should().BeTrue();
            result.Data!.AccessToken.Should().Be("access-token");
            result.Data.RefreshToken.Should().Be("refresh-token");
            // Old token should be revoked
            uow.RefreshTokenRepo.Data.First(t => t.Token == tokenValue).IsRevoked.Should().BeTrue();
            // New token should be in store
            uow.RefreshTokenRepo.Data.Should().Contain(t => t.Token == "refresh-token");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ReturnsFailure()
        {
            var (svc, uow, _, _, _) = Build();

            uow.RefreshTokenRepo.Data.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "old-token",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(-1) // expired
            });

            var result = await svc.RefreshTokenAsync("old-token", "ip");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid or expired");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithRevokedToken_ReturnsFailure()
        {
            var (svc, uow, _, _, _) = Build();

            uow.RefreshTokenRepo.Data.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "revoked-token",
                IsRevoked = true,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            var result = await svc.RefreshTokenAsync("revoked-token", "ip");

            result.IsSuccess.Should().BeFalse();
        }

        // ── Logout ────────────────────────────────────────────────────────────

        [Fact]
        public async Task LogoutAsync_WithActiveToken_RevokesToken()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();
            var tokenValue = "my-token";

            uow.RefreshTokenRepo.Data.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = tokenValue,
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            var result = await svc.LogoutAsync(userId, tokenValue);

            result.IsSuccess.Should().BeTrue();
            uow.RefreshTokenRepo.Data.First().IsRevoked.Should().BeTrue();
            uow.RefreshTokenRepo.Data.First().RevokedReason.Should().Be("Logout");
        }

        [Fact]
        public async Task LogoutAsync_WithAlreadyRevokedToken_ReturnsOkIdempotently()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();

            uow.RefreshTokenRepo.Data.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "already-revoked",
                IsRevoked = true,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            var result = await svc.LogoutAsync(userId, "already-revoked");

            result.IsSuccess.Should().BeTrue();
        }

        // ── Email OTP ─────────────────────────────────────────────────────────

        [Fact]
        public async Task SendEmailOtpAsync_WithValidUser_StoresOtpAndSendsEmail()
        {
            var (svc, uow, um, _, notification) = Build();
            var user = MakeUser("otp@test.com");

            um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

            var result = await svc.SendEmailOtpAsync(user.Id);

            result.IsSuccess.Should().BeTrue();
            uow.OtpCodeRepo.Data.Should().HaveCount(1);
            uow.OtpCodeRepo.Data.First().Purpose.Should().Be("EmailVerify");
            uow.OtpCodeRepo.Data.First().IsUsed.Should().BeFalse();
            notification.Verify(n => n.SendEmailAsync(user.Email!, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendEmailOtpAsync_InvalidatesExistingActiveOtps()
        {
            var (svc, uow, um, _, _) = Build();
            var user = MakeUser("otp@test.com");

            // Pre-existing active OTP
            uow.OtpCodeRepo.Data.Add(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Code = "111111",
                Purpose = "EmailVerify",
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            });

            um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

            await svc.SendEmailOtpAsync(user.Id);

            // Old OTP should be invalidated
            uow.OtpCodeRepo.Data.First(o => o.Code == "111111").IsUsed.Should().BeTrue();
            // New OTP should also exist
            uow.OtpCodeRepo.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task SendEmailOtpAsync_WithNonExistentUser_ReturnsFailure()
        {
            var (svc, _, um, _, _) = Build();
            um.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await svc.SendEmailOtpAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("User not found");
        }

        // ── Verify email OTP ──────────────────────────────────────────────────

        [Fact]
        public async Task VerifyEmailOtpAsync_WithCorrectCode_ConfirmsEmailAndReturnsOk()
        {
            var (svc, uow, um, _, _) = Build();
            var userId = Guid.NewGuid();
            var code = "123456";

            uow.OtpCodeRepo.Data.Add(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = code,
                Purpose = "EmailVerify",
                IsUsed = false,
                Attempts = 0,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });

            var user = new User { Id = userId, Email = "u@test.com", FirstName = "U", LastName = "T" };
            um.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            um.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await svc.VerifyEmailOtpAsync(userId, code);

            result.IsSuccess.Should().BeTrue();
            uow.OtpCodeRepo.Data.First().IsUsed.Should().BeTrue();
            user.EmailConfirmed.Should().BeTrue();
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyEmailOtpAsync_WithWrongCode_ReturnsIncorrectCodeError()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();

            uow.OtpCodeRepo.Data.Add(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "999999",
                Purpose = "EmailVerify",
                IsUsed = false,
                Attempts = 0,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });

            var result = await svc.VerifyEmailOtpAsync(userId, "000000");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Incorrect code");
        }

        [Fact]
        public async Task VerifyEmailOtpAsync_WhenAttemptLimitReached_BurnsOtpAndReturnsFailure()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();

            uow.OtpCodeRepo.Data.Add(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "999999",
                Purpose = "EmailVerify",
                IsUsed = false,
                Attempts = 4, // next increment hits 5 → burned
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });

            var result = await svc.VerifyEmailOtpAsync(userId, "000000");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Too many failed attempts");
            uow.OtpCodeRepo.Data.First().IsUsed.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyEmailOtpAsync_WhenNoActiveOtpExists_ReturnsFailure()
        {
            var (svc, uow, _, _, _) = Build();
            var userId = Guid.NewGuid();

            // OTP is expired
            uow.OtpCodeRepo.Data.Add(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "123456",
                Purpose = "EmailVerify",
                IsUsed = false,
                Attempts = 0,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5) // already expired
            });

            var result = await svc.VerifyEmailOtpAsync(userId, "123456");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid or expired");
        }

        // ── Forgot / reset password ───────────────────────────────────────────

        [Fact]
        public async Task ForgotPasswordAsync_WithExistingEmail_SendsResetEmail()
        {
            var (svc, _, um, _, notification) = Build();
            var user = MakeUser("reset@test.com");

            um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            um.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

            var result = await svc.ForgotPasswordAsync(user.Email!);

            result.IsSuccess.Should().BeTrue();
            notification.Verify(n => n.SendPasswordResetEmailAsync(user.Email!, "reset-token", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsOkSilently()
        {
            var (svc, _, um, _, notification) = Build();
            um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await svc.ForgotPasswordAsync("ghost@test.com");

            result.IsSuccess.Should().BeTrue();
            notification.Verify(n => n.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithCorrectOldPassword_ReturnsOk()
        {
            var (svc, _, um, _, _) = Build();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var dto = new ChangePasswordDto { OldPassword = "Old1!", NewPassword = "New1!" };

            um.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            um.Setup(m => m.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword))
              .ReturnsAsync(IdentityResult.Success);

            var result = await svc.ChangePasswordAsync(userId, dto);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ChangePasswordAsync_WithWrongOldPassword_ReturnsFailure()
        {
            var (svc, _, um, _, _) = Build();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var dto = new ChangePasswordDto { OldPassword = "WrongOld!", NewPassword = "New1!" };

            um.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            um.Setup(m => m.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword))
              .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

            var result = await svc.ChangePasswordAsync(userId, dto);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Incorrect password");
        }
    }
}
