using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IEmailSender = HelpAtHome.Application.Interfaces.Services.IEmailSender;

namespace HelpAtHome.Application.Services
{

    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        //private readonly IFirebasePush _firebasePush;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork uow, 
            IEmailSender emailSender,
            //ISmsSender smsSender, 
            //IFirebasePush firebasePush,
            IConfiguration config, 
            ILogger<NotificationService> logger)
        {
            _uow = uow; _emailSender = emailSender; 
            //_smsSender = smsSender;
            //_firebasePush = firebasePush; 
            _config = config; _logger = logger;
        }

        public async Task SendAsync(Guid userId, string title, string body,
            string? type, string? referenceId,
            bool sendPush = true, bool sendEmail = false, bool sendSms = false)
        {
            // 1. Persist in-app notification
            var n = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                ReferenceId = referenceId
            };
            await _uow.Notifications.AddAsync(n);
            await _uow.SaveChangesAsync();

            // 2. Push (fire-and-forget; failure is logged, never throws)
            if (sendPush)
            {
                var user = await _uow.Users.GetByIdWithProfileAsync(userId);
                if (!string.IsNullOrEmpty(user?.FcmDeviceToken))
                    _ = SendPushAsync(user.FcmDeviceToken, title, body)
                        .ContinueWith(t => _logger.LogWarning(
                            "Push failed for {Id}: {Err}", userId, t.Exception?.Message),
                            TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                await _emailSender.SendEmailAsync(new EmailRequest
                {
                    To = new List<Recipient> { new Recipient { Email = toEmail, Name = toEmail } },
                    HtmlContent = htmlBody,
                    Sender = new Sender { Name = _config["Email:SenderName"], Email = _config["Email:SenderEmail"] },
                    Subject = subject
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email failed to {Email}", toEmail);
            }
        }

        public async Task SendSmsAsync(string toPhone, string message)
        {
            try 
            { 
                //await _smsSender.SendAsync(toPhone, message); 
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, "SMS failed to {Phone}", toPhone); 
            }
        }

        public async Task SendPushAsync(
            string fcmToken, string title, string body, object? data = null)
        {
            try 
            { 
                //await _firebasePush.SendAsync(fcmToken, title, body, data); 
            }
            catch (Exception ex)
            { _logger.LogError(ex, "Push failed to FCM token"); }
        }

        public Task SendEmailVerificationAsync(
            string toEmail, string token, string fullName)
        {
            var encoded = Uri.EscapeDataString(token);
            var verifyUrl = $"{_config["App:ClientBaseUrl"]}/verify-email?token={encoded}&email={toEmail}";
            var html = $"<p>Hi {fullName},</p>" +
                       "<p>Click below to verify your account:</p>" +
                       $"<p><a href='{verifyUrl}'>Verify Email</a></p>" +
                        "<p>This link expires in 24 hours.</p>";
            return SendEmailAsync(toEmail, "Verify Your Help At Home Account", html);
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string token, string fullName)
        {
            var encoded = Uri.EscapeDataString(token);
            var resetUrl = $"{_config["App:ClientBaseUrl"]}reset-password?token={encoded}&email={toEmail}";
            var html = $"<p>Hi {fullName},</p>" +
                       "<p>Click below to reset your password:</p>" +
                       $"<p><a href='{resetUrl}'>Reset Password</a></p>" +
                        "<p>If you did not request this, ignore this email.</p>";
            return SendEmailAsync(toEmail, "Reset Your Help At Home Password", html);
        }
    }




}
