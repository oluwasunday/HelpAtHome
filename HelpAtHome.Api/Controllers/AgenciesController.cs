using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    [ApiController][Route("api/agencies")][Authorize]
    public class AgenciesController : ControllerBase
    {
        [HttpPost("register")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> RegisterAgency([FromForm] RegisterAgencyDto dto) 
        { 
            return Ok(new { Message = "Agency registered successfully", AgencyId = Guid.NewGuid() });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAgency(Guid id)
        {
            return Ok(new
            {
                Id = id,
                Name = "Sample Agency",
                Address = "123 Main St, Cityville",
                Phone = "555-1234",
                Email = ""
            });
        }

        [HttpGet("{id}/caregivers")]
        [Authorize(Roles = "AgencyAdmin,Admin,SuperAdmin")]
        public async Task<IActionResult> GetAgencyCaregivers(Guid id, int page = 1, int size = 10)
        {
            return Ok(new
            {
                Caregivers = new[]
                {
                    new { Id = Guid.NewGuid(), FullName = "John Doe", Email = "" }
                }
            });
        }

        [HttpPost("{id}/add-caregiver")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> AddCaregiver(Guid id, [FromBody] RegisterAgencyCaregiverDto dto)
        {
            return Ok(new { Message = "Caregiver added successfully", CaregiverId = Guid.NewGuid() });
        }

        [HttpDelete("{agencyId}/caregivers/{caregiverId}")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> RemoveCaregiver(Guid agencyId, Guid caregiverId)
        {
            return Ok(new { Message = "Caregiver removed successfully" });
        }

        [HttpGet("{id}/bookings")]
        [Authorize(Roles = "AgencyAdmin")]
        public async Task<IActionResult> GetAgencyBookings(Guid id, int page = 1, int size = 10)
        {
            return Ok(new
            {
                Bookings = new[]
                {
                    new { Id = Guid.NewGuid(), ClientName = "Jane Smith", Date = DateTime.UtcNow.AddDays(1) }
                }
            });
        }

        [HttpGet("{id}/activity-logs")]
        [Authorize(Roles = "AgencyAdmin,Admin,SuperAdmin")]
        public async Task<IActionResult> GetActivityLogs(Guid id, int page = 1, int size = 20)
        { 
            /* pulls from MongoDB AgencyActivityLog collection */ 
            return Ok(new
            {
                Logs = new[]
                {
                    new { Id = Guid.NewGuid(), Action = "Added Caregiver", Timestamp = DateTime.UtcNow }
                }
            });
        }
    }
}
