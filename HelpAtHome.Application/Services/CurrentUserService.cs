using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HelpAtHome.Application.Services
{
    public class CurrentUserService : ICurrentUser
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Per-request cache — populated on first access, null-safe flags track load state
        private User? _user;
        private CaregiverProfile? _caregiverProfile;
        private ClientProfile? _clientProfile;
        private Wallet? _wallet;

        private bool _userLoaded;
        private bool _caregiverLoaded;
        private bool _clientLoaded;
        private bool _walletLoaded;

        public CurrentUserService(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── Synchronous (JWT claims only, no DB) ──────────────────────────────

        public Guid Id
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue("sub")
                    ?? _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(value, out var id) ? id : Guid.Empty;
            }
        }

        public bool IsAuthenticated
            => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true
               && Id != Guid.Empty;

        // ── Async lazy-loaders ────────────────────────────────────────────────

        /// <summary>
        /// Loads the User entity with CaregiverProfile and ClientProfile navigations
        /// included. A single DB call per request; subsequent calls are free.
        /// </summary>
        public async Task<User?> GetUserAsync(CancellationToken ct = default)
        {
            if (_userLoaded) return _user;

            _user = await _uow.Users.GetByIdWithProfileAsync(Id);
            _userLoaded = true;

            // Seed the profile caches from the already-loaded navigation properties
            // so that GetCaregiverProfileAsync / GetClientProfileAsync are also free.
            if (_user != null)
            {
                if (!_caregiverLoaded)
                {
                    _caregiverProfile = _user.CaregiverProfile;
                    _caregiverLoaded = true;
                }
                if (!_clientLoaded)
                {
                    _clientProfile = _user.ClientProfile;
                    _clientLoaded = true;
                }
            }

            return _user;
        }

        /// <summary>
        /// Returns the CaregiverProfile for the current user. If GetUserAsync has
        /// already been called this request, returns the cached value with no
        /// additional DB round-trip.
        /// </summary>
        public async Task<CaregiverProfile?> GetCaregiverProfileAsync(CancellationToken ct = default)
        {
            if (_caregiverLoaded) return _caregiverProfile;

            // Trigger the user load which seeds both profile caches
            await GetUserAsync(ct);
            return _caregiverProfile;
        }

        /// <summary>
        /// Returns the ClientProfile for the current user. If GetUserAsync has
        /// already been called this request, returns the cached value with no
        /// additional DB round-trip.
        /// </summary>
        public async Task<ClientProfile?> GetClientProfileAsync(CancellationToken ct = default)
        {
            if (_clientLoaded) return _clientProfile;

            await GetUserAsync(ct);
            return _clientProfile;
        }

        /// <summary>
        /// Returns the Wallet for the current user. Separate from the user load
        /// since Wallet is not a navigation on User. One DB call per request.
        /// </summary>
        public async Task<Wallet?> GetWalletAsync(CancellationToken ct = default)
        {
            if (_walletLoaded) return _wallet;

            _wallet = await _uow.Wallets.GetByUserIdAsync(Id, ct);
            _walletLoaded = true;
            return _wallet;
        }
    }
}
