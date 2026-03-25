using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    /*public class CaregiverProfileConfiguration : IEntityTypeConfiguration<CaregiverProfile>
    {
        public void Configure(EntityTypeBuilder<CaregiverProfile> builder)
        {
            builder.ToTable("CaregiverProfiles");
            builder.HasKey(c => c.Id);

            // ── Precision ────────────────────────────────────────────────
            builder.Property(c => c.HourlyRate).HasPrecision(18, 2);
            builder.Property(c => c.DailyRate).HasPrecision(18, 2);
            builder.Property(c => c.MonthlyRate).HasPrecision(18, 2);
            builder.Property(c => c.AverageRating).HasPrecision(3, 2);

            // ── String lengths ───────────────────────────────────────────
            builder.Property(c => c.Bio).HasMaxLength(2000);
            builder.Property(c => c.LanguagesSpoken).HasMaxLength(500);
            builder.Property(c => c.WorkingHours).HasMaxLength(1000);
            builder.Property(c => c.RejectionReason).HasMaxLength(1000);
            builder.Property(c => c.VerifiedByAdminId).HasMaxLength(100);

            // ── Enums stored as int ──────────────────────────────────────
            builder.Property(c => c.Badge).HasConversion<int>();
            builder.Property(c => c.GenderProvided).HasConversion<int>();
            builder.Property(c => c.VerificationStatus).HasConversion<int>();

            // ── Defaults ────────────────────────────────────────────────
            builder.Property(c => c.Badge).HasDefaultValue(BadgeLevel.New);
            builder.Property(c => c.IsAvailable).HasDefaultValue(true);
            builder.Property(c => c.IsBackgroundChecked).HasDefaultValue(false);
            builder.Property(c => c.TotalCompletedBookings).HasDefaultValue(0);
            builder.Property(c => c.TotalReviews).HasDefaultValue(0);
            builder.Property(c => c.AverageRating).HasDefaultValue(0m);
            builder.Property(c => c.VerificationStatus).HasDefaultValue(VerificationStatus.Pending);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(c => c.UserId).IsUnique();
            builder.HasIndex(c => c.AgencyId);
            builder.HasIndex(c => c.Badge);
            builder.HasIndex(c => c.VerificationStatus);
            builder.HasIndex(c => c.IsAvailable);
            builder.HasIndex(c => c.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            // User (AspNetUsers) — one-to-one
            builder.HasOne(c => c.User)
                .WithOne(u => u.CaregiverProfile)
                .HasForeignKey<CaregiverProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Agency — optional many-to-one
            builder.HasOne(c => c.Agency)
                .WithMany(a => a.Caregivers)
                .HasForeignKey(c => c.AgencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }*/

    public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
    {
        public void Configure(EntityTypeBuilder<ClientProfile> builder)
        {
            builder.ToTable("ClientProfiles");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.SpecialNotes).HasMaxLength(2000);
            builder.Property(c => c.MedicalConditions).HasMaxLength(2000);
            builder.Property(c => c.WalletBalance).HasPrecision(18, 2);
            builder.Property(c => c.CaregiverGenderPreference).HasConversion<int>();

            builder.Property(c => c.WalletBalance).HasDefaultValue(0m);
            builder.Property(c => c.RequireVerifiedOnly).HasDefaultValue(false);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.HasIndex(c => c.UserId).IsUnique();
            builder.HasIndex(c => c.IsDeleted);

            // User (AspNetUsers) — one-to-one
            builder.HasOne(c => c.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<ClientProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}