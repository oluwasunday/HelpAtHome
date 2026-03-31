using HelpAtHome.Shared;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IBadgeService
    {
        /// <summary>Recalculate and update the badge for a caregiver based on completed bookings and rating.</summary>
        Task RecalculateBadgeAsync(Guid caregiverProfileId);

        /// <summary>Batch-recalculate badges for all caregivers. Admin use only.</summary>
        Task RecalculateAllAsync();
    }
}
