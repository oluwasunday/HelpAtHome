using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using HelpAtHome.Tests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HelpAtHome.Tests.Infrastructure;

/// <summary>
/// Spins up the full API in an in-process test server.
/// - Each instance gets a unique InMemory database — no cross-class interference.
/// - Roles + two test users (client + caregiver) are pre-seeded in InitializeAsync.
/// - External senders (email/SMS/push) are replaced with no-op fakes.
/// - Redis is replaced with in-memory IDistributedCache.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ── Unique DB per factory instance ──────────────────────────────────────
    private readonly string _dbName = $"HelpAtHomeTest_{Guid.NewGuid():N}";

    // ── Pre-seeded user credentials (safe to reference from any test) ───────
    public const string TestPassword  = "Test@1234";
    public const string ClientEmail   = "seeded.client@test.internal";
    public const string CaregiverEmail = "seeded.caregiver@test.internal";

    // Populated in InitializeAsync
    public Guid ClientUserId       { get; private set; }
    public Guid CaregiverUserId    { get; private set; }
    public Guid CaregiverProfileId { get; private set; }
    public Guid ServiceCategoryId  { get; private set; }

    static TestWebApplicationFactory()
    {
        // Pre-set env vars required by ConfigurationValidator before any factory
        // instance builds its host. Double-underscore maps to "Section:Key".
        var cfg = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"]               = "Testing",
            ["ConnectionStrings__DefaultConnection"] = "Server=fake;Database=fake;",
            ["MongoDb__ConnectionString"]            = "mongodb://localhost:27017",
            ["MongoDb__DatabaseName"]                = "HelpAtHomeTest",
            ["Jwt__Key"]                             = "test-jwt-signing-key-must-be-at-least-32-chars!",
            ["Jwt__Issuer"]                          = "HelpAtHome.Test",
            ["Jwt__Audience"]                        = "HelpAtHome.Test",
            ["Jwt__ExpiryMinutes"]                   = "60",
            ["Email__SenderEmail"]                   = "test@example.com",
            ["Email__ApiKey"]                        = "fake-email-api-key",
            ["EmailSettings__Mail"]                  = "test@example.com",
            ["EmailSettings__Login"]                 = "testlogin",
            ["EmailSettings__Password"]              = "testpassword",
            ["EmailSettings__ApiKey"]                = "fake-smtp-api-key",
            ["Paystack__SecretKey"]                  = "sk_test_fake",
            ["Paystack__PublicKey"]                  = "pk_test_fake",
            ["Paystack__WebhookSecret"]              = "webhook_fake",
            ["Cloudinary__CloudName"]                = "fakecloud",
            ["Cloudinary__ApiKey"]                   = "fakeapikey",
            ["Cloudinary__ApiSecret"]                = "fakeapisecret",
            ["Platform__CommissionRate"]             = "15",
            ["Platform__BookingReferencePrefix"]     = "HAH",
            ["App__ClientBaseUrl"]                   = "http://localhost:3000",
        };
        foreach (var (k, v) in cfg)
            Environment.SetEnvironmentVariable(k, v);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Unique InMemory DB — isolated from all other test classes
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            // Redis → in-memory
            RemoveAll<IDistributedCache>(services);
            services.AddDistributedMemoryCache();

            // External senders → no-op fakes
            RemoveAll<IEmailSender>(services);
            RemoveAll<ISmsSender>(services);
            RemoveAll<IFirebasePush>(services);
            services.AddScoped<IEmailSender, FakeEmailSender>();
            services.AddScoped<ISmsSender, FakeSmsSender>();
            services.AddScoped<IFirebasePush, FakeFirebasePush>();
        });
    }

    // ── IAsyncLifetime ───────────────────────────────────────────────────────

    /// <summary>
    /// Seeds the database once before any test in the class runs.
    /// Creates roles, a funded client, and a caregiver with profile.
    /// </summary>
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var sp = scope.ServiceProvider;

        var db      = sp.GetRequiredService<AppDbContext>();
        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userMgr = sp.GetRequiredService<UserManager<User>>();

        await db.Database.EnsureCreatedAsync();

        // ── Roles ──────────────────────────────────────────────────────────
        foreach (var name in Enum.GetNames<UserRole>())
            if (!await roleMgr.RoleExistsAsync(name))
                await roleMgr.CreateAsync(new IdentityRole<Guid>(name));

        // ── Seeded client ──────────────────────────────────────────────────
        var clientId = Guid.NewGuid();
        ClientUserId = clientId;

        var clientUser = new User
        {
            Id            = clientId,
            UserName      = ClientEmail,
            Email         = ClientEmail,
            FirstName     = "Seed",
            LastName      = "Client",
            PhoneNumber   = "+2348100000001",
            Role          = UserRole.Client,
            IsActive      = true,
            LockoutEnabled = true,
        };
        await userMgr.CreateAsync(clientUser, TestPassword);
        await userMgr.AddToRoleAsync(clientUser, nameof(UserRole.Client));

        var clientProfileId = Guid.NewGuid();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id          = clientProfileId,
            UserId      = clientId,
            DateOfBirth = new DateTime(1985, 1, 1),
            Gender      = Gender.Male,
            Address     = new ClientAddress
            {
                Id              = Guid.NewGuid(),
                ClientProfileId = clientProfileId,
                Line1           = "1 Seed Street",
                Locality        = "Victoria Island",
                LGA             = "Eti-Osa",
                State           = "Lagos",
                Country         = "Nigeria",
            },
        });
        db.Wallets.Add(new Wallet
        {
            Id      = Guid.NewGuid(),
            UserId  = clientId,
            Balance = 500_000m,
        });

        // ── Seeded caregiver ───────────────────────────────────────────────
        var caregiverId = Guid.NewGuid();
        CaregiverUserId = caregiverId;

        var cgUser = new User
        {
            Id             = caregiverId,
            UserName       = CaregiverEmail,
            Email          = CaregiverEmail,
            FirstName      = "Seed",
            LastName       = "Caregiver",
            PhoneNumber    = "+2348100000002",
            Role           = UserRole.IndividualCaregiver,
            IsActive       = true,
            LockoutEnabled = true,
        };
        await userMgr.CreateAsync(cgUser, TestPassword);
        await userMgr.AddToRoleAsync(cgUser, nameof(UserRole.IndividualCaregiver));

        var cgProfileId = Guid.NewGuid();
        CaregiverProfileId = cgProfileId;

        db.CaregiverProfiles.Add(new CaregiverProfile
        {
            Id                  = cgProfileId,
            UserId              = caregiverId,
            HourlyRate          = 2500m,
            DailyRate           = 18_000m,
            MonthlyRate         = 250_000m,
            YearsOfExperience   = 5,
            IdType              = DocumentType.NationalId,
            IdNumber            = "12345678901",
            NextOfKinName       = "Next Kin",
            NextOfKinPhoneNumber = "+2348222222222",
            IsAvailable         = true,
            GenderProvided      = Gender.Male,
            VerificationStatus  = VerificationStatus.Pending,
            User                = cgUser,
            Address             = new CaregiverAddress
            {
                Id                = Guid.NewGuid(),
                CaregiverProfileId = cgProfileId,
                Line1             = "20 Caregiver Close",
                Locality          = "Gbagada",
                LGA               = "Kosofe",
                State             = "Lagos",
                Country           = "Nigeria",
            },
        });
        db.Wallets.Add(new Wallet
        {
            Id     = Guid.NewGuid(),
            UserId = caregiverId,
            Balance = 0m,
        });

        // ── Service category ───────────────────────────────────────────────
        var catId = Guid.NewGuid();
        ServiceCategoryId = catId;
        db.ServiceCategories.Add(new ServiceCategory
        {
            Id          = catId,
            Name        = "Home Care",
            Description = "Home Care",
        });

        await db.SaveChangesAsync();
    }

    public new Task DisposeAsync() => Task.CompletedTask;

    // ── Helpers used by individual tests ────────────────────────────────────

    /// <summary>No-op — roles are now seeded in InitializeAsync.</summary>
    public Task SeedRolesAsync() => Task.CompletedTask;

    /// <summary>Seed an extra ServiceCategory and return its Id.</summary>
    public async Task<Guid> SeedServiceCategoryAsync(string name = "Home Care")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existing = db.ServiceCategories.FirstOrDefault(s => s.Name == name);
        if (existing != null) return existing.Id;
        var cat = new ServiceCategory { Id = Guid.NewGuid(), Name = name, Description = name };
        db.ServiceCategories.Add(cat);
        await db.SaveChangesAsync();
        return cat.Id;
    }

    /// <summary>Add funds to a user wallet directly (bypasses Paystack).</summary>
    public async Task FundWalletAsync(Guid userId, decimal amount)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = db.Wallets.FirstOrDefault(w => w.UserId == userId)
            ?? throw new InvalidOperationException($"No wallet for user {userId}");
        wallet.Balance += amount;
        db.Wallets.Update(wallet);
        await db.SaveChangesAsync();
    }

    /// <summary>Return the CaregiverProfile.Id for the given user.</summary>
    public async Task<Guid> GetCaregiverProfileIdAsync(Guid userId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var profile = db.CaregiverProfiles.FirstOrDefault(c => c.UserId == userId)
            ?? throw new InvalidOperationException($"No caregiver profile for user {userId}");
        return await Task.FromResult(profile.Id);
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in descriptors) services.Remove(d);
    }
}
