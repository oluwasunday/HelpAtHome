using HelpAtHome.Api.Extensions;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Support tickets — raise, track, and resolve support requests.</summary>
    [ApiController]
    [Route("api/support")]
    [Authorize]
    [Produces("application/json")]
    public class SupportController : ControllerBase
    {
        private readonly ISupportService _support;

        public SupportController(ISupportService support)
        {
            _support = support;
        }

        /// <summary>Create a new support ticket. Any authenticated user can raise a ticket.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var userId = User.GetUserId();
            var res = await _support.CreateTicketAsync(userId, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return StatusCode(StatusCodes.Status201Created, res.Data);
        }

        /// <summary>Get all tickets raised by the current user.</summary>
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTickets(int page = 1, int size = 10)
        {
            var userId = User.GetUserId();
            var res = await _support.GetMyTicketsAsync(userId, page, size);
            return Ok(res.Data);
        }

        /// <summary>Get a single ticket by ID. Owners see public messages; admins see all including internal notes.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicket(Guid id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            var res = await _support.GetTicketAsync(userId, id, isAdmin);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        /// <summary>Add a message to a ticket. Admins can post internal notes.</summary>
        [HttpPost("{id}/messages")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddTicketMessageDto dto)
        {
            var userId = User.GetUserId();
            var res = await _support.AddMessageAsync(userId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return StatusCode(StatusCodes.Status201Created, res.Data);
        }

        // ── Admin endpoints ──────────────────────────────────────────────────

        /// <summary>List all tickets with optional status and priority filters. Admin only.</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] TicketStatus? status,
            [FromQuery] TicketPriority? priority,
            int page = 1,
            int size = 20)
        {
            var res = await _support.GetAllTicketsAsync(status, priority, page, size);
            return Ok(res.Data);
        }

        /// <summary>Assign a ticket to an admin user. Admin only.</summary>
        [HttpPatch("{id}/assign")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketDto dto)
        {
            var adminId = User.GetUserId();
            var res = await _support.AssignTicketAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Ticket assigned." });
        }

        /// <summary>Update a ticket's status. Admin only.</summary>
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusDto dto)
        {
            var adminId = User.GetUserId();
            var res = await _support.UpdateStatusAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Ticket status updated." });
        }

        /// <summary>Resolve a ticket with a resolution note. Admin only.</summary>
        [HttpPost("{id}/resolve")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResolveTicket(Guid id, [FromBody] ResolveTicketDto dto)
        {
            var adminId = User.GetUserId();
            var res = await _support.ResolveTicketAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Ticket resolved." });
        }
    }
}
