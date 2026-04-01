using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Balance).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.TotalEarned).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.TotalSpent).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.TotalWithdrawn).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.LockReason).HasMaxLength(500);
            builder.Property(w => w.IsLocked).HasDefaultValue(false);
            builder.Property(w => w.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(w => w.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // One wallet per user — enforce uniqueness
            builder.HasIndex(w => w.UserId).IsUnique();

            // User (AspNetUsers) — one-to-one
            builder.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
