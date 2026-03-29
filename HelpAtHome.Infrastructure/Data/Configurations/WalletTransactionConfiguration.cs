using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.BalanceBefore).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.BalanceAfter).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.WalletId).HasMaxLength(36);
            builder.Property(w => w.Amount).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(w => w.Reference).HasMaxLength(100);
            builder.Property(w => w.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(w => w.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // Oenforce uniqueness
            builder.HasIndex(w => w.Reference).IsUnique();
        }
    }

}