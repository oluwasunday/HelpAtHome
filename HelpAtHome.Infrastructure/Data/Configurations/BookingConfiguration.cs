using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);
            builder.HasIndex(b => b.BookingReference).IsUnique();

            builder.Property(b => b.AgreedAmount).HasPrecision(18, 2);
            builder.Property(b => b.PlatformFee).HasPrecision(18, 2);
            builder.Property(b => b.CaregiverEarnings).HasPrecision(18, 2);
            builder.Property(b => b.ClientLatitude).HasPrecision(10, 7);
            builder.Property(b => b.ClientLongitude).HasPrecision(10, 7);
            builder.Property(b => b.ClientAddress).HasMaxLength(500);
            builder.Property(b => b.SpecialInstructions).HasMaxLength(2000);
            builder.Property(b => b.CancellationReason).HasMaxLength(1000);
            builder.Property(b => b.CancelledBy).HasMaxLength(100);

            // ── Indexes ──────────────────────────────────────────────────────
            // Composite: client history list ordered by date
            builder.HasIndex(b => new { b.ClientProfileId, b.Status, b.CreatedAt });
            // Composite: caregiver schedule list ordered by date
            builder.HasIndex(b => new { b.CaregiverProfileId, b.Status, b.CreatedAt });
            // Composite: conflict-detection query
            builder.HasIndex(b => new { b.CaregiverProfileId, b.Status, b.ScheduledStartDate });
            builder.HasIndex(b => b.IsDeleted);

            builder.HasOne(b => b.ClientProfile)
                .WithMany(c => c.Bookings).HasForeignKey(b => b.ClientProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(b => b.CaregiverProfile)
                .WithMany(c => c.Bookings).HasForeignKey(b => b.CaregiverProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
