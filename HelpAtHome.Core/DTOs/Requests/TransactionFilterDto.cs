using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── TransactionFilterDto ─────────────────────────────────────────────
    public class TransactionFilterDto
    {
        public TransactionType? Type { get; set; }
        public TransactionStatus? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
