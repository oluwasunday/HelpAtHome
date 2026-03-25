using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder)
        {
            builder.ToTable("ServiceCategories");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).HasMaxLength(1000);
            builder.Property(s => s.IconUrl).HasMaxLength(500);

            builder.Property(s => s.IsActive).HasDefaultValue(true);
            builder.Property(s => s.SortOrder).HasDefaultValue(0);
            builder.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(s => s.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.HasIndex(s => s.Name).IsUnique();
            //builder.HasIndex(s => new { s.Name, s.CreatedAt }).IsUnique();
            //builder.HasIndex(s => s.IsActive);
            //builder.HasIndex(s => s.SortOrder);
            //builder.HasIndex(s => s.IsDeleted);
        }
    }

}