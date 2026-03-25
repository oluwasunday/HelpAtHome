namespace HelpAtHome.Core.Enums
{
    public enum BadgeLevel
    {
        New = 1,        // 0–4 completed bookings
        Verified = 2,   // 5–19 completed bookings + no open disputes
        Elite = 3,      // 20–49 completed bookings + avg rating ≥ 4.5
        Champion = 4    // 50+ completed bookings + avg rating ≥ 4.8
    }
}
