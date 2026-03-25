using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class AgencyAddressConfiguration : IEntityTypeConfiguration<AgencyAddress>
    {
        public void Configure(EntityTypeBuilder<AgencyAddress> b)
        {
            b.ToTable("AgencyAddresses");
            b.HasKey(x => x.Id);

            b.Property(x => x.Line1).IsRequired().HasMaxLength(200);
            b.Property(x => x.Line2).HasMaxLength(200);
            b.Property(x => x.Locality).IsRequired().HasMaxLength(100);
            b.Property(x => x.City).IsRequired().HasMaxLength(100);
            b.Property(x => x.LGA).IsRequired().HasMaxLength(100);
            b.Property(x => x.State).IsRequired().HasMaxLength(100);
            b.Property(x => x.Country)
             .IsRequired().HasMaxLength(100).HasDefaultValue("Nigeria");
            b.Property(x => x.PostalCode).HasMaxLength(20);

            // ── FK ───────────────────────────────────────────────────────
            b.HasOne(x => x.Agency)
             .WithOne(a => a.AgencyAddress)
             .HasForeignKey<AgencyAddress>(x => x.AgencyId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.AgencyId)
             .IsUnique()
             .HasDatabaseName("IX_AgencyAddresses_AgencyId");

            b.HasIndex(x => x.State)
             .HasDatabaseName("IX_AgencyAddresses_State");

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}