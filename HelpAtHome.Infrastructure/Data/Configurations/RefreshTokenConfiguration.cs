using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
            builder.Property(r => r.RevokedReason).HasMaxLength(500);
            builder.Property(r => r.DeviceInfo).HasMaxLength(500);
            builder.Property(r => r.IpAddress).HasMaxLength(50);

            builder.Property(r => r.IsRevoked).HasDefaultValue(false);
            builder.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(r => r.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.HasIndex(r => r.Token).IsUnique();
            builder.HasIndex(r => r.UserId);
            // Index for cleanup job: delete expired tokens efficiently
            builder.HasIndex(r => new { r.UserId, r.IsRevoked, r.ExpiresAt });
            //builder.HasIndex(r => r.IsDeleted);

            builder.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}