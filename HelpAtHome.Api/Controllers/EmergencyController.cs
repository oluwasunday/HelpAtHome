using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Responses;
using HelpAtHome.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Emergency alerts — trigger, track, and respond to emergency situations.</summary>
    [ApiController]
    [Route("api/emergency")]
    [Authorize]
    [Produces("application/json")]
    public class EmergencyController : ControllerBase
    {
        private readonly IEmergencyService _emergency;

        public EmergencyController(IEmergencyService emergency)
        {
            _emergency = emergency;
        }

        /// <summary>Trigger an emergency alert. Client only. Notifies family members and active caregiver.</summary>
        [HttpPost("trigger")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TriggerAlert([FromBody] TriggerAlertDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _emergency.TriggerAlertAsync(userId, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return StatusCode(StatusCodes.Status201Created, res.Data);
        }

        /// <summary>Get a specific emergency alert. Accessible by the client, their approved family members, and admins.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAlert(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _emergency.GetAlertAsync(userId, id);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        /// <summary>Get the current client's emergency alert history. Client only.</summary>
        [HttpGet("my")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyAlerts(int page = 1, int size = 10)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _emergency.GetMyAlertsAsync(userId, page, size);
            return Ok(res.Data);
        }

        /// <summary>List all emergency alerts with optional status filter. Admin only.</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAlerts(
            [FromQuery] AlertStatus? status,
            int page = 1,
            int size = 20)
        {
            var res = await _emergency.GetActiveAlertsAsync(status, page, size);
            return Ok(res.Data);
        }

        /// <summary>Respond to an emergency alert (update status, add resolution note). Admin only.</summary>
        [HttpPatch("{id}/respond")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RespondToAlert(Guid id, [FromBody] RespondAlertDto dto)
        {
            var responderId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _emergency.RespondToAlertAsync(responderId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }
    }
}
