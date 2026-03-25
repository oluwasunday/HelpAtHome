using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class FamilyAccessConfiguration : IEntityTypeConfiguration<FamilyAccess>
    {
        public void Configure(EntityTypeBuilder<FamilyAccess> builder)
        {
            builder.ToTable("FamilyAccesses");
            builder.HasKey(f => f.Id);

            builder.Property(f => f.AccessLevel).HasConversion<int>().HasDefaultValue(AccessLevel.ViewOnly);
            builder.Property(f => f.IsApproved).HasDefaultValue(false);
            builder.Property(f => f.ReceiveEmergencyAlerts).HasDefaultValue(true);
            builder.Property(f => f.ReceiveBookingUpdates).HasDefaultValue(true);
            builder.Property(f => f.ReceivePaymentAlerts).HasDefaultValue(false);
            builder.Property(f => f.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(f => f.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            // One family member can only be granted access once per client
            builder.HasIndex(f => new { f.ClientUserId, f.FamilyMemberUserId }).IsUnique();
            builder.HasIndex(f => f.FamilyMemberUserId);
            //builder.HasIndex(f => f.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(f => f.Client)
                .WithMany(u => u.FamilyAccessGrants)
                .HasForeignKey(f => f.ClientUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.FamilyMember)
                .WithMany(u => u.FamilyAccessReceived)
                .HasForeignKey(f => f.FamilyMemberUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}