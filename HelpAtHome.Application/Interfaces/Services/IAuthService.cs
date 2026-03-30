using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.DTOs.Responses.Auth;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterClientAsync(RegisterClientDto dto);
        Task<Result<AuthResponseDto>> RegisterIndividualCaregiverAsync(RegisterCaregiverDto dto);
        Task<Result<AuthResponseDto>> RegisterAgencyAdminAsync(RegisterAgencyAdminDto dto);
        Task<Result<Guid>> RegisterAgencyCaregiverAsync(RegisterAgencyCaregiverDto dto, Guid agencyId);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, string ipAddress);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<Result> LogoutAsync(Guid userId, string refreshToken);
        Task<Result> SendEmailOtpAsync(Guid userId);
        Task<Result> VerifyEmailOtpAsync(Guid userId, string otp);
        Task<Result> SendPhoneOtpAsync(Guid userId);
        Task<Result> VerifyPhoneOtpAsync(Guid userId, string otp);
        Task<Result> ForgotPasswordAsync(string email);
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
        Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}
