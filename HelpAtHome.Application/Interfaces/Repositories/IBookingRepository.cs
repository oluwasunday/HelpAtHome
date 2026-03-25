using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    // IBookingRepository.cs
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<Booking?> GetByReferenceAsync(string reference);
        Task<IEnumerable<Booking>> GetClientBookingsAsync(Guid clientProfileId, BookingStatus? status);
        Task<IEnumerable<Booking>> GetCaregiverBookingsAsync(Guid caregiverProfileId, BookingStatus? status);
        Task<bool> HasActiveBookingAsync(Guid caregiverProfileId, DateTime startDate, DateTime endDate);
        Task<Booking?> GetWithDetailsAsync(Guid id);
    }


}
