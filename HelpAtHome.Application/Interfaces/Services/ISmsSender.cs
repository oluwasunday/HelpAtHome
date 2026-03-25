namespace HelpAtHome.Application.Interfaces.Services
{
    public interface ISmsSender { Task SendAsync(string toPhone, string message); }
}
