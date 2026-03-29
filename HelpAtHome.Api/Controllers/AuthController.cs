using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) { _authService = authService; }

        [HttpPost("register/client")]
        public async Task<IActionResult> RegisterClient([FromBody] RegisterClientDto dto)
        {
            var result = await _authService.RegisterClientAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("register/caregiver")]
        public async Task<IActionResult> RegisterCaregiver([FromBody] RegisterCaregiverDto dto)
        {
            var result = await _authService.RegisterIndividualCaregiverAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("register/agency-admin")]
        public async Task<IActionResult> RegisterAgencyAdmin([FromBody] RegisterAgencyAdminDto dto)
        {
            var result = await _authService.RegisterAgencyAdminAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(dto, ip);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ip);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.LogoutAsync(userId, refreshToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.VerifyEmailOtpAsync(userId, dto.Otp);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("resend-email-otp")]
        public async Task<IActionResult> ResendEmailOtp()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.SendEmailOtpAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("send-phone-otp")]
        public async Task<IActionResult> SendPhoneOtp()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.SendPhoneOtpAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyOtpDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.VerifyPhoneOtpAsync(userId, dto.Otp);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "If the email exists, a reset link has been sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                //
            }
            var result = await _authService.ResetPasswordAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

}
