using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Caregiver profiles — search, update, verification, and availability.</summary>
    [ApiController]
    [Route("api/caregivers")]
    [Authorize]
    [Produces("application/json")]
    public class CaregiversController : ControllerBase
    {
        private readonly ICaregiverService _caregiverService;

        public CaregiversController(ICaregiverService caregiverService)
        {
            _caregiverService = caregiverService;
        }

        /// <summary>Search and filter caregivers. Open to the public — no token required.</summary>
        /// <remarks>Supports filtering by state, city, max hourly rate, services, gender, and more.</remarks>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] CaregiverSearchDto filter)
        {
            var result = await _caregiverService.SearchAsync(filter);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        /// <summary>Get the full profile of a caregiver by their profile ID. Open to the public.</summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var result = await _caregiverService.GetProfileAsync(id);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        /// <summary>Update the authenticated caregiver's own profile.</summary>
        [HttpPut("profile")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCaregiverProfileDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _caregiverService.UpdateProfileAsync(userId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        /// <summary>List caregivers whose verification is pending review. Admin only.</summary>
        [HttpGet("pending-verification")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PendingVerification([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _caregiverService.GetPendingVerificationAsync(page, pageSize);
            return Ok(result.Data);
        }

        /// <summary>Approve or reject a caregiver's verification application. Admin only.</summary>
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyCaregiverDto dto)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result  = await _caregiverService.VerifyCaregiverAsync(adminId, id, dto);
            return result.IsSuccess
                ? Ok(new { Message = $"Caregiver {(dto.IsApproved ? "approved" : "rejected")}." })
                : BadRequest(result.ErrorMessage);
        }

        /// <summary>Toggle the authenticated caregiver's availability status on or off.</summary>
        [HttpPost("{id}/availability")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleAvailability(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _caregiverService.ToggleAvailabilityAsync(userId);
            return result.IsSuccess
                ? Ok(new { IsAvailable = result.Data, Message = result.Data ? "You are now available." : "You are now unavailable." })
                : BadRequest(result.ErrorMessage);
        }
    }
}
