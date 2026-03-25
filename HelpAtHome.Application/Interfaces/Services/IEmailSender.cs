namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IEmailSender 
    { 
        Task SendAsync(string to, string subject, string html); 
    }

}
