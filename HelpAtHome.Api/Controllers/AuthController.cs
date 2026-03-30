using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Authentication — registration, login, token management, and OTP verification.</summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) { _authService = authService; }

        /// <summary>Register a new client account.</summary>
        [HttpPost("register/client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterClient([FromBody] RegisterClientDto dto)
        {
            var result = await _authService.RegisterClientAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Register a new individual caregiver account.</summary>
        [HttpPost("register/caregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterCaregiver([FromBody] RegisterCaregiverDto dto)
        {
            var result = await _authService.RegisterIndividualCaregiverAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Register a new agency admin account.</summary>
        [HttpPost("register/agency-admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAgencyAdmin([FromBody] RegisterAgencyAdminDto dto)
        {
            var result = await _authService.RegisterAgencyAdminAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Authenticate and receive a JWT access token plus refresh token.</summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(dto, ip);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        /// <summary>Exchange a valid refresh token for a new access/refresh token pair.</summary>
        /// <remarks>The old refresh token is immediately revoked (rotation).</remarks>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ip);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        /// <summary>Revoke the current refresh token and end the session.</summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.LogoutAsync(userId, refreshToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Verify the email OTP sent to the authenticated user's email address.</summary>
        [Authorize]
        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.VerifyEmailOtpAsync(userId, dto.Otp);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Re-send the email verification OTP to the authenticated user.</summary>
        [Authorize]
        [HttpPost("resend-email-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendEmailOtp()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.SendEmailOtpAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Send a phone verification OTP via SMS to the authenticated user's number.</summary>
        [Authorize]
        [HttpPost("send-phone-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendPhoneOtp()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.SendPhoneOtpAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Verify the phone OTP sent to the authenticated user's phone number.</summary>
        [Authorize]
        [HttpPost("verify-phone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyOtpDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var result = await _authService.VerifyPhoneOtpAsync(userId, dto.Otp);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Request a password reset link to be sent to the given email address.</summary>
        /// <remarks>Always returns 200 to avoid user enumeration — the email is only sent if the address exists.</remarks>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "If the email exists, a reset link has been sent" });
        }

        /// <summary>Reset the user's password using the OTP / token received via email.</summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
