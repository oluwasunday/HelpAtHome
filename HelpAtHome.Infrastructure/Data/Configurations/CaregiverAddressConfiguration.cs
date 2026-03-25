using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class CaregiverAddressConfiguration : IEntityTypeConfiguration<CaregiverAddress>
    {
        public void Configure(EntityTypeBuilder<CaregiverAddress> b)
        {
            b.ToTable("CaregiverAddresses");
            b.HasKey(x => x.Id);

            b.Property(x => x.Line1).IsRequired().HasMaxLength(200);
            b.Property(x => x.Line2).HasMaxLength(200);
            b.Property(x => x.Locality).IsRequired().HasMaxLength(100);
            b.Property(x => x.City).HasMaxLength(100);
            b.Property(x => x.LGA).IsRequired().HasMaxLength(100);
            b.Property(x => x.State).IsRequired().HasMaxLength(100);
            b.Property(x => x.Country)
             .IsRequired().HasMaxLength(100).HasDefaultValue("Nigeria");
            b.Property(x => x.PostalCode).HasMaxLength(20);

            // ── FK to CaregiverProfile (required — one-to-one) ───────────
            b.HasOne(x => x.CaregiverProfile)
             .WithOne(cp => cp.Address)
             .HasForeignKey<CaregiverAddress>(x => x.CaregiverProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            // ── FK to Agency (optional) ──────────────────────────────────
            // AgencyId = Guid.Empty means individual caregiver
            b.HasOne(x => x.Agency)
             .WithMany()
             .HasForeignKey(x => x.AgencyId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.CaregiverProfileId)
             .IsUnique()
             .HasDatabaseName("IX_CaregiverAddresses_CaregiverProfileId");

            b.HasIndex(new[] { "State", "City", "LGA" })
             .HasDatabaseName("IX_CaregiverAddresses_Location");

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}