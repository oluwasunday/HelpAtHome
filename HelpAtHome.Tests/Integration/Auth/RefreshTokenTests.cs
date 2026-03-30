using FluentAssertions;
using HelpAtHome.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HelpAtHome.Tests.Integration.Auth;

public class RefreshTokenTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public RefreshTokenTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    /// <summary>Login as the pre-seeded client and return the token pair.</summary>
    private async Task<(string AccessToken, string RefreshToken)> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email    = TestWebApplicationFactory.ClientEmail,
            password = TestWebApplicationFactory.TestPassword,
        });

        response.EnsureSuccessStatusCode();
        var doc  = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");
        return (
            data.GetProperty("accessToken").GetString()!,
            data.GetProperty("refreshToken").GetString()!
        );
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokenPair()
    {
        var (_, refreshToken) = await LoginAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token",
            new { refreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()
            .Should().NotBeNullOrEmpty();
        doc.RootElement.GetProperty("data").GetProperty("refreshToken").GetString()
            .Should().NotBe(refreshToken, "token should be rotated on each refresh");
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token",
            new { refreshToken = "this-is-not-a-real-refresh-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_UsedTwice_SecondCallFails()
    {
        var (_, refreshToken) = await LoginAsync();

        var first = await _client.PostAsJsonAsync("/api/auth/refresh-token",
            new { refreshToken });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/auth/refresh-token",
            new { refreshToken });
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ValidToken_Returns200()
    {
        var (accessToken, refreshToken) = await LoginAsync();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsJsonAsync("/api/auth/logout", refreshToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
