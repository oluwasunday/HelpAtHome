using System.ComponentModel.DataAnnotations;

namespace HelpAtHome.Core.DTOs.Requests
{
    public class WithdrawalDto
    {
        [Required]
        [Range(2000, 10_000_000, ErrorMessage = "Amount must be between ₦2,000 and ₦10,000,000.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Bank name must be between 2 and 100 characters.")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Account name must be between 2 and 100 characters.")]
        public string AccountName { get; set; } = string.Empty;
    }
}
