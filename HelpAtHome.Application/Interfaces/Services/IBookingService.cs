using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Enums;
using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<Result<BookingDto>> CreateBookingAsync(Guid clientUserId, CreateBookingDto dto);
        Task<Result<BookingDto>> AcceptBookingAsync(Guid caregiverUserId, Guid bookingId);
        Task<Result<BookingDto>> StartBookingAsync(Guid caregiverUserId, Guid bookingId);
        Task<Result<BookingDto>> CompleteBookingAsync(Guid caregiverUserId, Guid bookingId);
        Task<Result> CancelBookingAsync(Guid userId, Guid bookingId, string reason);
        Task<Result<BookingDto>> GetBookingAsync(Guid bookingId, Guid requestingUserId);
        Task<Result<PagedResult<BookingDto>>> GetClientBookingsAsync(Guid clientUserId, BookingStatus? status, int page, int size);
        Task<Result<PagedResult<BookingDto>>> GetCaregiverBookingsAsync(Guid caregiverUserId, BookingStatus? status, int page, int size);
        Task<Result> RaiseDisputeAsync(Guid userId, Guid bookingId, string reason);
        Task<Result> AdminResolveDisputeAsync(Guid adminId, Guid bookingId, string resolution, bool refundClient);
        //Task<object?> CreateBookingAsync(Guid guid, CreateBookingDto dto);
    }

}
