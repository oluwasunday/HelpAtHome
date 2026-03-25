namespace HelpAtHome.Core.DTOs.Requests.Auth
{
    // ── ResetPasswordDto ─────────────────────────────────────────────────
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    //https://localhost:7138/reset-password?token=CfDJ8CIicMlW80tEnNIVTlBRqNupHtgKLRWDiXdcfEnVzLQcxQ%2Bi6epcoIYxEkLMVrDQBmCRQ8H%2Bt2I4Z1T3p3dFL5M9mt9DjLiuFSjmtI%2BWH8AHP7j7cCcIyVVfRGGLxWQ%2BvxT5Z4mrQd1ktkzQhsK17nI6NAdmv4UFO%2BLFuSKWfKmJ13%2FkuWkf9kJRuYWiIh5CpIvyEdifwpWGclkvB%2BzRZi5VqHghiNKNn%2BT91aQDGSfc&email=superadmin@helpathoment.ng
}
