using HelpAtHome.Core.Enums;

namespace HelpAtHome.Core.DTOs.Requests
{
    // ── BookingFilterDto (admin/query) ────────────────────────────────────
    public class BookingFilterDto
    {
        public BookingStatus? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? CaregiverId { get; set; }
        public Guid? ClientId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
