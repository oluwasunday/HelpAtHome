using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.ToTable("OtpCodes");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Code).IsRequired().HasMaxLength(10);
            builder.Property(o => o.Purpose).IsRequired().HasMaxLength(50);
            // Purpose values: "EmailVerify" | "PhoneVerify" | "PasswordReset" | "Login2FA"

            builder.Property(o => o.IsUsed).HasDefaultValue(false);
            builder.Property(o => o.Attempts).HasDefaultValue(0);
            builder.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(o => o.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.HasIndex(o => o.UserId);
            // Lookup index: find latest unused OTP for a user/purpose pair
            builder.HasIndex(o => new { o.UserId, o.Purpose, o.IsUsed, o.ExpiresAt });
            //builder.HasIndex(o => o.IsDeleted);

            builder.HasOne(o => o.User)
                .WithMany(u => u.OtpCodes)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}