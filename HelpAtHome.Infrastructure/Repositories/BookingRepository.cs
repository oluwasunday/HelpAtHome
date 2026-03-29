using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        private readonly AppDbContext _ctx;

        public BookingRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<Booking?> GetWithDetailsAsync(Guid id)
            => await _ctx.Bookings
                .Include(b => b.ServiceCategory)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.User)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.Address)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.Agency)
                .Include(b => b.ClientProfile).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        public async Task<Booking?> GetByReferenceAsync(string reference)
            => await _ctx.Bookings
                .FirstOrDefaultAsync(b => b.BookingReference == reference && !b.IsDeleted);

        public async Task<IEnumerable<Booking>> GetClientBookingsAsync(
            Guid clientProfileId, BookingStatus? status)
            => await _ctx.Bookings
                .Where(b => b.ClientProfileId == clientProfileId
                         && !b.IsDeleted
                         && (status == null || b.Status == status))
                .Include(b => b.ServiceCategory)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.User)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.Address)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.Agency)
                .Include(b => b.ClientProfile).ThenInclude(c => c.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetCaregiverBookingsAsync(
            Guid caregiverProfileId, BookingStatus? status)
            => await _ctx.Bookings
                .Where(b => b.CaregiverProfileId == caregiverProfileId
                         && !b.IsDeleted
                         && (status == null || b.Status == status))
                .Include(b => b.ServiceCategory)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.User)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.Address)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.CaregiverServices)
                    .ThenInclude(cs => cs.ServiceCategory)
                .Include(b => b.CaregiverProfile).ThenInclude(c => c.Agency)
                .Include(b => b.ClientProfile).ThenInclude(c => c.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<bool> HasActiveBookingAsync(
            Guid caregiverProfileId, DateTime startDate, DateTime endDate)
            => await _ctx.Bookings.AnyAsync(b =>
                b.CaregiverProfileId == caregiverProfileId &&
                !b.IsDeleted &&
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Accepted ||
                 b.Status == BookingStatus.InProgress) &&
                b.ScheduledStartDate < endDate &&
                b.ScheduledEndDate > startDate);
    }
}
