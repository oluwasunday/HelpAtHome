using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IBookingService _bookingService;

        public AdminController(IAdminService adminService, IBookingService bookingService)
        {
            _adminService = adminService;
            _bookingService = bookingService;
        }

        // ── Dashboard ─────────────────────────────────────────────────────────

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var res = await _adminService.GetDashboardAsync();
            return Ok(res.Data);
        }

        // ── User management ───────────────────────────────────────────────────

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] AdminUserFilterDto filter)
        {
            var res = await _adminService.GetUsersAsync(filter);
            return Ok(res.Data);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var res = await _adminService.GetUserAsync(id);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        [HttpPost("users/{id}/suspend")]
        public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.SuspendUserAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "User suspended successfully" });
        }

        [HttpPost("users/{id}/unsuspend")]
        public async Task<IActionResult> UnsuspendUser(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.UnsuspendUserAsync(adminId, id);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "User unsuspended successfully" });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.DeleteUserAsync(adminId, id);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "User deleted successfully" });
        }

        // ── Document review ───────────────────────────────────────────────────

        [HttpGet("documents/pending")]
        public async Task<IActionResult> GetPendingDocuments(int page = 1, int size = 20)
        {
            var res = await _adminService.GetPendingDocumentsAsync(page, size);
            return Ok(res.Data);
        }

        [HttpPatch("documents/{id}/review")]
        public async Task<IActionResult> ReviewDocument(Guid id, [FromBody] ReviewDocumentDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.ReviewDocumentAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = dto.IsApproved ? "Document approved" : "Document rejected" });
        }

        // ── Agency management ─────────────────────────────────────────────────

        [HttpPost("agencies/{id}/commission")]
        public async Task<IActionResult> SetCommission(Guid id, [FromBody] SetCommissionDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.SetAgencyCommissionAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Commission rates updated" });
        }

        // ── Dispute management ────────────────────────────────────────────────

        [HttpGet("disputes")]
        public async Task<IActionResult> GetOpenDisputes(int page = 1, int size = 20)
        {
            var res = await _adminService.GetOpenDisputesAsync(page, size);
            return Ok(res.Data);
        }

        [HttpPost("bookings/{id}/resolve-dispute")]
        public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _bookingService.AdminResolveDisputeAsync(adminId, id, dto.Resolution, dto.RefundClient);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Dispute resolved successfully" });
        }
    }
}
