using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    /// <summary>Family access — clients invite family members to monitor their care.</summary>
    [ApiController]
    [Route("api/family")]
    [Authorize]
    [Produces("application/json")]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyAccessService _familyAccess;

        public FamilyController(IFamilyAccessService familyAccess)
        {
            _familyAccess = familyAccess;
        }

        /// <summary>Invite a family member by phone or email. Client only.</summary>
        [HttpPost("invite")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InviteFamilyMember([FromBody] InviteFamilyMemberDto dto)
        {
            var clientUserId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.InviteFamilyMemberAsync(clientUserId, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return StatusCode(StatusCodes.Status201Created, res.Data);
        }

        /// <summary>Approve a family access invitation. Family member only.</summary>
        [HttpPost("{accessId}/approve")]
        [Authorize(Roles = "FamilyMember")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveAccess(Guid accessId)
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.ApproveAccessAsync(userId, accessId);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Access approved." });
        }

        /// <summary>Revoke a family member's access. Client only.</summary>
        [HttpDelete("{accessId}")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeAccess(Guid accessId)
        {
            var clientUserId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.RevokeAccessAsync(clientUserId, accessId);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(new { Message = "Access revoked." });
        }

        /// <summary>Update a family member's access level and notification preferences. Client only.</summary>
        [HttpPatch("{accessId}")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAccess(Guid accessId, [FromBody] UpdateFamilyAccessDto dto)
        {
            var clientUserId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.UpdateAccessAsync(clientUserId, accessId, dto);
            if (!res.IsSuccess) return BadRequest(new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }

        /// <summary>List all family members the current client has invited. Client only.</summary>
        [HttpGet("my-members")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFamilyMembers()
        {
            var clientUserId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.GetClientFamilyAccessesAsync(clientUserId);
            return Ok(res.Data);
        }

        /// <summary>List all clients the current family member has access to. Family member only.</summary>
        [HttpGet("my-clients")]
        [Authorize(Roles = "FamilyMember")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyClients()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.GetMyAccessesAsync(userId);
            return Ok(res.Data);
        }

        /// <summary>Get a monitored client's care overview — profile, active booking, recent bookings and alerts. Family member only.</summary>
        [HttpGet("view/{clientUserId}")]
        [Authorize(Roles = "FamilyMember")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClientView(Guid clientUserId)
        {
            var familyMemberUserId = Guid.Parse(User.FindFirst("sub")!.Value);
            var res = await _familyAccess.GetClientViewAsync(familyMemberUserId, clientUserId);
            if (!res.IsSuccess) return StatusCode(StatusCodes.Status403Forbidden, new { Message = res.ErrorMessage });
            return Ok(res.Data);
        }
    }
}
