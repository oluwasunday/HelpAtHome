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
            builder.HasOne(b => b.ClientProfile)
                .WithMany(c => c.Bookings).HasForeignKey(b => b.ClientProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(b => b.CaregiverProfile)
                .WithMany(c => c.Bookings).HasForeignKey(b => b.CaregiverProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
