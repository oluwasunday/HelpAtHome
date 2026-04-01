using System.Security.Claims;

namespace HelpAtHome.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Safely extracts the authenticated user's Guid from the "sub" JWT claim.
        /// Returns Guid.Empty if the claim is missing or malformed — never throws.
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("sub")
                     ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
