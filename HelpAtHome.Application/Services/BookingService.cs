using AutoMapper;
using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HelpAtHome.Application.Services
{
    public class BookingService : IBookingService
    {
        public readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;
        private readonly IConfiguration _config;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IUnitOfWork uow,
            IMapper mapper,
            INotificationService notification,
            IConfiguration config,
            ILogger<BookingService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _notification = notification;
            _config = config;
            _logger = logger;
        }

        // ── Create ────────────────────────────────────────────────────────────

        public async Task<Result<BookingDto>> CreateBookingAsync(Guid clientUserId, CreateBookingDto dto)
        {
            var clientProfile = await _uow.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == clientUserId);
            if (clientProfile == null)
                return Result<BookingDto>.Fail("Client profile not found.");

            var caregiver = await _uow.CaregiverProfiles.GetByIdAsync(dto.CaregiverProfileId);
            if (caregiver == null || !caregiver.IsAvailable)
                return Result<BookingDto>.Fail("Caregiver is not available.");

            if (clientProfile.RequireVerifiedOnly
                && caregiver.VerificationStatus != VerificationStatus.Approved)
                return Result<BookingDto>.Fail("This client requires a verified caregiver.");

            if (await _uow.Bookings.HasActiveBookingAsync(
                    caregiver.Id, dto.ScheduledStartDate, dto.ScheduledEndDate))
                return Result<BookingDto>.Fail("Caregiver has a conflicting booking for those dates.");

            var amount = CalculateAmount(caregiver, dto);
            var platformFee = amount * (_config.GetValue<decimal>("Platform:CommissionRate") / 100);

            var wallet = await _uow.Wallets.FirstOrDefaultAsync(w => w.UserId == clientUserId);
            if (wallet == null || wallet.Balance < amount)
                return Result<BookingDto>.Fail("Insufficient wallet balance.");

            await _uow.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingReference = GenerateReference(),
                    ClientProfileId = clientProfile.Id,
                    CaregiverProfileId = caregiver.Id,
                    ServiceCategoryId = dto.ServiceCategoryId,
                    Frequency = dto.Frequency,
                    ScheduledStartDate = dto.ScheduledStartDate,
                    ScheduledEndDate = dto.ScheduledEndDate,
                    DailyStartTime = dto.DailyStartTime,
                    DailyEndTime = dto.DailyEndTime,
                    SpecialInstructions = dto.SpecialInstructions,
                    ClientAddress = dto.Address,
                    ClientLatitude = dto.Latitude,
                    ClientLongitude = dto.Longitude,
                    AgreedAmount = amount,
                    PlatformFee = platformFee,
                    CaregiverEarnings = amount - platformFee,
                    Status = BookingStatus.Pending,
                    PaymentStatus = PaymentStatus.Paid
                };
                await _uow.Bookings.AddAsync(booking);

                var balanceBefore = wallet.Balance;
                wallet.Balance -= amount;
                wallet.TotalSpent += amount;
                _uow.Wallets.Update(wallet);

                await _uow.Transactions.AddAsync(new Transaction
                {
                    Id = Guid.NewGuid(),
                    TransactionReference = Guid.NewGuid().ToString("N"),
                    WalletId = wallet.Id,
                    BookingId = booking.Id,
                    Type = TransactionType.Booking,
                    Status = TransactionStatus.Success,
                    Amount = -amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    Description = $"Payment for booking {booking.BookingReference}"
                });

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();

                await _notification.SendAsync(
                    caregiver.UserId,
                    "New Booking Request",
                    $"You have a new booking request: {booking.BookingReference}",
                    "booking", booking.Id.ToString());

                // Re-fetch with full navigations for accurate DTO mapping
                var created = await _uow.Bookings.GetWithDetailsAsync(booking.Id);
                return Result<BookingDto>.Ok(_mapper.Map<BookingDto>(created));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                _logger.LogError(ex, "CreateBookingAsync failed for client {UserId}", clientUserId);
                throw;
            }
        }

        // ── Accept ────────────────────────────────────────────────────────────

        public async Task<Result<BookingDto>> AcceptBookingAsync(Guid caregiverUserId, Guid bookingId)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result<BookingDto>.Fail("Booking not found.");

            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == caregiverUserId);
            if (caregiverProfile == null || booking.CaregiverProfileId != caregiverProfile.Id)
                return Result<BookingDto>.Fail("Access denied.");

            if (booking.Status != BookingStatus.Pending)
                return Result<BookingDto>.Fail($"Cannot accept a booking with status '{booking.Status}'.");

            booking.Status = BookingStatus.Accepted;
            booking.AcceptedAt = DateTime.UtcNow;
            _uow.Bookings.Update(booking);
            await _uow.SaveChangesAsync();

            await _notification.SendAsync(
                booking.ClientProfile.UserId,
                "Booking Accepted",
                $"Your booking {booking.BookingReference} has been accepted by the caregiver.",
                "booking", booking.Id.ToString());

            return Result<BookingDto>.Ok(_mapper.Map<BookingDto>(booking));
        }

        // ── Start ─────────────────────────────────────────────────────────────

        public async Task<Result<BookingDto>> StartBookingAsync(Guid caregiverUserId, Guid bookingId)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result<BookingDto>.Fail("Booking not found.");

            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == caregiverUserId);
            if (caregiverProfile == null || booking.CaregiverProfileId != caregiverProfile.Id)
                return Result<BookingDto>.Fail("Access denied.");

            if (booking.Status != BookingStatus.Accepted)
                return Result<BookingDto>.Fail($"Cannot start a booking with status '{booking.Status}'.");

            booking.Status = BookingStatus.InProgress;
            booking.StartedAt = DateTime.UtcNow;
            _uow.Bookings.Update(booking);
            await _uow.SaveChangesAsync();

            await _notification.SendAsync(
                booking.ClientProfile.UserId,
                "Care Session Started",
                $"Your caregiver has started the session for booking {booking.BookingReference}.",
                "booking", booking.Id.ToString());

            return Result<BookingDto>.Ok(_mapper.Map<BookingDto>(booking));
        }

        // ── Complete ──────────────────────────────────────────────────────────

        public async Task<Result<BookingDto>> CompleteBookingAsync(Guid caregiverUserId, Guid bookingId)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result<BookingDto>.Fail("Booking not found.");

            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == caregiverUserId);
            if (caregiverProfile == null || booking.CaregiverProfileId != caregiverProfile.Id)
                return Result<BookingDto>.Fail("Access denied.");

            if (booking.Status != BookingStatus.InProgress)
                return Result<BookingDto>.Fail($"Cannot complete a booking with status '{booking.Status}'.");

            var caregiverWallet = await _uow.Wallets
                .FirstOrDefaultAsync(w => w.UserId == caregiverUserId);
            if (caregiverWallet == null)
                return Result<BookingDto>.Fail("Caregiver wallet not found.");

            await _uow.BeginTransactionAsync();
            try
            {
                booking.Status = BookingStatus.Completed;
                booking.CompletedAt = DateTime.UtcNow;
                _uow.Bookings.Update(booking);

                // Release caregiver earnings from escrow to their wallet
                var balanceBefore = caregiverWallet.Balance;
                caregiverWallet.Balance += booking.CaregiverEarnings;
                caregiverWallet.TotalEarned += booking.CaregiverEarnings;
                _uow.Wallets.Update(caregiverWallet);

                await _uow.Transactions.AddAsync(new Transaction
                {
                    Id = Guid.NewGuid(),
                    TransactionReference = Guid.NewGuid().ToString("N"),
                    WalletId = caregiverWallet.Id,
                    BookingId = booking.Id,
                    Type = TransactionType.Payout,
                    Status = TransactionStatus.Success,
                    Amount = booking.CaregiverEarnings,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = caregiverWallet.Balance,
                    Description = $"Earnings for completed booking {booking.BookingReference}"
                });

                caregiverProfile.TotalCompletedBookings++;
                _uow.CaregiverProfiles.Update(caregiverProfile);

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();

                await _notification.SendAsync(
                    booking.ClientProfile.UserId,
                    "Booking Completed",
                    $"Your booking {booking.BookingReference} has been completed. Please leave a review!",
                    "booking", booking.Id.ToString());

                return Result<BookingDto>.Ok(_mapper.Map<BookingDto>(booking));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                _logger.LogError(ex, "CompleteBookingAsync failed for booking {BookingId}", bookingId);
                throw;
            }
        }

        // ── Cancel ────────────────────────────────────────────────────────────

        public async Task<Result> CancelBookingAsync(Guid userId, Guid bookingId, string reason)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result.Fail("Booking not found.");

            var isClient = booking.ClientProfile.UserId == userId;
            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);
            var isCaregiver = caregiverProfile != null
                              && booking.CaregiverProfileId == caregiverProfile.Id;

            if (!isClient && !isCaregiver)
                return Result.Fail("Access denied.");

            if (booking.Status is BookingStatus.Completed
                                or BookingStatus.Cancelled
                                or BookingStatus.Disputed)
                return Result.Fail($"Cannot cancel a booking with status '{booking.Status}'.");

            // Full refund when cancelling before service starts
            bool issueRefund = booking.Status is BookingStatus.Pending or BookingStatus.Accepted;

            await _uow.BeginTransactionAsync();
            try
            {
                booking.Status = BookingStatus.Cancelled;
                booking.CancellationReason = reason;
                booking.CancelledBy = isClient ? "Client" : "Caregiver";
                booking.CancelledAt = DateTime.UtcNow;
                _uow.Bookings.Update(booking);

                if (issueRefund)
                {
                    var clientWallet = await _uow.Wallets
                        .FirstOrDefaultAsync(w => w.UserId == booking.ClientProfile.UserId);
                    if (clientWallet != null)
                    {
                        var balanceBefore = clientWallet.Balance;
                        clientWallet.Balance += booking.AgreedAmount;
                        clientWallet.TotalSpent -= booking.AgreedAmount;
                        _uow.Wallets.Update(clientWallet);

                        await _uow.Transactions.AddAsync(new Transaction
                        {
                            Id = Guid.NewGuid(),
                            TransactionReference = Guid.NewGuid().ToString("N"),
                            WalletId = clientWallet.Id,
                            BookingId = booking.Id,
                            Type = TransactionType.Refund,
                            Status = TransactionStatus.Success,
                            Amount = booking.AgreedAmount,
                            BalanceBefore = balanceBefore,
                            BalanceAfter = clientWallet.Balance,
                            Description = $"Refund for cancelled booking {booking.BookingReference}"
                        });
                    }
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();

                var notifyUserId = isClient
                    ? booking.CaregiverProfile.UserId
                    : booking.ClientProfile.UserId;

                await _notification.SendAsync(
                    notifyUserId,
                    "Booking Cancelled",
                    $"Booking {booking.BookingReference} was cancelled. Reason: {reason}",
                    "booking", booking.Id.ToString());

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                _logger.LogError(ex, "CancelBookingAsync failed for booking {BookingId}", bookingId);
                throw;
            }
        }

        // ── Get single ────────────────────────────────────────────────────────

        public async Task<Result<BookingDto>> GetBookingAsync(Guid bookingId, Guid requestingUserId)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result<BookingDto>.Fail("Booking not found.");

            var isClient = booking.ClientProfile.UserId == requestingUserId;
            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == requestingUserId);
            var isCaregiver = caregiverProfile != null
                              && booking.CaregiverProfileId == caregiverProfile.Id;

            if (!isClient && !isCaregiver)
                return Result<BookingDto>.Fail("Access denied.");

            return Result<BookingDto>.Ok(_mapper.Map<BookingDto>(booking));
        }

        // ── Paged lists ───────────────────────────────────────────────────────

        public async Task<Result<PagedResult<BookingDto>>> GetClientBookingsAsync(
            Guid clientUserId, BookingStatus? status, int page, int size)
        {
            size = Math.Clamp(size, 1, 50);
            var clientProfile = await _uow.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == clientUserId);
            if (clientProfile == null)
                return Result<PagedResult<BookingDto>>.Fail("Client profile not found.");

            var all = (await _uow.Bookings
                .GetClientBookingsAsync(clientProfile.Id, status)).ToList();

            var paged = all.Skip((page - 1) * size).Take(size).ToList();
            return Result<PagedResult<BookingDto>>.Ok(
                new PagedResult<BookingDto>(_mapper.Map<List<BookingDto>>(paged), all.Count, page, size));
        }

        public async Task<Result<PagedResult<BookingDto>>> GetCaregiverBookingsAsync(
            Guid caregiverUserId, BookingStatus? status, int page, int size)
        {
            size = Math.Clamp(size, 1, 50);
            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == caregiverUserId);
            if (caregiverProfile == null)
                return Result<PagedResult<BookingDto>>.Fail("Caregiver profile not found.");

            var all = (await _uow.Bookings
                .GetCaregiverBookingsAsync(caregiverProfile.Id, status)).ToList();

            var paged = all.Skip((page - 1) * size).Take(size).ToList();
            return Result<PagedResult<BookingDto>>.Ok(
                new PagedResult<BookingDto>(_mapper.Map<List<BookingDto>>(paged), all.Count, page, size));
        }

        // ── Dispute ───────────────────────────────────────────────────────────

        public async Task<Result> RaiseDisputeAsync(Guid userId, Guid bookingId, string reason)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result.Fail("Booking not found.");

            var isClient = booking.ClientProfile.UserId == userId;
            var caregiverProfile = await _uow.CaregiverProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);
            var isCaregiver = caregiverProfile != null
                              && booking.CaregiverProfileId == caregiverProfile.Id;

            if (!isClient && !isCaregiver)
                return Result.Fail("Access denied.");

            if (booking.Status is not (BookingStatus.InProgress or BookingStatus.Completed))
                return Result.Fail("Disputes can only be raised on in-progress or completed bookings.");

            if (booking.HasDispute)
                return Result.Fail("A dispute is already open for this booking.");

            booking.HasDispute = true;
            booking.Status = BookingStatus.Disputed;
            _uow.Bookings.Update(booking);

            await _uow.SupportTickets.AddAsync(new SupportTicket
            {
                Id = Guid.NewGuid(),
                TicketNumber = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                RaisedByUserId = userId,
                BookingId = booking.Id,
                IsDispute = true,
                Status = TicketStatus.Open,
                Priority = TicketPriority.High,
                Subject = $"Dispute: booking {booking.BookingReference}",
                Description = reason
            });

            await _uow.SaveChangesAsync();

            return Result.Ok();
        }

        // ── Admin resolve dispute ─────────────────────────────────────────────

        public async Task<Result> AdminResolveDisputeAsync(
            Guid adminId, Guid bookingId, string resolution, bool refundClient)
        {
            var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
                return Result.Fail("Booking not found.");
            if (!booking.HasDispute || booking.Status != BookingStatus.Disputed)
                return Result.Fail("No active dispute found for this booking.");

            var ticket = await _uow.SupportTickets.FirstOrDefaultAsync(
                t => t.BookingId == bookingId
                  && t.IsDispute
                  && t.Status != TicketStatus.Resolved
                  && t.Status != TicketStatus.Closed);

            await _uow.BeginTransactionAsync();
            try
            {
                if (refundClient)
                {
                    var clientWallet = await _uow.Wallets
                        .FirstOrDefaultAsync(w => w.UserId == booking.ClientProfile.UserId);
                    if (clientWallet != null)
                    {
                        var balanceBefore = clientWallet.Balance;
                        clientWallet.Balance += booking.AgreedAmount;
                        clientWallet.TotalSpent -= booking.AgreedAmount;
                        _uow.Wallets.Update(clientWallet);

                        await _uow.Transactions.AddAsync(new Transaction
                        {
                            Id = Guid.NewGuid(),
                            TransactionReference = Guid.NewGuid().ToString("N"),
                            WalletId = clientWallet.Id,
                            BookingId = booking.Id,
                            Type = TransactionType.Refund,
                            Status = TransactionStatus.Success,
                            Amount = booking.AgreedAmount,
                            BalanceBefore = balanceBefore,
                            BalanceAfter = clientWallet.Balance,
                            Description = $"Dispute refund for booking {booking.BookingReference}"
                        });
                    }
                    booking.Status = BookingStatus.Cancelled;
                }
                else
                {
                    // Ruling in caregiver's favour — release their earnings
                    var caregiverWallet = await _uow.Wallets
                        .FirstOrDefaultAsync(w => w.UserId == booking.CaregiverProfile.UserId);
                    if (caregiverWallet != null)
                    {
                        var balanceBefore = caregiverWallet.Balance;
                        caregiverWallet.Balance += booking.CaregiverEarnings;
                        caregiverWallet.TotalEarned += booking.CaregiverEarnings;
                        _uow.Wallets.Update(caregiverWallet);

                        await _uow.Transactions.AddAsync(new Transaction
                        {
                            Id = Guid.NewGuid(),
                            TransactionReference = Guid.NewGuid().ToString("N"),
                            WalletId = caregiverWallet.Id,
                            BookingId = booking.Id,
                            Type = TransactionType.Payout,
                            Status = TransactionStatus.Success,
                            Amount = booking.CaregiverEarnings,
                            BalanceBefore = balanceBefore,
                            BalanceAfter = caregiverWallet.Balance,
                            Description = $"Dispute payout for booking {booking.BookingReference}"
                        });
                    }
                    booking.Status = BookingStatus.Completed;
                }

                booking.HasDispute = false;
                _uow.Bookings.Update(booking);

                if (ticket != null)
                {
                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolutionNote = resolution;
                    _uow.SupportTickets.Update(ticket);
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();

                await _notification.SendAsync(
                    booking.ClientProfile.UserId,
                    "Dispute Resolved",
                    $"The dispute for booking {booking.BookingReference} has been resolved.",
                    "booking", booking.Id.ToString());
                await _notification.SendAsync(
                    booking.CaregiverProfile.UserId,
                    "Dispute Resolved",
                    $"The dispute for booking {booking.BookingReference} has been resolved.",
                    "booking", booking.Id.ToString());

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                _logger.LogError(ex, "AdminResolveDisputeAsync failed for booking {BookingId}", bookingId);
                throw;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static decimal CalculateAmount(CaregiverProfile caregiver, CreateBookingDto dto)
        {
            var days = Math.Max(1, (dto.ScheduledEndDate - dto.ScheduledStartDate).TotalDays);
            return dto.Frequency switch
            {
                FrequencyType.OneTime => caregiver.DailyRate,
                FrequencyType.Daily   => caregiver.DailyRate   * (decimal)days,
                FrequencyType.Weekly  => caregiver.DailyRate   * (decimal)days,
                FrequencyType.Monthly => caregiver.MonthlyRate * (decimal)Math.Max(1, days / 30),
                _                     => caregiver.HourlyRate
            };
        }

        private string GenerateReference()
        {
            var prefix = _config.GetValue<string>("Platform:BookingReferencePrefix", "HAH");
            return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999):D4}";
        }
    }
}
