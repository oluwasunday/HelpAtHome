using HelpAtHome.Application.Interfaces.Services;

namespace HelpAtHome.Tests.Infrastructure.Fakes;

public class FakeSmsSender : ISmsSender
{
    public Task SendAsync(string toPhone, string message) => Task.CompletedTask;
}
