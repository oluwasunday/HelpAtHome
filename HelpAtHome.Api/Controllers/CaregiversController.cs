using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpAtHome.Api.Controllers
{
    [ApiController]
    [Route("api/caregivers")]
    [Authorize]
    public class CaregiversController : ControllerBase
    {
        private readonly ICaregiverService _caregiverService;

        public CaregiversController(ICaregiverService caregiverService)
        {
            _caregiverService = caregiverService;
        }

        // GET /api/caregivers?state=Lagos&city=Ikeja&maxHourlyRate=5000&page=1&pageSize=10
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] CaregiverSearchDto filter)
        {
            var result = await _caregiverService.SearchAsync(filter);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        // GET /api/caregivers/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var result = await _caregiverService.GetProfileAsync(id);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        // PUT /api/caregivers/profile
        [HttpPut("profile")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCaregiverProfileDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _caregiverService.UpdateProfileAsync(userId, dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        // GET /api/caregivers/pending-verification
        [HttpGet("pending-verification")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> PendingVerification([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _caregiverService.GetPendingVerificationAsync(page, pageSize);
            return Ok(result.Data);
        }

        // POST /api/caregivers/{id}/verify
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyCaregiverDto dto)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result  = await _caregiverService.VerifyCaregiverAsync(adminId, id, dto);
            return result.IsSuccess ? Ok(new { Message = $"Caregiver {(dto.IsApproved ? "approved" : "rejected")}." }) : BadRequest(result.ErrorMessage);
        }

        // POST /api/caregivers/{id}/availability
        [HttpPost("{id}/availability")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
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
