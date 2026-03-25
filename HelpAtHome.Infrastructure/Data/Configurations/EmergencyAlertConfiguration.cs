using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class EmergencyAlertConfiguration : IEntityTypeConfiguration<EmergencyAlert>
    {
        public void Configure(EntityTypeBuilder<EmergencyAlert> builder)
        {
            builder.ToTable("EmergencyAlerts");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.LocationAddress).HasMaxLength(500);
            builder.Property(e => e.Message).HasMaxLength(1000);
            builder.Property(e => e.ResolutionNote).HasMaxLength(2000);
            builder.Property(e => e.Latitude).HasPrecision(10, 7);
            builder.Property(e => e.Longitude).HasPrecision(10, 7);

            builder.Property(e => e.Status).HasConversion<int>().HasDefaultValue(AlertStatus.Active);
            builder.Property(e => e.NotifiedFamily).HasDefaultValue(false);
            builder.Property(e => e.NotifiedAdmin).HasDefaultValue(false);
            builder.Property(e => e.NotifiedCaregiver).HasDefaultValue(false);
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.HasIndex(e => e.ClientProfileId);
            builder.HasIndex(e => e.Status);
            // Quickly find active alerts for operations team
            builder.HasIndex(e => new { e.Status, e.CreatedAt });
            //builder.HasIndex(e => e.IsDeleted);

            builder.HasOne(e => e.ClientProfile)
                .WithMany(c => c.EmergencyAlerts)
                .HasForeignKey(e => e.ClientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ActiveBooking)
                .WithMany()
                .HasForeignKey(e => e.ActiveBookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }

}