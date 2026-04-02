using HelpAtHome.Api.Extensions;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Booking lifecycle — create, track, accept, start, complete, cancel, and dispute bookings.</summary>
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    [Produces("application/json")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingsController(IBookingService bookingService) { _bookingService = bookingService; }

        private Guid GetUserId() => User.GetUserId();

        /// <summary>Create a new booking request for a caregiver. Client role required.</summary>
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            var clientId = User.GetUserId();
            var result = await _bookingService.CreateBookingAsync(GetUserId(), dto);
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
        }

        /// <summary>Get details of a specific booking by ID.</summary>
        /// <remarks>Only the client, caregiver, or an admin involved in the booking may access it.</remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _bookingService.GetBookingAsync(id, GetUserId());
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>List all bookings belonging to the authenticated client.</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MyBookings([FromQuery] BookingStatus? status,
            [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _bookingService.GetClientBookingsAsync(GetUserId(), status, page, size);
            return Ok(result);
        }

        /// <summary>List all booking requests assigned to the authenticated caregiver.</summary>
        [HttpGet("my-requests")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MyRequests([FromQuery] BookingStatus? status,
            [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _bookingService.GetCaregiverBookingsAsync(GetUserId(), status, page, size);
            return Ok(result);
        }

        /// <summary>Accept a pending booking request. Caregiver role required.</summary>
        [HttpPatch("{id}/accept")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Accept(Guid id)
        {
            var result = await _bookingService.AcceptBookingAsync(GetUserId(), id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Mark a booking as started. Caregiver role required.</summary>
        [HttpPatch("{id}/start")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Start(Guid id)
        {
            var result = await _bookingService.StartBookingAsync(GetUserId(), id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Mark a booking as completed. Caregiver role required.</summary>
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _bookingService.CompleteBookingAsync(GetUserId(), id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Cancel a booking. Available to the client or caregiver involved.</summary>
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingDto dto)
        {
            var result = await _bookingService.CancelBookingAsync(GetUserId(), id, dto.Reason);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Raise a dispute against a booking. Available to either party.</summary>
        [HttpPost("{id}/dispute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RaiseDispute(Guid id, [FromBody] DisputeDto dto)
        {
            var result = await _bookingService.RaiseDisputeAsync(GetUserId(), id, dto.Reason);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>Resolve an open booking dispute. Admin only.</summary>
        [HttpPost("{id}/resolve-dispute")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeDto dto)
        {
            var result = await _bookingService.AdminResolveDisputeAsync(GetUserId(), id, dto.Resolution, dto.RefundClient);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

}
