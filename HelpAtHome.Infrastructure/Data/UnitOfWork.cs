using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpAtHome.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        //repositories
        public IUserRepository Users { get; }
        public ICaregiverProfileRepository CaregiverProfiles { get; }
        public IBookingRepository Bookings { get; }
        public IWalletRepository Wallets { get; }
        public ITransactionRepository Transactions { get; }
        public IReviewRepository Reviews { get; }
        public INotificationRepository Notifications { get; }
        public ISupportTicketRepository SupportTickets { get; }
        public ITicketMessageRepository TicketMessages { get; }
        public IEmergencyAlertRepository EmergencyAlerts { get; }
        public IFamilyAccessRepository FamilyAccesses { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IOtpCodeRepository OtpCodes { get; }
        public IVerificationDocumentRepository VerificationDocuments { get; }
        public IServiceCategoryRepository ServiceCategories { get; }
        public IClientProfileRepository ClientProfiles { get; }
        public IAgencyRepository Agencies { get; }
        public IClientAddressRepository ClientAddresses { get; }
        public IAgencyAddressRepository AgencyAddresses { get; }
        public ICaregiverAddressRepository CaregiverAddresses { get; }

        public UnitOfWork(AppDbContext context, 
            IUserRepository users, 
            ICaregiverProfileRepository caregivers,
            IBookingRepository bookings,
            IWalletRepository wallets,
            ITransactionRepository transactions,
            IReviewRepository reviews,
            INotificationRepository notifications,
            ISupportTicketRepository tickets,
            ITicketMessageRepository ticketMessages,
            IEmergencyAlertRepository emergencies,
            IFamilyAccessRepository familyAccesses,
            IRefreshTokenRepository refreshTokens,
            IOtpCodeRepository otpCodes,
            IVerificationDocumentRepository verificationDocs,
            IServiceCategoryRepository serviceCategories,
            IClientProfileRepository clients,
            IAgencyRepository agencies,
            ICaregiverServiceRepository caregiverServices,
            IClientAddressRepository clientAddresses,
            IAgencyAddressRepository agencyAddresses,
            ICaregiverAddressRepository caregiverAddresses

            )
        {
            _context = context;
            Users = users; 
            CaregiverProfiles = caregivers; 
            Bookings = bookings;
            Wallets = wallets;
            Transactions = transactions;
            Reviews = reviews;
            Notifications = notifications;
            SupportTickets = tickets;
            TicketMessages = ticketMessages;
            EmergencyAlerts = emergencies;
            FamilyAccesses = familyAccesses;
            RefreshTokens = refreshTokens;
            OtpCodes = otpCodes;
            VerificationDocuments = verificationDocs;
            ServiceCategories = serviceCategories;
            ClientProfiles = clients;
            Agencies = agencies;
            CaregiverServices = caregiverServices;
            ClientAddresses = clientAddresses;
            AgencyAddresses = agencyAddresses;
            CaregiverAddresses = caregiverAddresses;

        }

        public ICaregiverServiceRepository CaregiverServices { get; }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitAsync()
        { await _transaction!.CommitAsync(); _transaction.Dispose(); }

        public async Task RollbackAsync()
        { await _transaction!.RollbackAsync(); _transaction.Dispose(); }

        public void Dispose() => _context.Dispose();
    }

}
