using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Tests.Fakes
{
    // ── Specialized fakes ────────────────────────────────────────────────────

    public class FakeUserRepository : IUserRepository
    {
        public readonly List<User> Data = new();

        public Task<User?> GetByPhoneAsync(string phone)
            => Task.FromResult(Data.FirstOrDefault(u => u.PhoneNumber == phone));

        public Task<User?> GetByIdWithProfileAsync(Guid id)
            => Task.FromResult(Data.FirstOrDefault(u => u.Id == id));

        public Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
            => Task.FromResult<IEnumerable<User>>(Data.Where(u => u.Role == role).ToList());

        public Task<bool> PhoneExistsAsync(string phoneNumber)
            => Task.FromResult(Data.Any(u => u.PhoneNumber == phoneNumber));
    }

    public class FakeBookingRepository : FakeGenericRepository<Booking>, IBookingRepository
    {
        public Task<Booking?> GetByReferenceAsync(string reference)
            => Task.FromResult(Data.FirstOrDefault(b => b.BookingReference == reference));

        public Task<IEnumerable<Booking>> GetClientBookingsAsync(Guid clientProfileId, BookingStatus? status)
        {
            var q = Data.Where(b => b.ClientProfileId == clientProfileId && !b.IsDeleted);
            if (status.HasValue) q = q.Where(b => b.Status == status.Value);
            return Task.FromResult<IEnumerable<Booking>>(q.ToList());
        }

        public Task<IEnumerable<Booking>> GetCaregiverBookingsAsync(Guid caregiverProfileId, BookingStatus? status)
        {
            var q = Data.Where(b => b.CaregiverProfileId == caregiverProfileId && !b.IsDeleted);
            if (status.HasValue) q = q.Where(b => b.Status == status.Value);
            return Task.FromResult<IEnumerable<Booking>>(q.ToList());
        }

        public Task<bool> HasActiveBookingAsync(Guid caregiverProfileId, DateTime startDate, DateTime endDate)
        {
            var has = Data.Any(b =>
                b.CaregiverProfileId == caregiverProfileId
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.Completed
                && b.ScheduledStartDate < endDate
                && b.ScheduledEndDate > startDate);
            return Task.FromResult(has);
        }

        public Task<Booking?> GetWithDetailsAsync(Guid id)
            => Task.FromResult(Data.FirstOrDefault(b => b.Id == id && !b.IsDeleted));

        public Task<(IEnumerable<Booking> Items, int Total)> GetAgencyBookingsAsync(Guid agencyId, int page, int size)
        {
            var all = Data.Where(b => !b.IsDeleted).ToList();
            var total = all.Count;
            var items = all.Skip((page - 1) * size).Take(size);
            return Task.FromResult<(IEnumerable<Booking>, int)>((items, total));
        }

        public Task<(decimal AllTime, decimal ThisMonth, decimal ThisWeek)> GetRevenueStatsAsync()
            => Task.FromResult<(decimal, decimal, decimal)>((0m, 0m, 0m));
    }

    public class FakeWalletRepository : FakeGenericRepository<Wallet>, IWalletRepository
    {
        public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(Data.FirstOrDefault(w => w.UserId == userId && !w.IsDeleted));

        public Task CreditAsync(Guid walletId, decimal amount, CancellationToken ct = default)
        {
            var wallet = Data.FirstOrDefault(w => w.Id == walletId);
            if (wallet != null) wallet.Balance += amount;
            return Task.CompletedTask;
        }

        public Task DebitAsync(Guid walletId, decimal amount, CancellationToken ct = default)
        {
            var wallet = Data.FirstOrDefault(w => w.Id == walletId);
            if (wallet != null) wallet.Balance -= amount;
            return Task.CompletedTask;
        }
    }

    public class FakeCaregiverProfileRepository : FakeGenericRepository<CaregiverProfile>, ICaregiverProfileRepository
    {
        public Task<(IEnumerable<CaregiverProfile> Items, int Total)> SearchAsync(CaregiverSearchDto filter)
            => Task.FromResult<(IEnumerable<CaregiverProfile>, int)>((Data.ToList(), Data.Count));

        public Task<(IEnumerable<CaregiverProfile> Items, int Total)> GetPendingVerificationAsync(int page, int pageSize)
        {
            var pending = Data.Where(c => c.VerificationStatus == VerificationStatus.Pending).ToList();
            return Task.FromResult<(IEnumerable<CaregiverProfile>, int)>((pending.Skip((page - 1) * pageSize).Take(pageSize).ToList(), pending.Count));
        }

        public Task<CaregiverProfile?> GetWithDocumentsAsync(Guid id)
            => Task.FromResult(Data.FirstOrDefault(c => c.Id == id));

        public Task<CaregiverProfile?> GetByUserIdAsync(Guid userId)
            => Task.FromResult(Data.FirstOrDefault(c => c.UserId == userId));

        public Task UpdateBadgeAsync(Guid caregiverProfileId)
            => Task.CompletedTask;
    }

    public class FakeClientProfileRepository : FakeGenericRepository<ClientProfile>, IClientProfileRepository
    {
        public Task<ClientProfile> GetByUserIdAsync(Guid userId)
        {
            var profile = Data.FirstOrDefault(c => c.UserId == userId);
            return Task.FromResult(profile ?? new ClientProfile { UserId = userId });
        }
    }

    public class FakeAgencyRepository : FakeGenericRepository<Agency>, IAgencyRepository
    {
        public Task<bool> RegistrationNumberExistsAsync(string registrationNumber)
            => Task.FromResult(Data.Any(a => a.RegistrationNumber == registrationNumber));

        public Task<Agency?> GetWithDetailsAsync(Guid agencyId)
            => Task.FromResult(Data.FirstOrDefault(a => a.Id == agencyId && !a.IsDeleted));

        public Task<(IEnumerable<CaregiverProfile> Items, int Total)> GetCaregiversPagedAsync(
            Guid agencyId, int page, int size)
            => Task.FromResult<(IEnumerable<CaregiverProfile>, int)>((new List<CaregiverProfile>(), 0));
    }

    public class FakeTransactionRepository : FakeGenericRepository<Transaction>, ITransactionRepository { }
    public class FakeRefreshTokenRepository : FakeGenericRepository<RefreshToken>, IRefreshTokenRepository { }
    public class FakeOtpCodeRepository : FakeGenericRepository<OtpCode>, IOtpCodeRepository { }
    public class FakeSupportTicketRepository : FakeGenericRepository<SupportTicket>, ISupportTicketRepository { }
    public class FakeNotificationRepository : FakeGenericRepository<Notification>, INotificationRepository { }
    public class FakeCaregiverServiceRepository : FakeGenericRepository<CaregiverService>, ICaregiverServiceRepository { }
    public class FakeReviewRepository : FakeGenericRepository<Review>, IReviewRepository { }
    public class FakeEmergencyAlertRepository : FakeGenericRepository<EmergencyAlert>, IEmergencyAlertRepository { }
    public class FakeFamilyAccessRepository : FakeGenericRepository<FamilyAccess>, IFamilyAccessRepository { }
    public class FakeVerificationDocumentRepository : FakeGenericRepository<VerificationDocument>, IVerificationDocumentRepository
    {
        public Task<(IEnumerable<VerificationDocument> Items, int Total)> GetPendingPagedAsync(int page, int size)
            => Task.FromResult<(IEnumerable<VerificationDocument>, int)>((new List<VerificationDocument>(), 0));
    }
    public class FakeServiceCategoryRepository : FakeGenericRepository<ServiceCategory>, IServiceCategoryRepository { }

    public class FakeClientAddressRepository : FakeGenericRepository<ClientAddress>, IClientAddressRepository
    {
        public Task<ClientAddress?> GetByClientProfileIdAsync(Guid clientProfileId)
            => Task.FromResult(Data.FirstOrDefault(a => a.ClientProfileId == clientProfileId));

        public Task UpsertAsync(Guid clientProfileId, ClientAddress address)
        {
            var existing = Data.FirstOrDefault(a => a.ClientProfileId == clientProfileId);
            if (existing == null) Data.Add(address);
            return Task.CompletedTask;
        }
    }

    public class FakeAgencyAddressRepository : FakeGenericRepository<AgencyAddress>, IAgencyAddressRepository
    {
        public Task<AgencyAddress?> GetByAgencyIdAsync(Guid agencyId)
            => Task.FromResult(Data.FirstOrDefault(a => a.AgencyId == agencyId));

        public Task UpsertAsync(Guid agencyId, AgencyAddress address)
        {
            var existing = Data.FirstOrDefault(a => a.AgencyId == agencyId);
            if (existing == null) Data.Add(address);
            return Task.CompletedTask;
        }
    }

    public class FakeCaregiverAddressRepository : FakeGenericRepository<CaregiverAddress>, ICaregiverAddressRepository
    {
        public Task<CaregiverAddress?> GetByCaregiverProfileIdAsync(Guid caregiverProfileId)
            => Task.FromResult(Data.FirstOrDefault(a => a.CaregiverProfileId == caregiverProfileId));

        public Task UpsertAsync(Guid caregiverProfileId, CaregiverAddress address)
        {
            var existing = Data.FirstOrDefault(a => a.CaregiverProfileId == caregiverProfileId);
            if (existing == null) Data.Add(address);
            return Task.CompletedTask;
        }
    }

    // ── FakeUnitOfWork ───────────────────────────────────────────────────────

    public class FakeUnitOfWork : IUnitOfWork
    {
        // Expose concrete fakes so tests can seed data
        public readonly FakeUserRepository UserRepo = new();
        public readonly FakeBookingRepository BookingRepo = new();
        public readonly FakeWalletRepository WalletRepo = new();
        public readonly FakeCaregiverProfileRepository CaregiverProfileRepo = new();
        public readonly FakeClientProfileRepository ClientProfileRepo = new();
        public readonly FakeAgencyRepository AgencyRepo = new();
        public readonly FakeTransactionRepository TransactionRepo = new();
        public readonly FakeRefreshTokenRepository RefreshTokenRepo = new();
        public readonly FakeOtpCodeRepository OtpCodeRepo = new();
        public readonly FakeSupportTicketRepository SupportTicketRepo = new();
        public readonly FakeNotificationRepository NotificationRepo = new();
        public readonly FakeCaregiverServiceRepository CaregiverServiceRepo = new();
        public readonly FakeReviewRepository ReviewRepo = new();
        public readonly FakeEmergencyAlertRepository EmergencyAlertRepo = new();
        public readonly FakeFamilyAccessRepository FamilyAccessRepo = new();
        public readonly FakeVerificationDocumentRepository VerificationDocumentRepo = new();
        public readonly FakeServiceCategoryRepository ServiceCategoryRepo = new();
        public readonly FakeClientAddressRepository ClientAddressRepo = new();
        public readonly FakeAgencyAddressRepository AgencyAddressRepo = new();
        public readonly FakeCaregiverAddressRepository CaregiverAddressRepo = new();

        // IUnitOfWork interface implementation
        IUserRepository IUnitOfWork.Users => UserRepo;
        ICaregiverProfileRepository IUnitOfWork.CaregiverProfiles => CaregiverProfileRepo;
        IClientProfileRepository IUnitOfWork.ClientProfiles => ClientProfileRepo;
        IAgencyRepository IUnitOfWork.Agencies => AgencyRepo;
        IBookingRepository IUnitOfWork.Bookings => BookingRepo;
        IWalletRepository IUnitOfWork.Wallets => WalletRepo;
        ITransactionRepository IUnitOfWork.Transactions => TransactionRepo;
        IReviewRepository IUnitOfWork.Reviews => ReviewRepo;
        INotificationRepository IUnitOfWork.Notifications => NotificationRepo;
        ISupportTicketRepository IUnitOfWork.SupportTickets => SupportTicketRepo;
        IEmergencyAlertRepository IUnitOfWork.EmergencyAlerts => EmergencyAlertRepo;
        IFamilyAccessRepository IUnitOfWork.FamilyAccesses => FamilyAccessRepo;
        IRefreshTokenRepository IUnitOfWork.RefreshTokens => RefreshTokenRepo;
        IOtpCodeRepository IUnitOfWork.OtpCodes => OtpCodeRepo;
        IVerificationDocumentRepository IUnitOfWork.VerificationDocuments => VerificationDocumentRepo;
        IServiceCategoryRepository IUnitOfWork.ServiceCategories => ServiceCategoryRepo;
        ICaregiverServiceRepository IUnitOfWork.CaregiverServices => CaregiverServiceRepo;
        IClientAddressRepository IUnitOfWork.ClientAddresses => ClientAddressRepo;
        IAgencyAddressRepository IUnitOfWork.AgencyAddresses => AgencyAddressRepo;
        ICaregiverAddressRepository IUnitOfWork.CaregiverAddresses => CaregiverAddressRepo;

        public Task<int> SaveChangesAsync() => Task.FromResult(0);
        public Task BeginTransactionAsync() => Task.CompletedTask;
        public Task CommitAsync() => Task.CompletedTask;
        public Task RollbackAsync() => Task.CompletedTask;
        public void Dispose() { }
    }
}
