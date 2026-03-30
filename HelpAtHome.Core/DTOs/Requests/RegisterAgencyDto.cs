using HelpAtHome.Core.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests
{
    public class RegisterAgencyDto
    {
        [Required] public string AgencyName { get; set; } = string.Empty;
        [Required] public string RegistrationNumber { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string PhoneNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website { get; set; }
        [Required] public AddressUpsertDto Address { get; set; } = new();
    }
}
