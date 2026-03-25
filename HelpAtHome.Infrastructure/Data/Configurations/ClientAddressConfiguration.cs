using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class ClientAddressConfiguration : IEntityTypeConfiguration<ClientAddress>
    {
        public void Configure(EntityTypeBuilder<ClientAddress> b)
        {
            b.ToTable("ClientAddresses");
            b.HasKey(x => x.Id);

            b.Property(x => x.Line1)
             .IsRequired()
             .HasMaxLength(200);

            b.Property(x => x.Line2)
             .HasMaxLength(200);

            b.Property(x => x.Locality)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(x => x.City)
             .HasMaxLength(100);

            b.Property(x => x.LGA)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(x => x.State)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(x => x.Country)
             .IsRequired()
             .HasMaxLength(100)
             .HasDefaultValue("Nigeria");

            b.Property(x => x.PostalCode)
             .HasMaxLength(20);

            b.Property(x => x.Latitude)
             .HasPrecision(10, 7);

            b.Property(x => x.Longitude)
             .HasPrecision(10, 7);

            // ── FK relationship ──────────────────────────────────────────
            // One ClientProfile has at most ONE address
            b.HasOne(x => x.ClientProfile)
             .WithOne(cp => cp.Address)
             .HasForeignKey<ClientAddress>(x => x.ClientProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            // ── Indexes ──────────────────────────────────────────────────
            b.HasIndex(x => x.ClientProfileId)
             .IsUnique()    // enforces one-to-one at DB level
             .HasDatabaseName("IX_ClientAddresses_ClientProfileId");

            b.HasIndex(x => x.State)
             .HasDatabaseName("IX_ClientAddresses_State");

            // ── Audit fields ─────────────────────────────────────────────
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.Property(x => x.IsDeleted).HasDefaultValue(false);

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}