using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests
{
    public class InitiateDepositDto
    {
        [Required]
        [Range(500, 10_000_000, ErrorMessage = "Amount must be between ₦500 and ₦10,000,000.")]
        public decimal Amount { get; set; }

        [Required]
        [Url(ErrorMessage = "CallbackUrl must be a valid URL.")]
        public string CallbackUrl { get; set; } = string.Empty;
    }
}
