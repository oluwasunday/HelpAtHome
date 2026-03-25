namespace HelpAtHome.Core.DTOs.Responses
{
    // ── AdminDashboardDto ─────────────────────────────────────────────────
    public class AdminDashboardDto
    {
        // Users
        public int TotalClients { get; set; }
        public int TotalCaregivers { get; set; }
        public int TotalAgencies { get; set; }
        public int PendingVerifications { get; set; }
        public int SuspendedUsers { get; set; }
        // Bookings
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedThisMonth { get; set; }
        public int OpenDisputes { get; set; }
        // Revenue
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueTotalAllTime { get; set; }
        // Alerts
        public int ActiveEmergencyAlerts { get; set; }
        public int OpenSupportTickets { get; set; }
    }

}
