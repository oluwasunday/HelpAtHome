using HelpAtHome.Application.Interfaces.Services;
using System.Text.Json;

namespace HelpAtHome.Application.Services
{
    public class FirebasePushSender : IFirebasePush
    {
        public async Task SendAsync(
            string fcmToken, string title, string body, object? data = null)
        {
            /*var message = new Message
            {
                Token = fcmToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                { Title = title, Body = body },
                Data = data is null ? null
                       : JsonSerializer.Deserialize<Dictionary<string, string>>(
                           JsonSerializer.Serialize(data))
            };
            await FirebaseMessaging.DefaultInstance.SendAsync(message);*/
        }
    }







    /*public class AuthService2 : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notification;
        private readonly IAuditLogService _audit;
        private readonly IMapper _mapper;

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, string ip)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return Result<AuthResponseDto>.Fail("Invalid credentials");
            if (user.IsSuspended)
                return Result<AuthResponseDto>.Fail("Account suspended: " + user.SuspensionReason);
            var signIn = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);
            if (signIn.IsLockedOut)
                return Result<AuthResponseDto>.Fail($"Account locked until {user.LockoutEnd?.UtcDateTime:HH:mm UTC}.");
            if (!signIn.Succeeded)
                return Result<AuthResponseDto>.Fail("Invalid credentials");
            user.LastLoginAt = DateTime.UtcNow; user.LastLoginIp = ip;
            await _userManager.UpdateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user, roles, claims);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _uow.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IpAddress = ip
            });
            await _uow.SaveChangesAsync();
            await _audit.LogAsync(user.Id.ToString(), user.Role.ToString(),
                AuditAction.Login, "User", user.Id.ToString(), ipAddress: ip);
            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email!,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        public async Task<Result> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Result.Ok();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _notification.SendPasswordResetEmailAsync(user.Email!, token, user.FullName);
            return Result.Ok();
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found");
            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            return result.Succeeded ? Result.Ok()
                : Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }*/

}
