namespace HelpAtHome.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(Guid userId, string title, string body,
            string? type, string? referenceId,
            bool sendPush = true, bool sendEmail = false, bool sendSms = false);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendSmsAsync(string toPhone, string message);
        Task SendPushAsync(string fcmToken, string title, string body, object? data = null);
        Task SendEmailVerificationAsync(string toEmail, string token, string fullName);
        Task SendPasswordResetEmailAsync(string toEmail, string token, string fullName);
    }

}
