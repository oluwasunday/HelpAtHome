using HelpAtHome.Application.Interfaces;
using HelpAtHome.Application.Interfaces.Services;
using HelpAtHome.Core.Enums;

namespace HelpAtHome.Application.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly IUnitOfWork _uow;

        public BadgeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task RecalculateBadgeAsync(Guid caregiverProfileId)
        {
            var profile = await _uow.CaregiverProfiles.GetByIdAsync(caregiverProfileId);
            if (profile == null) return;

            var newBadge = CalculateBadge(profile.TotalCompletedBookings, profile.AverageRating);

            if (profile.Badge == newBadge) return;

            profile.Badge = newBadge;
            _uow.CaregiverProfiles.Update(profile);
            await _uow.SaveChangesAsync();
        }

        public async Task RecalculateAllAsync()
        {
            var profiles = await _uow.CaregiverProfiles.GetAllAsync();
            foreach (var profile in profiles)
            {
                var newBadge = CalculateBadge(profile.TotalCompletedBookings, profile.AverageRating);
                if (profile.Badge != newBadge)
                {
                    profile.Badge = newBadge;
                    _uow.CaregiverProfiles.Update(profile);
                }
            }
            await _uow.SaveChangesAsync();
        }

        private static BadgeLevel CalculateBadge(int completedBookings, decimal avgRating)
        {
            if (completedBookings >= 50 && avgRating >= 4.8m)
                return BadgeLevel.Champion;

            if (completedBookings >= 20 && avgRating >= 4.5m)
                return BadgeLevel.Elite;

            if (completedBookings >= 5)
                return BadgeLevel.Verified;

            return BadgeLevel.New;
        }
    }
}
