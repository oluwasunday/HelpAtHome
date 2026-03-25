namespace HelpAtHome.Core.DTOs.Requests
{
    // ── SetCommissionDto (admin sets platform commission on an agency) ─────
    public class SetCommissionDto
    {
        public decimal CommissionRate { get; set; }
        public decimal AgencyCommissionRate { get; set; }
    }

}
