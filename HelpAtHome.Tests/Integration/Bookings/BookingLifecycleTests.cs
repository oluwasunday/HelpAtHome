using FluentAssertions;
using HelpAtHome.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HelpAtHome.Tests.Integration.Bookings;

/// <summary>
/// Tests the full booking lifecycle: Create → Accept → Start → Complete.
/// Uses pre-seeded client and caregiver (from TestWebApplicationFactory.InitializeAsync).
/// </summary>
public class BookingLifecycleTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public BookingLifecycleTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── Login helper ───────────────────────────────────────────────────────

    private async Task<(HttpClient Client, string AccessToken)> LoginAsync(string email)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = TestWebApplicationFactory.TestPassword,
        });

        response.EnsureSuccessStatusCode();
        var doc   = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return (client, token);
    }

    private static Guid GetBookingId(string json)
    {
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    // ── tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_InsufficientBalance_Returns400()
    {
        // Register a fresh client — wallet starts at zero
        var regClient = _factory.CreateClient();
        var zeroEmail = $"zero_{Guid.NewGuid():N}@test.com";
        var regResp = await regClient.PostAsJsonAsync("/api/auth/register/client", new
        {
            firstName       = "Zero",
            lastName        = "Balance",
            email           = zeroEmail,
            phoneNumber     = "+2348700000098",
            password        = "Test@1234",
            confirmPassword = "Test@1234",
            gender          = 2,
            dateOfBirth     = "1985-07-20",
            address         = new
            {
                line1    = "10 Zero Ave",
                locality = "Surulere",
                lga      = "Surulere",
                state    = "Lagos",
                country  = "Nigeria",
            },
        });
        regResp.EnsureSuccessStatusCode();

        var doc      = JsonDocument.Parse(await regResp.Content.ReadAsStringAsync());
        var token    = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
        var zeroHttp = _factory.CreateClient();
        zeroHttp.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await zeroHttp.PostAsJsonAsync("/api/bookings", new
        {
            caregiverProfileId = _factory.CaregiverProfileId,
            serviceCategoryId  = _factory.ServiceCategoryId,
            frequency          = 1,
            scheduledStartDate = DateTime.UtcNow.AddDays(3).ToString("o"),
            scheduledEndDate   = DateTime.UtcNow.AddDays(4).ToString("o"),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBooking_SufficientBalance_Returns200WithPendingStatus()
    {
        var (clientHttp, _) = await LoginAsync(TestWebApplicationFactory.ClientEmail);

        var dto = new
        {
            caregiverProfileId = _factory.CaregiverProfileId,
            serviceCategoryId  = _factory.ServiceCategoryId,
            frequency          = 1,
            scheduledStartDate = DateTime.UtcNow.AddDays(3).ToString("o"),
            scheduledEndDate   = DateTime.UtcNow.AddDays(4).ToString("o"),
        };

        var response = await clientHttp.PostAsJsonAsync("/api/bookings", dto);
        var body     = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("data").GetProperty("status").GetInt32()
            .Should().Be(0); // BookingStatus.Pending = 0
    }

    [Fact]
    public async Task FullLifecycle_CreateAcceptStartComplete_AllSucceed()
    {
        var (clientHttp, _)   = await LoginAsync(TestWebApplicationFactory.ClientEmail);
        var (cgHttp, _)       = await LoginAsync(TestWebApplicationFactory.CaregiverEmail);

        // 1. Create
        var createResp = await clientHttp.PostAsJsonAsync("/api/bookings", new
        {
            caregiverProfileId = _factory.CaregiverProfileId,
            serviceCategoryId  = _factory.ServiceCategoryId,
            frequency          = 1,
            scheduledStartDate = DateTime.UtcNow.AddDays(5).ToString("o"),
            scheduledEndDate   = DateTime.UtcNow.AddDays(6).ToString("o"),
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var bookingId = GetBookingId(await createResp.Content.ReadAsStringAsync());

        // 2. Accept
        var accept = await cgHttp.PatchAsync($"/api/bookings/{bookingId}/accept", null);
        accept.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Start
        var start = await cgHttp.PatchAsync($"/api/bookings/{bookingId}/start", null);
        start.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Complete
        var complete = await cgHttp.PatchAsync($"/api/bookings/{bookingId}/complete", null);
        complete.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await complete.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("data").GetProperty("status").GetInt32()
            .Should().Be(3); // BookingStatus.Completed = 3
    }

    [Fact]
    public async Task CancelBooking_ByClient_Returns200()
    {
        var (clientHttp, _) = await LoginAsync(TestWebApplicationFactory.ClientEmail);

        var createResp = await clientHttp.PostAsJsonAsync("/api/bookings", new
        {
            caregiverProfileId = _factory.CaregiverProfileId,
            serviceCategoryId  = _factory.ServiceCategoryId,
            frequency          = 1,
            scheduledStartDate = DateTime.UtcNow.AddDays(7).ToString("o"),
            scheduledEndDate   = DateTime.UtcNow.AddDays(8).ToString("o"),
        });
        createResp.EnsureSuccessStatusCode();
        var bookingId = GetBookingId(await createResp.Content.ReadAsStringAsync());

        var cancel = await clientHttp.PatchAsync(
            $"/api/bookings/{bookingId}/cancel?reason=Changed+my+mind", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBooking_UnauthenticatedUser_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/bookings/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBooking_NonexistentId_Returns404OrBadRequest()
    {
        var (clientHttp, _) = await LoginAsync(TestWebApplicationFactory.ClientEmail);
        var response = await clientHttp.GetAsync($"/api/bookings/{Guid.NewGuid()}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
