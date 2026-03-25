using HelpAtHome.Core.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Controllers
{
    [ApiController]
    [Route("api/caregivers")]
    [Authorize]
    public class CaregiversController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] CaregiverSearchDto filter,
            [FromQuery] int page = 1, [FromQuery] int size = 10)
        { 
            /* delegates to ICaregiverService.SearchAsync */ 
            return Ok(new
            {
                Total = 1,
                Page = page,
                Size = size,
                Caregivers = new[]
                {
                    new
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Jane Doe",
                        Bio = "Experienced caregiver with 5 years of experience.",
                        Skills = new[] { "Elderly Care", "Medication Management" },
                        HourlyRate = 15.00m,
                        IsAvailable = true,
                        VerificationStatus = "Approved"
                    }
                }
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(Guid id) 
        {
            return Ok(new
            {
                Id = id,
                FullName = "Jane Doe",
                Bio = "Experienced caregiver with 5 years of experience.",
                Skills = new[] { "Elderly Care", "Medication Management" },
                HourlyRate = 15.00m,
                IsAvailable = true,
                VerificationStatus = "Approved"
            });
        }

        [HttpPut("profile")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCaregiverProfileDto dto)
        {
            // Validate and update caregiver profile via ICaregiverService.UpdateProfileAsync
            return Ok(new { Message = "Profile updated successfully" });
        }

        [HttpPost("documents")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto dto) 
        {
            // Validate file, save to storage, and create document record via ICaregiverService.UploadDocumentAsync
            return Ok(new { Message = "Document uploaded successfully" });
        }

        [HttpGet("pending-verification")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> PendingVerification([FromQuery] int page = 1, [FromQuery] int size = 20) 
        {
            return Ok(new
            {
                Total = 1,
                Page = page,
                Size = size,
                Caregivers = new[]
                {
                    new
                    {
                        Id = Guid.NewGuid(),
                        FullName = "John Smith",
                        SubmittedAt = DateTime.UtcNow.AddDays(-2),
                        Documents = new[] { "ID Card", "Background Check" }
                    }
                }
            });
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyCaregiverDto dto) 
        {
            // Update caregiver verification status via ICaregiverService.VerifyCaregiverAsync
            return Ok(new { Message = $"Caregiver {(dto.IsApproved ? "approved" : "rejected")} successfully" });
        }

        [HttpPost("{id}/availability")]
        [Authorize(Roles = "IndividualCaregiver,AgencyCaregiver")]
        public async Task<IActionResult> ToggleAvailability(Guid id) 
        {
            // Toggle caregiver availability via ICaregiverService.ToggleAvailabilityAsync
            return Ok(new { Message = "Availability status updated successfully" });
        }
    }
}
