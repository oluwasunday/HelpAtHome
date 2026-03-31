using HelpAtHome.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpAtHome.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ICaregiverProfileRepository CaregiverProfiles { get; }
        IClientProfileRepository ClientProfiles { get; }
        IAgencyRepository Agencies { get; }
        IBookingRepository Bookings { get; }
        IWalletRepository Wallets { get; }
        ITransactionRepository Transactions { get; }
        IReviewRepository Reviews { get; }
        INotificationRepository Notifications { get; }
        ISupportTicketRepository SupportTickets { get; }
        ITicketMessageRepository TicketMessages { get; }
        IEmergencyAlertRepository EmergencyAlerts { get; }
        IFamilyAccessRepository FamilyAccesses { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IOtpCodeRepository OtpCodes { get; }
        IVerificationDocumentRepository VerificationDocuments { get; }
        IServiceCategoryRepository ServiceCategories { get; }
        ICaregiverServiceRepository CaregiverServices { get; }
        IClientAddressRepository ClientAddresses { get; }
        IAgencyAddressRepository AgencyAddresses { get; }
        ICaregiverAddressRepository CaregiverAddresses { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }

}
