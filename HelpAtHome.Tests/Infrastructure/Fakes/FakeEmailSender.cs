using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Shared;

namespace HelpAtHome.Tests.Infrastructure.Fakes;

/// <summary>No-op email sender — swallows all emails in the test environment.</summary>
public class FakeEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string html) => Task.CompletedTask;

    public Task<Result<string>> SendEmailAsync(EmailRequest requestDto)
        => Task.FromResult(Result<string>.Ok("fake-message-id"));
}
