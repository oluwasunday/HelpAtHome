namespace HelpAtHome.Core.DTOs.Common
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string Locality { get; set; } = string.Empty;
        public string? City { get; set; }
        public string LGA { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "Nigeria";
        public string? PostalCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

}
