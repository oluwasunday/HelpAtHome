# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build entire solution
dotnet build

# Run the API (from repo root)
dotnet run --project HelpAtHome.Api

# Add a new EF Core migration (migrations live in HelpAtHome.Infrastructure)
dotnet ef migrations add <MigrationName> --project HelpAtHome.Infrastructure --startup-project HelpAtHome.Api

# Apply migrations manually (also runs automatically on startup)
dotnet ef database update --project HelpAtHome.Infrastructure --startup-project HelpAtHome.Api

# There are no tests yet — no test project exists in the solution
```

Swagger UI is available at `https://localhost:<port>/swagger` in Development mode only.

## Architecture

Five projects in a strict dependency chain:

```
HelpAtHome.Api → HelpAtHome.Application → HelpAtHome.Infrastructure → HelpAtHome.Core
                                                                    ↗
                                               HelpAtHome.Shared ──
```

- **Core** — Domain entities, DTOs (Requests/Responses), Enums, MongoDB documents. No dependencies.
- **Shared** — `Result<T>` / `Result` wrapper used as the universal service return type. No dependencies.
- **Infrastructure** — EF Core `AppDbContext` (MySQL via Pomelo), all repository implementations, `UnitOfWork`, MongoDB context, migrations, data seeder, external service senders (email/SMS/push).
- **Application** — All service interfaces + implementations, repository interfaces, `IUnitOfWork`, `MappingProfile` (AutoMapper), FluentValidation validators.
- **Api** — Controllers, `Program.cs`, `ServiceCollectionExtensions` (DI wiring), middleware.

## Key Patterns

### Result<T>
All service methods return `Result` (non-generic) or `Result<T>`. Never throw exceptions from services — always return `Result.Fail("message")`. Controllers check `result.IsSuccess` and return the appropriate HTTP status.

### Unit of Work
All data access goes through `IUnitOfWork` (injected as `_uow`). Never inject repositories directly into services. Call `_uow.SaveChangesAsync()` to persist. For multi-step operations use `_uow.BeginTransactionAsync()` / `CommitAsync()` / `RollbackAsync()`.

### Repository pattern
`IGenericRepository<T>` provides: `GetByIdAsync`, `GetAllAsync`, `FindAsync` (raw predicate), `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, `AddAsync`, `Update`, `SoftDelete`. Specialized repos extend this interface. `FindAsync` and `FirstOrDefaultAsync` bypass global query filters — be aware when querying soft-deletable entities.

### Soft delete
`BaseEntity` has `IsDeleted`, `DeletedAt`, `DeletedBy`. Global query filters in `AppDbContext` apply `!IsDeleted` for `User`, `Booking`, and `CaregiverProfile`. `User` has its own `IsDeleted` field (not via `BaseEntity`) — same pattern, separate implementation.

### JWT claims
The `sub` claim holds the user's `Guid` ID. Controllers extract it as:
```csharp
var userId = Guid.Parse(User.FindFirst("sub")!.Value);
```
The token also carries `firstName`, `lastName`, `role`, `securityStamp`, and `ClaimTypes.Role` (for `[Authorize(Roles="...")]`).

### Authorization policies
Defined in `Program.cs`:
- `"AdminOnly"` — SuperAdmin, Admin
- `"CaregiverOnly"` — IndividualCaregiver, AgencyCaregiver
- `"AgencyAdminOnly"` — AgencyAdmin
- `"ClientOnly"` — Client
- `"FamilyOrClient"` — Client, FamilyMember

### OTP codes
`OtpCode` entity has `Purpose` (string: `"EmailVerify"`, `"PhoneVerify"`, `"PasswordReset"`, `"Login2FA"`), `ExpiresAt` (10-minute window), `IsUsed`, and `Attempts` (burned after 5 failures). Always invalidate existing active OTPs before issuing a new one.

### Refresh token rotation
On every `RefreshTokenAsync` call: mark old token `IsRevoked = true` with `RevokedReason = "Rotated"`, then issue a new pair. `LogoutAsync` sets `RevokedReason = "Logout"` and is idempotent.

## What Is and Isn't Implemented

**Fully implemented:** All four registration flows, login, forgot/reset password, change password, refresh token, logout, email OTP send/verify, phone OTP send/verify.

**Stub (throws `NotImplementedException`):**
- `BookingService` — all methods except `CreateBookingAsync` (partial)
- `WalletService` — all methods except `VerifyDepositAsync` (partial)

**Commented out / not yet registered in `ServiceCollectionExtensions`:**
- `ICaregiverService`, `IAgencyService`, `IAdminService`, `IReviewService`, `ISupportService`, `IEmergencyService`, `IFamilyAccessService`, `IBadgeService`, `IVerificationService`, `IPaymentGatewayService`, `IAuditLogService`
- Redis caching
- Hangfire background jobs
- Firebase push (`IFirebasePush` is registered but `SendPushAsync` body is commented out)
- SMS sending (`ISmsSender` is registered but `SendSmsAsync` body is commented out)

## Database

- **MySQL** via Pomelo EF Core. Connection string key: `ConnectionStrings:DefaultConnection`.
- **MongoDB** for audit logs, agency activity logs, notification logs, search logs. Settings under `MongoDb` config section. `MongoDbContext` is registered as `Singleton`.
- Migrations are in `HelpAtHome.Infrastructure/Migrations/`. `db.Database.MigrateAsync()` runs on every startup.
- `AppDbContext.SaveChangesAsync()` auto-sets `UpdatedAt` on all modified `BaseEntity` and `User` instances.

## Configuration Sections

| Section | Purpose |
|---|---|
| `Jwt` | Key, Issuer, Audience, ExpiryMinutes (default 60), RefreshTokenExpiryDays (30) |
| `ConnectionStrings:DefaultConnection` | MySQL |
| `ConnectionStrings:Redis` | Redis (not yet active) |
| `MongoDb` | MongoDB connection + collection names |
| `Email` | Brevo/Sendinblue API key and sender info (used by `MailKitEmailSender` via `HttpClient`) |
| `EmailSettings` | MailKit SMTP settings (Mail, Host, Port, Login, Password) |
| `Paystack` | SecretKey, PublicKey, WebhookSecret, BaseUrl |
| `Platform` | CommissionRate (15%), BookingReferencePrefix ("HAH") |
| `App:ClientBaseUrl` | Frontend base URL (used in email links) |
| `Firebase:ServiceAccountJsonPath` | Path to firebase-service-account.json |

Secrets must **not** be committed. Use `dotnet user-secrets` locally or environment variables.

## Adding a New Feature (Typical Flow)

1. Add/update entity in `HelpAtHome.Core/Entities`, add `DbSet` in `AppDbContext`, create an `IEntityTypeConfiguration` in `HelpAtHome.Infrastructure/Data/Configurations/`.
2. Add interface to `HelpAtHome.Application/Interfaces/Repositories/`, implement in `HelpAtHome.Infrastructure/Repositories/`, add property to `IUnitOfWork` and `UnitOfWork`.
3. Add service interface to `HelpAtHome.Application/Interfaces/Services/`, implement in `HelpAtHome.Application/Services/`.
4. Register both in `ServiceCollectionExtensions.cs`.
5. Add AutoMapper mapping in `MappingProfile.cs`.
6. Add controller in `HelpAtHome.Api/Controllers/`.
7. Run `dotnet ef migrations add` if schema changed.
