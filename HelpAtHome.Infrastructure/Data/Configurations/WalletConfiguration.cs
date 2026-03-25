using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    /*public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");
            builder.HasKey(b => b.Id);

            // ── Required fields ──────────────────────────────────────────
            builder.Property(b => b.BookingReference).IsRequired().HasMaxLength(50);
            builder.Property(b => b.SpecialInstructions).HasMaxLength(2000);
            builder.Property(b => b.CancellationReason).HasMaxLength(1000);
            builder.Property(b => b.CancelledBy).HasMaxLength(100);
            builder.Property(b => b.ClientAddress).HasMaxLength(500);

            // ── Precision ────────────────────────────────────────────────
            builder.Property(b => b.AgreedAmount).HasPrecision(18, 2);
            builder.Property(b => b.PlatformFee).HasPrecision(18, 2);
            builder.Property(b => b.CaregiverEarnings).HasPrecision(18, 2);
            builder.Property(b => b.ClientLatitude).HasPrecision(10, 7);
            builder.Property(b => b.ClientLongitude).HasPrecision(10, 7);

            // ── Enums ────────────────────────────────────────────────────
            builder.Property(b => b.Status).HasConversion<int>().HasDefaultValue(BookingStatus.Pending);
            builder.Property(b => b.Frequency).HasConversion<int>();
            builder.Property(b => b.PaymentStatus).HasConversion<int>().HasDefaultValue(PaymentStatus.Pending);

            // ── Defaults ────────────────────────────────────────────────
            builder.Property(b => b.IsReviewedByClient).HasDefaultValue(false);
            builder.Property(b => b.IsReviewedByCaregiver).HasDefaultValue(false);
            builder.Property(b => b.HasDispute).HasDefaultValue(false);
            builder.Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(b => b.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(b => b.BookingReference).IsUnique();
            builder.HasIndex(b => b.ClientProfileId);
            builder.HasIndex(b => b.CaregiverProfileId);
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.ScheduledStartDate);
            builder.HasIndex(b => b.IsDeleted);
            // Composite index for conflict detection query
            builder.HasIndex(b => new { b.CaregiverProfileId, b.Status, b.ScheduledStartDate });

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(b => b.ClientProfile)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.CaregiverProfile)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CaregiverProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ServiceCategory)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Transaction)
                .WithOne(t => t.Booking)
                .HasForeignKey<Transaction>(t => t.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }*/
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Balance).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.TotalEarned).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.TotalSpent).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.TotalWithdrawn).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.LockReason).HasMaxLength(500);
            builder.Property(w => w.IsLocked).HasDefaultValue(false);
            builder.Property(w => w.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(w => w.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // One wallet per user — enforce uniqueness
            builder.HasIndex(w => w.UserId).IsUnique();

            // User (AspNetUsers) — one-to-one
            builder.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}