using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Core.DTOs.Requests
{
    public class UpdateAgencyDto
    {
        public string? AgencyName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public AddressUpsertDto? Address { get; set; }
    }
}
