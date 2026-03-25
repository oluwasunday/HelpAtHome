using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        private readonly AppDbContext _ctx;
        public BookingRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public Task<Booking?> GetByReferenceAsync(string reference)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Booking>> GetCaregiverBookingsAsync(Guid caregiverProfileId, BookingStatus? status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Booking>> GetClientBookingsAsync(Guid clientProfileId, BookingStatus? status)
        {
            throw new NotImplementedException();
        }

        public Task<Booking?> GetWithDetailsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasActiveBookingAsync(Guid caregiverProfileId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}
