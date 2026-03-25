using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options; // Add this using directive at the top of the file
using MimeKit;
using Newtonsoft.Json.Linq;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Mail;

namespace HelpAtHome.Application.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        private readonly EmailSettings _emailSettings;
        private readonly HttpClient _client;

        public MailKitEmailSender(IConfiguration cfg, IOptions<EmailSettings> options, IHttpClientFactory httpClientFactory)
        {
            _cfg = cfg;
            _emailSettings = options.Value;
            _client = httpClientFactory.CreateClient("Brevo");
        }

        public async System.Threading.Tasks.Task SendAsync(string to, string subject, string html)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_cfg["Email:SenderName"], _cfg["Email:SenderEmail"]));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_cfg["Email:SmtpHost"], int.Parse(_cfg["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_cfg["Email:SmtpUser"], _cfg["Email:SmtpPassword"]);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }

        public async Task<Result<string>> SendEmailAsync(EmailRequestDto mailRequest)
        {
            try
            {
                var apiInstance = new TransactionalEmailsApi();
                string SenderName = _emailSettings.DisplayName;
                string SenderEmail = _emailSettings.Mail;
                SendSmtpEmailSender Email = new SendSmtpEmailSender(SenderName, SenderEmail);

                SendSmtpEmailTo receiver1 = new SendSmtpEmailTo(mailRequest.ToEmail, mailRequest.ToEmail);
                List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
                To.Add(receiver1);


                string HtmlContent = null;
                string TextContent = mailRequest.Body;
                
                try
                {
                    var sendSmtpEmail = new SendSmtpEmail(Email, To, null, null, HtmlContent, TextContent, mailRequest.Subject);
                    CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                    Debug.WriteLine(result.ToJson());
                    Console.WriteLine(result.ToJson());

                    return Result<string>.Ok("Mail successful!");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Console.WriteLine(e.Message);
                    //Console.ReadLine();

                    return Result<string>.Fail("failed to send mail");
                }
            }
            catch (Exception e)
            {
                return Result<string>.Fail($"something went wrong - {e}");
            }
        }

        public async Task<Result<string>> SendEmailAsync(EmailRequest requestDto)
        {
            try
            {
                try
                {
                    var payload = new
                    {
                        htmlContent = requestDto.HtmlContent,
                        sender = requestDto.Sender,
                        subject = requestDto.Subject,
                        to = requestDto.To,
                    };

                    var response = await _client.PostAsJsonAsync("smtp/email", payload);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();

                    return Result<string>.Ok("Mail successful!");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return Result<string>.Fail("failed to send mail");
                }
            }
            catch (Exception e)
            {
                return Result<string>.Fail($"something went wrong - {e}");
            }
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
