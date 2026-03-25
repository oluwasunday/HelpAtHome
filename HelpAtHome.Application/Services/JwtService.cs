using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HelpAtHome.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config) { _config = config; }

        public string GenerateAccessToken(
            User user, IList<string> roles, IList<Claim> extraClaims)
        {
            var cfg = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(cfg["Key"]!));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new("firstName",    user.FirstName),
                new("lastName",     user.LastName),
                new("role",         user.Role.ToString()),
            
                // SecurityStamp ensures token is void after password/role change
                new("securityStamp", user.SecurityStamp ?? string.Empty)
            };
            // Add Identity role claims (required for [Authorize(Roles="...")])
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.AddRange(extraClaims);

            var expiryMinutes = int.Parse(cfg["ExpiryMinutes"] ?? "60");
            var token = new JwtSecurityToken(
                issuer: cfg["Issuer"],
                audience: cfg["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        // Used to read claims from an expired token (for silent refresh validation)
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var cfg = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(cfg["Key"]!));
            var vp = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,   // intentionally ignore expiry
                ValidIssuer = cfg["Issuer"],
                ValidAudience = cfg["Audience"],
                IssuerSigningKey = key
            };
            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, vp, out var validated);
                if (validated is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.OrdinalIgnoreCase))
                    return null;
                return principal;
            }
            catch { return null; }
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
