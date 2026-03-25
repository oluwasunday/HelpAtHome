using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Common
{
    public class AddressUpsertDto
    {
        [Required]
        [MaxLength(200)]
        public string Line1 { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Line2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string Locality { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? City { get; set; }

        [Required]
        [MaxLength(100)]
        public string LGA { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Country { get; set; } = "Nigeria";

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
