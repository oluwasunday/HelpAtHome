using FluentValidation;
using FluentValidation.AspNetCore;
using HelpAtHome.Api.Extensions;
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
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using sib_api_v3_sdk.Client;
using System.Security.Claims;
using System.Text;

namespace HelpAtHome.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddServices(config);

            // ??? MySQL (EF Core + IdentityDbContext) ????????????????????????????
            /*builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ));*/

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(config.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection")),
                    b => b.MigrationsAssembly("HelpAtHome.Infrastructure")));


            // ??? ASP.NET Core Identity ??????????????????????????????????????????
            // Registers: UserManager<User>, SignInManager<User>, RoleManager,
            // IPasswordHasher<User>, token providers, lockout policy.
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
                options.SignIn.RequireConfirmedEmail = false; // true in production
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ??? JWT Bearer (override Identity cookie default for API use) ???????
            var jwtSettings = config.GetSection("Jwt");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            // ??? Role-based Authorization Policies ??????????????????????????????
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("SuperAdmin", "Admin"));
                options.AddPolicy("CaregiverOnly", p => p.RequireRole("IndividualCaregiver", "AgencyCaregiver"));
                options.AddPolicy("AgencyAdminOnly", p => p.RequireRole("AgencyAdmin"));
                options.AddPolicy("ClientOnly", p => p.RequireRole("Client"));
                options.AddPolicy("FamilyOrClient", p => p.RequireRole("Client", "FamilyMember"));
            });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // brevo email config
            //Configuration.Default.ApiKey.Add("api-key", builder.Configuration["Email:ApiKey"]);
            builder.Services.AddHttpClient("Brevo", client =>
            {
                client.BaseAddress = new Uri("https://api.brevo.com/v3/");
                client.DefaultRequestHeaders.Add("api-key", builder.Configuration["Email:ApiKey"]);
            });




            // ??? Background Jobs (Hangfire) ??????????????????????????????????????
            /*services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseStorage(new MySqlStorage(connStr, new MySqlStorageOptions())));
            services.AddHangfireServer();*/

            // ??? Other ???????????????????????????????????????????????????????????
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterClientDtoValidator>();
            //services.AddControllers().AddNewtonsoftJson();
            //services.AddEndpointsApiExplorer();
            //services.AddSwaggerGen(/* JWT bearer config */);
            builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", p =>
                p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var mongo = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
                var mongoSettings = scope.ServiceProvider
                    .GetRequiredService<IOptions<MongoDbSettings>>().Value;

                await db.Database.MigrateAsync();
                //await MongoIndexInitializer.InitializeAsync(mongo, mongoSettings);
                await DataSeeder.SeedAllAsync(db, userMgr, roleMgr, mongo, mongoSettings);
            }


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
