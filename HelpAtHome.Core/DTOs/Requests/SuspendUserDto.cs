namespace HelpAtHome.Core.DTOs.Requests
{
    // ── SuspendUserDto ────────────────────────────────────────────────────
    public class SuspendUserDto
    {
        public string Reason { get; set; } = string.Empty;
        public DateTime? SuspendedUntil { get; set; }   // null = indefinite
    }

}
