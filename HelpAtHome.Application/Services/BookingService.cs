using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.Extensions.Configuration;

namespace HelpAtHome.Application.Services
{
    public class BookingService : IBookingService
    {
        public readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;
        private readonly IConfiguration _config;

        public BookingService(IUnitOfWork uow, IMapper mapper, INotificationService notification, IConfiguration config)
        {
            _uow = uow;
            _mapper = mapper;
            _notification = notification;
            _config = config;
        }

        public Task<Result<BookingDto>> AcceptBookingAsync(Guid caregiverUserId, Guid bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<Result> AdminResolveDisputeAsync(Guid adminId, Guid bookingId, string resolution, bool refundClient)
        {
            throw new NotImplementedException();
        }

        public Task<Result> CancelBookingAsync(Guid userId, Guid bookingId, string reason)
        {
            throw new NotImplementedException();
        }

        public Task<Result<BookingDto>> CompleteBookingAsync(Guid caregiverUserId, Guid bookingId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<BookingDto>> CreateBookingAsync(Guid clientUserId, CreateBookingDto dto)
        {
            var clientProfile = await _uow.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == clientUserId);
            if (clientProfile == null) return Result<BookingDto>.Fail("Client profile not found");

            var caregiver = await _uow.CaregiverProfiles.GetByIdAsync(dto.CaregiverProfileId);
            if (caregiver == null || !caregiver.IsAvailable)
                return Result<BookingDto>.Fail("Caregiver not available");

            if (clientProfile.RequireVerifiedOnly && caregiver.VerificationStatus != VerificationStatus.Approved)
                return Result<BookingDto>.Fail("Client requires a verified caregiver");

            bool conflict = await _uow.Bookings.HasActiveBookingAsync(
                caregiver.Id, dto.ScheduledStartDate, dto.ScheduledEndDate);
            if (conflict) return Result<BookingDto>.Fail("Caregiver has a conflicting booking");

            var amount = CalculateAmount(caregiver, dto);
            var platformFee = amount * (_config.GetValue<decimal>("Platform:CommissionRate") / 100);

            var wallet = await _uow.Wallets.FirstOrDefaultAsync(w => w.UserId == clientUserId);
            if (wallet == null || wallet.Balance < amount)
                return Result<BookingDto>.Fail("Insufficient wallet balance");

            await _uow.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    BookingReference = GenerateReference(),
                    ClientProfileId = clientProfile.Id,
                    CaregiverProfileId = caregiver.Id,
                    ServiceCategoryId = dto.ServiceCategoryId,
                    Frequency = dto.Frequency,
                    ScheduledStartDate = dto.ScheduledStartDate,
                    ScheduledEndDate = dto.ScheduledEndDate,
                    AgreedAmount = amount,
                    PlatformFee = platformFee,
                    CaregiverEarnings = amount - platformFee,
                    SpecialInstructions = dto.SpecialInstructions,
                    ClientAddress = dto.Address,
                    Status = BookingStatus.Pending,
                    PaymentStatus = PaymentStatus.Paid
                };
                await _uow.Bookings.AddAsync(booking);

                wallet.Balance -= amount;
                wallet.TotalSpent += amount;
                _uow.Wallets.Update(wallet);

                var txn = new Transaction
                {
                    TransactionReference = Guid.NewGuid().ToString("N"),
                    WalletId = wallet.Id,
                    Type = TransactionType.Booking,
                    Status = TransactionStatus.Success,
                    Amount = -amount,
                    BalanceBefore = wallet.Balance + amount,
                    BalanceAfter = wallet.Balance,
                    Description = $"Booking {booking.BookingReference}",
                };
                await _uow.Transactions.AddAsync(txn);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();

                await _notification.SendAsync(caregiver.UserId,
                    "New Booking Request", $"You have a new booking: {booking.BookingReference}",
                    "booking", booking.Id.ToString());

                return Result<BookingDto>.Ok(_mapper.Map<BookingDto>(booking));
            }
            catch { await _uow.RollbackAsync(); throw; }
        }

        // Determines the total charge based on booking frequency and duration.
        private static decimal CalculateAmount(
            CaregiverProfile caregiver, CreateBookingDto dto)
        {
            var duration = (dto.ScheduledEndDate - dto.ScheduledStartDate).TotalDays;

            return dto.Frequency switch
            {
                FrequencyType.OneTime => caregiver.DailyRate,
                FrequencyType.Daily => caregiver.DailyRate * (decimal)duration,
                FrequencyType.Weekly => caregiver.DailyRate * (decimal)duration,
                FrequencyType.Monthly => caregiver.MonthlyRate * (decimal)(duration / 30),
                _ => caregiver.HourlyRate
            };
        }

        // ── GenerateReference ────────────────────────────────────────────────
        // Format: HAH-YYYYMMDD-XXXX (e.g. HAH-20240901-0042)
        private string GenerateReference()
        {
            var prefix = _config.GetValue<string>(
                "Platform:BookingReferencePrefix", "HAH");
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var seq = Random.Shared.Next(1000, 9999);
            return $"{prefix}-{datePart}-{seq:D4}";
        }


        public Task<Result<BookingDto>> GetBookingAsync(Guid bookingId, Guid requestingUserId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PagedResult<BookingDto>>> GetCaregiverBookingsAsync(Guid caregiverUserId, BookingStatus? status, int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PagedResult<BookingDto>>> GetClientBookingsAsync(Guid clientUserId, BookingStatus? status, int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RaiseDisputeAsync(Guid userId, Guid bookingId, string reason)
        {
            throw new NotImplementedException();
        }

        public Task<Result<BookingDto>> StartBookingAsync(Guid caregiverUserId, Guid bookingId)
        {
            throw new NotImplementedException();
        }
    }
}
