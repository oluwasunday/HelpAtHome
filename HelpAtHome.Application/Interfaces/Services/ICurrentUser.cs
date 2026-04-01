using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Services
{
    /// <summary>
    /// Provides lazily-loaded, per-request access to the authenticated user's data.
    /// Each Get*Async method hits the database at most once per HTTP request;
    /// subsequent calls return the cached result with no additional DB round-trip.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// The authenticated user's ID, read directly from the JWT 'sub' claim.
        /// Available synchronously — no database call required.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// True when the request carries a valid, authenticated JWT.
        /// Available synchronously — no database call required.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Returns the full User entity (including CaregiverProfile and ClientProfile
        /// navigations if present). Loads from the database on first call; cached
        /// for all subsequent calls within the same request.
        /// </summary>
        Task<User?> GetUserAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns the CaregiverProfile for the current user, or null if the user
        /// is not a caregiver. Cached after the first call.
        /// </summary>
        Task<CaregiverProfile?> GetCaregiverProfileAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns the ClientProfile for the current user, or null if the user
        /// is not a client. Cached after the first call.
        /// </summary>
        Task<ClientProfile?> GetClientProfileAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns the Wallet for the current user, or null if none exists.
        /// Cached after the first call.
        /// </summary>
        Task<Wallet?> GetWalletAsync(CancellationToken ct = default);
    }
}
