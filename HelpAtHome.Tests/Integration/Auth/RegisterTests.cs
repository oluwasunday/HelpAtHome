using FluentAssertions;
using HelpAtHome.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace HelpAtHome.Tests.Integration.Auth;

public class RegisterTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public RegisterTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static object ClientPayload(string email) => new
    {
        firstName       = "Jane",
        lastName        = "Doe",
        email,
        phoneNumber     = "+2348123456789",
        password        = "Test@1234",
        confirmPassword = "Test@1234",
        gender          = 2,
        dateOfBirth     = "1990-01-01",
        address         = new
        {
            line1    = "12 Test Street",
            locality = "Victoria Island",
            lga      = "Eti-Osa",
            state    = "Lagos",
            country  = "Nigeria",
        },
    };

    private static object CaregiverPayload(string email) => new
    {
        firstName            = "John",
        lastName             = "Care",
        email,
        phoneNumber          = "+2348987654321",
        password             = "Test@1234",
        confirmPassword      = "Test@1234",
        gender               = 1,
        hourlyRate           = 2000,
        dailyRate            = 15000,
        monthlyRate          = 200000,
        yearsOfExperience    = 3,
        idType               = 1,
        idNumber             = "AB1234567",
        nextOfKinName        = "Mary Care",
        nextOfKinPhoneNumber = "+2348111111111",
        address              = new
        {
            line1    = "5 Caregiver Lane",
            locality = "Lekki",
            lga      = "Eti-Osa",
            state    = "Lagos",
            country  = "Nigeria",
        },
    };

    // ── tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterClient_ValidPayload_Returns200WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register/client",
            ClientPayload("newclient@test.com"));

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    [Fact]
    public async Task RegisterClient_DuplicateEmail_Returns400()
    {
        const string email = "dup_register@test.com";

        await _client.PostAsJsonAsync("/api/auth/register/client", ClientPayload(email));
        var second = await _client.PostAsJsonAsync("/api/auth/register/client", ClientPayload(email));

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterClient_PasswordMismatch_Returns400()
    {
        var payload = new
        {
            firstName       = "Jane",
            lastName        = "Doe",
            email           = "mismatch@test.com",
            phoneNumber     = "+2348000000001",
            password        = "Test@1234",
            confirmPassword = "WrongPass1",
            gender          = 2,
            dateOfBirth     = "1990-01-01",
            address         = new
            {
                line1    = "1 Road",
                locality = "Island",
                lga      = "Eti-Osa",
                state    = "Lagos",
                country  = "Nigeria",
            },
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/client", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterCaregiver_ValidPayload_Returns200WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register/caregiver",
            CaregiverPayload("newcaregiver@test.com"));

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    [Fact]
    public async Task RegisterClient_MissingAddress_Returns400()
    {
        var payload = new
        {
            firstName       = "Jane",
            lastName        = "Doe",
            email           = "noaddress@test.com",
            phoneNumber     = "+2348000000002",
            password        = "Test@1234",
            confirmPassword = "Test@1234",
            gender          = 2,
            dateOfBirth     = "1990-01-01",
            // address omitted — Line1 defaults to "" which fails [Required]
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register/client", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
