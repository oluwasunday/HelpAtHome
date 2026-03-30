using FluentAssertions;
using HelpAtHome.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HelpAtHome.Tests.Integration.Auth;

public class LoginTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public LoginTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private static object LoginPayload(string email, string password = TestWebApplicationFactory.TestPassword) =>
        new { email, password };

    // ── tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            LoginPayload(TestWebApplicationFactory.ClientEmail));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()
            .Should().NotBeNullOrEmpty();
        doc.RootElement.GetProperty("data").GetProperty("refreshToken").GetString()
            .Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            LoginPayload(TestWebApplicationFactory.ClientEmail, "WrongPassword1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonexistentEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            LoginPayload("ghost@nowhere.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_SuccessfulLogin_ResponseContainsUserRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            LoginPayload(TestWebApplicationFactory.ClientEmail));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("data").GetProperty("role").GetInt32()
            .Should().Be(3); // UserRole.Client = 3
    }
}
