using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class CaregiverProfileConfiguration : IEntityTypeConfiguration<CaregiverProfile>
    {
        public void Configure(EntityTypeBuilder<CaregiverProfile> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.HourlyRate).HasPrecision(18, 2);
            builder.Property(c => c.DailyRate).HasPrecision(18, 2);
            builder.Property(c => c.WeeklyRate).HasPrecision(18, 2);
            builder.Property(c => c.MonthlyRate).HasPrecision(18, 2);
            builder.Property(c => c.AverageRating).HasPrecision(3, 2);

            // ── Indexes ──────────────────────────────────────────────────────
            // Agency caregiver lookup (e.g. agency admin viewing their team)
            builder.HasIndex(c => new { c.AgencyId, c.VerificationStatus });
            builder.HasIndex(c => c.IsDeleted);

            // UserId FK references AspNetUsers.Id (Guid)
            builder.HasOne(c => c.User)
                .WithOne(u => u.CaregiverProfile)
                .HasForeignKey<CaregiverProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}