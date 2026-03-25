using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class CaregiverServiceConfiguration : IEntityTypeConfiguration<CaregiverService>
    {
        public void Configure(EntityTypeBuilder<CaregiverService> builder)
        {
            builder.ToTable("CaregiverServices");
            builder.HasKey(cs => cs.Id);

            // Optional per-service rate override (null = use caregiver default rate)
            builder.Property(cs => cs.CustomRate).HasPrecision(18, 2);
            builder.Property(cs => cs.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(cs => cs.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // A caregiver can only list a service category once
            builder.HasIndex(cs => new { cs.CaregiverProfileId, cs.ServiceCategoryId }).IsUnique();
            builder.HasIndex(cs => cs.ServiceCategoryId);
            //builder.HasIndex(cs => cs.IsDeleted);

            builder.HasOne(cs => cs.CaregiverProfile)
                .WithMany(c => c.CaregiverServices)
                .HasForeignKey(cs => cs.CaregiverProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cs => cs.ServiceCategory)
                .WithMany(s => s.CaregiverServices)
                .HasForeignKey(cs => cs.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}