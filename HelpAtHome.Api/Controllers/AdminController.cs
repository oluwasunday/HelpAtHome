using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Admin panel — dashboard stats, user management, document review, and dispute resolution.</summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IBookingService _bookingService;
        private readonly IAuditLogService _auditLog;
        private readonly IBadgeService _badge;

        public AdminController(
            IAdminService adminService,
            IBookingService bookingService,
            IAuditLogService auditLog,
            IBadgeService badge)
        {
            _adminService = adminService;
            _bookingService = bookingService;
            _auditLog = auditLog;
            _badge = badge;
        }

        // ── Dashboard ─────────────────────────────────────────────────────────

        /// <summary>Get high-level platform statistics for the admin dashboard.</summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var res = await _adminService.GetDashboardAsync();
            return Ok(res.Data);
        }

        // ── User management ───────────────────────────────────────────────────

        /// <summary>Get a paginated, filterable list of all platform users.</summary>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] AdminUserFilterDto filter)
        {
            var res = await _adminService.GetUsersAsync(filter);
            return Ok(res.Data);
        }

        /// <summary>Get full details of a single user by ID.</summary>
        [HttpGet("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var res = await _adminService.GetUserAsync(id);
            if (!res.IsSuccess) return NotFound(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        /// <summary>Suspend a user account with an optional reason and duration.</summary>
        [HttpPost("users/{id}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.SuspendUserAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "User suspended successfully" });
        }

        /// <summary>Lift a suspension from a user account.</summary>
        [HttpPost("users/{id}/unsuspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnsuspendUser(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.UnsuspendUserAsync(adminId, id);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "User unsuspended successfully" });
        }

        /// <summary>Soft-delete a user account from the platform.</summary>
        [HttpDelete("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.DeleteUserAsync(adminId, id);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "User deleted successfully" });
        }

        // ── Document review ───────────────────────────────────────────────────

        /// <summary>Get a paginated list of caregiver verification documents awaiting review.</summary>
        [HttpGet("documents/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingDocuments(int page = 1, int size = 20)
        {
            var res = await _adminService.GetPendingDocumentsAsync(page, size);
            return Ok(res.Data);
        }

        /// <summary>Approve or reject a submitted verification document.</summary>
        [HttpPatch("documents/{id}/review")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReviewDocument(Guid id, [FromBody] ReviewDocumentDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.ReviewDocumentAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = dto.IsApproved ? "Document approved" : "Document rejected" });
        }

        // ── Agency management ─────────────────────────────────────────────────

        /// <summary>Override the commission rates charged to a specific agency.</summary>
        [HttpPost("agencies/{id}/commission")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetCommission(Guid id, [FromBody] SetCommissionDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _adminService.SetAgencyCommissionAsync(adminId, id, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Commission rates updated" });
        }

        // ── Dispute management ────────────────────────────────────────────────

        /// <summary>Get a paginated list of bookings currently in a disputed state.</summary>
        [HttpGet("disputes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOpenDisputes(int page = 1, int size = 20)
        {
            var res = await _adminService.GetOpenDisputesAsync(page, size);
            return Ok(res.Data);
        }

        /// <summary>Resolve a booking dispute, optionally issuing a refund to the client.</summary>
        [HttpPost("bookings/{id}/resolve-dispute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeDto dto)
        {
            var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _bookingService.AdminResolveDisputeAsync(adminId, id, dto.Resolution, dto.RefundClient);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Dispute resolved successfully" });
        }

        // ── Audit Logs ────────────────────────────────────────────────────────

        /// <summary>Query the platform audit log with optional filters. Admin only.</summary>
        [HttpGet("audit-logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
        {
            var res = await _auditLog.QueryAsync(filter);
            return Ok(res.Data);
        }

        // ── Badges ───────────────────────────────────────────────────────────

        /// <summary>Batch-recalculate badges for all caregivers. Admin only.</summary>
        [HttpPost("badges/recalculate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RecalculateAllBadges()
        {
            await _badge.RecalculateAllAsync();
            return Ok(new { Message = "Badges recalculated for all caregivers." });
        }
    }
}
