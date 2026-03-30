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
            // ... service registrations ...
            services.Configure<MongoDbSettings>(config.GetSection("MongoDb"));
            services.AddSingleton<MongoDbContext>();


            // ─── MongoDB ────────────────────────────────────────────────────────
            services.Configure<MongoDbSettings>(config.GetSection("MongoDb"));
            services.AddSingleton<MongoDbContext>();

            // ─── Redis ──────────────────────────────────────────────────────────
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config.GetConnectionString("Redis");
                options.InstanceName  = "HelpAtHome:";
            });

            // ─── Application Services ───────────────────────────────────────────
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<INotificationService, NotificationService>();
            //services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ICaregiverService, CaregiverProfileService>();
            //services.AddScoped<IAgencyService, AgencyService>();
            //services.AddScoped<ISupportService, SupportService>();
            //services.AddScoped<IEmergencyService, EmergencyService>();
            //services.AddScoped<IFamilyAccessService, FamilyAccessService>();
            //services.AddScoped<IBadgeService, BadgeService>();
            //services.AddScoped<IVerificationService, VerificationService>();
            //services.AddScoped<IPaymentGatewayService, PaystackService>();
            //services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IFirebasePush, FirebasePushSender>();
            services.AddScoped<IJwtService, JwtService>();
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
