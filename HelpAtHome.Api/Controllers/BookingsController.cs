using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingsController(IBookingService bookingService) { _bookingService = bookingService; }

        private Guid GetUserId() => Guid.Parse(User.FindFirst("sub")!.Value);

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            var result = await _bookingService.CreateBookingAsync(GetUserId(), dto);
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _bookingService.GetBookingAsync(id, GetUserId());
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyBookings([FromQuery] BookingStatus? status,
            [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _bookingService.GetClientBookingsAsync(GetUserId(), status, page, size);
            return Ok(result);
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> MyRequests([FromQuery] BookingStatus? status,
            [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _bookingService.GetCaregiverBookingsAsync(GetUserId(), status, page, size);
            return Ok(result);
        }

        [HttpPatch("{id}/accept")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> Accept(Guid id)
        {
            var result = await _bookingService.AcceptBookingAsync(GetUserId(), id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}/start")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> Start(Guid id)
        {
            var result = await _bookingService.StartBookingAsync(GetUserId(), id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _bookingService.CompleteBookingAsync(GetUserId(), id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingDto dto)
        {
            var result = await _bookingService.CancelBookingAsync(GetUserId(), id, dto.Reason);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/dispute")]
        public async Task<IActionResult> RaiseDispute(Guid id, [FromBody] DisputeDto dto)
        {
            var result = await _bookingService.RaiseDisputeAsync(GetUserId(), id, dto.Reason);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/resolve-dispute")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeDto dto)
        {
            var result = await _bookingService.AdminResolveDisputeAsync(GetUserId(), id, dto.Resolution, dto.RefundClient);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

}
