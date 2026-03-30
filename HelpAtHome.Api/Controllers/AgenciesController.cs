using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    [ApiController][Route("api/agencies")]
    [Authorize]
    public class AgenciesController : ControllerBase
    {
        private readonly IAgencyService _agencyService;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IAuthService _authService;

        public AgenciesController(
            IAgencyService agencyService,
            IAgencyRepository agencyRepository,
            IAuthService authService)
        {
            _agencyService = agencyService;
            _agencyRepository = agencyRepository;
            _authService = authService;
        }

        [HttpPost("register")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> RegisterAgency([FromBody] RegisterAgencyDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _agencyService.RegisterAgencyAsync(dto, userId);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAgency(Guid id)
        {
            var res = await _agencyService.GetAgencyAsync(id);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] UpdateAgencyDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _agencyService.UpdateAgencyAsync(id, dto, userId);
            if (!res.IsSuccess)
                return res.ErrorMessage == "Unauthorized"
                    ? Forbid()
                    : BadRequest(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        [HttpGet("{id}/caregivers")]
        [Authorize(Roles = "AgencyAdmin,Admin,SuperAdmin")]
        public async Task<IActionResult> GetAgencyCaregivers(Guid id, int page = 1, int size = 10)
        {
            if (!await IsAdminOrOwnsAgency(id)) return Forbid();
            var res = await _agencyService.GetAgencyCaregiversAsync(id, page, size);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        [HttpPost("{id}/add-caregiver")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> AddCaregiver(Guid id, [FromBody] RegisterAgencyCaregiverDto dto)
        {
            var callerId = Guid.Parse(User.FindFirst("sub")!.Value);
            var agency = await _agencyRepository.GetByIdAsync(id);
            if (agency == null)
                return NotFound(new { Message = "Agency not found" });
            if (agency.AgencyAdminUserId != callerId)
                return Forbid();

            var res = await _authService.RegisterAgencyCaregiverAsync(dto, id);
            if (!res.IsSuccess)
                return BadRequest(new { Message = res.ErrorMessage });

            return Ok(new { Message = "Caregiver added successfully", UserId = res.Data });
        }

        [HttpDelete("{agencyId}/caregivers/{caregiverId}")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> RemoveCaregiver(Guid agencyId, Guid caregiverId)
        {
            var requestingUserId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _agencyService.RemoveCaregiverAsync(agencyId, caregiverId, requestingUserId);
            if (!res.IsSuccess)
                return res.ErrorMessage == "Unauthorized"
                    ? Forbid()
                    : BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Caregiver removed successfully" });
        }

        [HttpGet("{id}/bookings")]
        [Authorize(Roles = "AgencyAdmin,Admin,SuperAdmin")]
        public async Task<IActionResult> GetAgencyBookings(Guid id, int page = 1, int size = 10)
        {
            if (!await IsAdminOrOwnsAgency(id)) return Forbid();
            var res = await _agencyService.GetAgencyBookingsAsync(id, page, size);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        [HttpPost("{id}/verify")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> VerifyAgency(Guid id, [FromBody] VerifyAgencyDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _agencyService.VerifyAgencyAsync(id, dto, adminId);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = dto.IsApproved ? "Agency approved" : "Agency rejected" });
        }

        [HttpGet("{id}/activity-logs")]
        [Authorize(Roles = "AgencyAdmin,Admin,SuperAdmin")]
        public async Task<IActionResult> GetActivityLogs(Guid id, int page = 1, int size = 20)
        {
            if (!await IsAdminOrOwnsAgency(id)) return Forbid();
            // TODO: implement when AuditLogService is enabled (MongoDB collection)
            return Ok(new { Message = "Activity log not yet implemented" });
        }

        private async Task<bool> IsAdminOrOwnsAgency(Guid agencyId)
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return true;
            var callerId = Guid.Parse(User.FindFirst("sub")!.Value);
            var agency = await _agencyRepository.GetByIdAsync(agencyId);
            return agency != null && agency.AgencyAdminUserId == callerId;
        }
    }
}
