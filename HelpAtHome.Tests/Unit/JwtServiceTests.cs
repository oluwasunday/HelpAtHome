using FluentAssertions;
using HelpAtHome.Application.Services;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace HelpAtHome.Tests.Unit;

public class JwtServiceTests
{
    private static JwtService CreateService(
        string key = "unit-test-jwt-key-minimum-32-characters-long!",
        string issuer = "TestIssuer",
        string audience = "TestAudience",
        string expiryMinutes = "60")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = issuer,
                ["Jwt:Audience"] = audience,
                ["Jwt:ExpiryMinutes"] = expiryMinutes,
            })
            .Build();

        return new JwtService(config);
    }

    private static User MakeUser(UserRole role = UserRole.Client) => new()
    {
        Id = Guid.NewGuid(),
        Email = "unit@test.com",
        UserName = "unit@test.com",
        FirstName = "Unit",
        LastName = "Test",
        Role = role,
        SecurityStamp = Guid.NewGuid().ToString(),
    };

    // ── GenerateAccessToken ────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsNonEmptyToken()
    {
        var svc = CreateService();
        var user = MakeUser();

        var token = svc.GenerateAccessToken(user, new[] { "Client" }, Array.Empty<Claim>());

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_ContainsSubClaim()
    {
        var svc = CreateService();
        var user = MakeUser();

        var token = svc.GenerateAccessToken(user, new[] { "Client" }, Array.Empty<Claim>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Subject.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ContainsRoleClaim()
    {
        var svc = CreateService();
        var user = MakeUser(UserRole.IndividualCaregiver);

        var token = svc.GenerateAccessToken(user, new[] { "IndividualCaregiver" }, Array.Empty<Claim>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "IndividualCaregiver");
    }

    [Fact]
    public void GenerateAccessToken_ExpiryRespectsConfig()
    {
        var svc = CreateService(expiryMinutes: "120");
        var user = MakeUser();

        var before = DateTime.UtcNow;
        var token = svc.GenerateAccessToken(user, Array.Empty<string>(), Array.Empty<Claim>());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(120), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_IssuerAndAudienceAreSet()
    {
        var svc = CreateService(issuer: "MyIssuer", audience: "MyAudience");
        var user = MakeUser();

        var token = svc.GenerateAccessToken(user, Array.Empty<string>(), Array.Empty<Claim>());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("MyIssuer");
        jwt.Audiences.Should().Contain("MyAudience");
    }

    // ── GenerateRefreshToken ───────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_IsBase64String()
    {
        var svc = CreateService();
        var token = svc.GenerateRefreshToken();

        var isBase64 = token.Length > 0 &&
            System.Convert.TryFromBase64String(token, new byte[token.Length], out _);
        isBase64.Should().BeTrue();
    }

    [Fact]
    public void GenerateRefreshToken_IsDifferentEachCall()
    {
        var svc = CreateService();
        var t1 = svc.GenerateRefreshToken();
        var t2 = svc.GenerateRefreshToken();

        t1.Should().NotBe(t2);
    }

    [Fact]
    public void GenerateRefreshToken_LengthIsAdequateForSecurity()
    {
        var svc = CreateService();
        // 64 random bytes → base64 → ~88 chars
        var token = svc.GenerateRefreshToken();
        token.Length.Should().BeGreaterThanOrEqualTo(80);
    }

    // ── GetPrincipalFromExpiredToken ──────────────────────────────────────

    [Fact]
    public void GetPrincipalFromExpiredToken_ValidJwtStructure_ReturnsPrincipalIgnoringLifetime()
    {
        // The method skips lifetime validation, so it works on both live and expired tokens.
        // We generate a very short-lived token and confirm claims are readable.
        var svc = CreateService(expiryMinutes: "1");
        var user = MakeUser();

        var token = svc.GenerateAccessToken(user, new[] { "Client" }, Array.Empty<Claim>());
        var principal = svc.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            .Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_GarbageToken_ReturnsNull()
    {
        var svc = CreateService();
        var principal = svc.GetPrincipalFromExpiredToken("not.a.jwt.token");
        principal.Should().BeNull();
    }
}
