using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.ToTable("Agencies");
            builder.HasKey(a => a.Id);

            // ── Required string fields ───────────────────────────────────
            builder.Property(a => a.AgencyName).IsRequired().HasMaxLength(300);
            builder.Property(a => a.RegistrationNumber).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Email).IsRequired().HasMaxLength(200);
            builder.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(20);
            //builder.Property(a => a.AgencyAddress).IsRequired().HasMaxLength(500);

            // ── Optional string fields ───────────────────────────────────
            builder.Property(a => a.LogoUrl).HasMaxLength(500);
            builder.Property(a => a.Description).HasMaxLength(3000);
            builder.Property(a => a.Website).HasMaxLength(300);

            // ── Precision ────────────────────────────────────────────────
            builder.Property(a => a.CommissionRate).HasPrecision(5, 2);
            builder.Property(a => a.AgencyCommissionRate).HasPrecision(5, 2);
            builder.Property(a => a.WalletBalance).HasPrecision(18, 2);

            // ── Enum & defaults ──────────────────────────────────────────
            builder.Property(a => a.VerificationStatus).HasDefaultValue(VerificationStatus.Pending);
            builder.Property(a => a.IsActive).HasDefaultValue(true);
            builder.Property(a => a.CommissionRate).HasDefaultValue(15m);
            builder.Property(a => a.AgencyCommissionRate).HasDefaultValue(10m);
            builder.Property(a => a.WalletBalance).HasDefaultValue(0m);
            builder.Property(a => a.TotalCaregiversCount).HasDefaultValue(0);
            builder.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(a => a.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(a => a.RegistrationNumber).IsUnique();
            builder.HasIndex(a => a.Email).IsUnique();
            builder.HasIndex(a => a.AgencyAdminUserId).IsUnique();
            builder.HasIndex(a => new { a.AgencyAdminUserId, a.RegistrationNumber, a.Email }).IsUnique();
            //builder.HasIndex(a => a.VerificationStatus);
            //builder.HasIndex(a => a.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(a => a.AgencyAdmin)
                .WithMany()
                .HasForeignKey(a => a.AgencyAdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}