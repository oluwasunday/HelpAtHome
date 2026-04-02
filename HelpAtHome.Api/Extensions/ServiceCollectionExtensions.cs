using FluentValidation;
using FluentValidation.AspNetCore;
using HelpAtHome.Application;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Application.Services;
using HelpAtHome.Application.Validators;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using HelpAtHome.Infrastructure.MongoDB;
using HelpAtHome.Infrastructure.Repositories;
using HelpAtHome.Infrastructure.Audit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace HelpAtHome.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // ─── MongoDB ────────────────────────────────────────────────────────
            services.Configure<MongoDbSettings>(config.GetSection("MongoDb"));
            services.AddSingleton<MongoDbContext>();

            // ─── Cache ──────────────────────────────────────────────────────────
            // Use Redis when a connection string is configured (staging/production).
            // Fall back to in-process memory cache in development so the app runs
            // without a local Redis instance.
            var redisConnectionString = config.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName  = "HelpAtHome:";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            // ─── Application Services ───────────────────────────────────────────
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUserService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IPaymentGatewayService, PaystackService>();

            // ─── Paystack HTTP client ────────────────────────────────────────────
            services.AddHttpClient("Paystack", client =>
            {
                var baseUrl = (config["Paystack:BaseUrl"] ?? "https://api.paystack.co").TrimEnd('/') + "/";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config["Paystack:SecretKey"]}");
            });
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ICaregiverService, CaregiverProfileService>();
            services.AddScoped<IAgencyService, AgencyService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISupportService, SupportService>();
            services.AddScoped<IEmergencyService, EmergencyService>();
            services.AddScoped<IFamilyAccessService, FamilyAccessService>();
            services.AddScoped<IBadgeService, BadgeService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            //services.AddScoped<IVerificationService, VerificationService>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IFirebasePush, FirebasePushSender>();
            services.AddScoped<ISmsSender, TwilioSmsSender>();
            services.AddScoped<IEmailSender, MailKitEmailSender>();

            // ─── Repositories ───────────────────────────────────────────────────
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICaregiverProfileRepository, CaregiverProfileRepository>();
            services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ICaregiverServiceRepository, CaregiverServiceRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();

            //configure email settings
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            services.AddScoped<ITicketMessageRepository, TicketMessageRepository>();
            services.AddScoped<IEmergencyAlertRepository, EmergencyAlertRepository>();
            services.AddScoped<IFamilyAccessRepository, FamilyAccessRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
            services.AddScoped<IVerificationDocumentRepository, VerificationDocumentRepository>();
            services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            services.AddScoped<IClientAddressRepository, ClientAddressRepository>();
            services.AddScoped<IAgencyAddressRepository, AgencyAddressRepository>();
            services.AddScoped<ICaregiverAddressRepository, CaregiverAddressRepository>();

            return services;
        }
    }
}
