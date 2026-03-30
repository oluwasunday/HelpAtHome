using HelpAtHome.Application.Interfaces.Services;

namespace HelpAtHome.Tests.Infrastructure.Fakes;

public class FakeFirebasePush : IFirebasePush
{
    public Task SendAsync(string fcmToken, string title, string body, object? data = null)
        => Task.CompletedTask;
}
