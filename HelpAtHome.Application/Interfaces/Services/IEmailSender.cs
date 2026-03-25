using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IEmailSender 
    { 
        Task SendAsync(string to, string subject, string html);
        //Task<Result<string>> SendEmailAsync(EmailRequestDto mailRequest);
        Task<Result<string>> SendEmailAsync(EmailRequest requestDto);
    }
}
