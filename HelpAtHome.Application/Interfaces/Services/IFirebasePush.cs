namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IFirebasePush
    {
        Task SendAsync(string fcmToken, string title, string body, object? data = null);
    }

}
