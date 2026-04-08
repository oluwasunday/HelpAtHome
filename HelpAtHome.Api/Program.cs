using FluentValidation;
using FluentValidation.AspNetCore;
using HelpAtHome.Api.Configuration;
using HelpAtHome.Api.Extensions;
using HelpAtHome.Api.Middleware;
using HelpAtHome.Application;
using HelpAtHome.Application.Validators;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using HelpAtHome.Infrastructure.Data.DataSeeder;
using HelpAtHome.Infrastructure.MongoDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;

// ── Step 1: Load .env file into process environment variables ───────────────
// Must happen before CreateBuilder so the .NET config system picks them up.
// System/OS env vars are never overwritten (production always wins).
EnvironmentLoader.Load();

// ── Step 2: Bootstrap logger — captures startup errors ──────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var config = builder.Configuration;

    // ── Step 3: Validate all required config values are present ─────────────
    // Fails immediately with a clear list of every missing key if anything
    // is absent — prevents cryptic runtime errors deep in the app.
    ConfigurationValidator.Validate(config);

    // ── Serilog — replaces the default Microsoft logging pipeline ──────────
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ── Application services ────────────────────────────────────────────────
    builder.Services.AddServices(config);

    // ── MySQL (EF Core + IdentityDbContext) ─────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(config.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection")),
            b => b.MigrationsAssembly("HelpAtHome.Infrastructure")
        )
    );

    // ── ASP.NET Core Identity ───────────────────────────────────────────────
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false; // set true in production
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // ── JWT Bearer ──────────────────────────────────────────────────────────
    var jwtSettings = config.GetSection("Jwt");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
            RoleClaimType            = ClaimTypes.Role,
            NameClaimType            = ClaimTypes.NameIdentifier
        };
    });

    // ── Authorization policies ──────────────────────────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly",      p => p.RequireRole("SuperAdmin", "Admin"));
        options.AddPolicy("CaregiverOnly",  p => p.RequireRole("IndividualCaregiver", "AgencyCaregiver"));
        options.AddPolicy("AgencyAdminOnly",p => p.RequireRole("AgencyAdmin"));
        options.AddPolicy("ClientOnly",     p => p.RequireRole("Client"));
        options.AddPolicy("FamilyOrClient", p => p.RequireRole("Client", "FamilyMember"));
    });

    // ── Global exception handling (RFC 7807 ProblemDetails) ─────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── MVC, Swagger, AutoMapper, Validation, CORS ──────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerConfiguration();
    builder.Services.AddAutoMapper(typeof(MappingProfile));
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterClientDtoValidator>();
    var allowedOrigins = (config["App:AllowedOrigins"] ?? config["App:ClientBaseUrl"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", p =>
    {
        if (builder.Environment.IsDevelopment())
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    }));

    // ── Brevo email HTTP client ─────────────────────────────────────────────
    builder.Services.AddHttpClient("Brevo", client =>
    {
        client.BaseAddress = new Uri("https://api.brevo.com/v3/");
        client.DefaultRequestHeaders.Add("api-key", config["Email:ApiKey"]);
    });

    // ───────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ───────────────────────────────────────────────────────────────────────

    // ── Middleware pipeline (order matters) ─────────────────────────────────
    app.UseExceptionHandler();          // must be first — catches everything below

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerConfiguration();
    }

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseHttpsRedirection();
    app.UseCors("CorsPolicy");
    app.UseAuthentication();            // was missing — must come before UseAuthorization
    app.UseAuthorization();
    app.MapControllers();

    // ── Database migration + seeding ────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db         = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userMgr    = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleMgr    = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var mongo      = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var mongoSettings = scope.ServiceProvider
                               .GetRequiredService<IOptions<MongoDbSettings>>().Value;

        await db.Database.MigrateAsync();
        await DataSeeder.SeedAllAsync(db, userMgr, roleMgr, mongo, mongoSettings);
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}
