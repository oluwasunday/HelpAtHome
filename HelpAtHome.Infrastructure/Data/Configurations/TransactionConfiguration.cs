using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);

            // ── String constraints ───────────────────────────────────────
            builder.Property(t => t.TransactionReference).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Description).HasMaxLength(500);
            builder.Property(t => t.PaystackReference).HasMaxLength(200);
            builder.Property(t => t.PaystackTransactionId).HasMaxLength(200);
            builder.Property(t => t.FailureReason).HasMaxLength(1000);
            builder.Property(t => t.BankName).HasMaxLength(200);
            builder.Property(t => t.AccountNumber).HasMaxLength(20);
            builder.Property(t => t.AccountName).HasMaxLength(200);

            // ── Precision ────────────────────────────────────────────────
            builder.Property(t => t.Amount).HasPrecision(18, 2);
            builder.Property(t => t.BalanceBefore).HasPrecision(18, 2);
            builder.Property(t => t.BalanceAfter).HasPrecision(18, 2);

            // ── Enums ────────────────────────────────────────────────────
            builder.Property(t => t.Type).HasConversion<int>();
            builder.Property(t => t.Status).HasConversion<int>().HasDefaultValue(TransactionStatus.Pending);

            builder.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(t => t.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(t => t.TransactionReference).IsUnique();
            builder.HasIndex(t => t.WalletId);
            builder.HasIndex(t => t.BookingId);
            builder.HasIndex(t => t.PaystackReference); // for idempotency check
            // Composite index on WalletId and TransactionDate
            builder.HasIndex(t => new { t.BookingId, t.CreatedAt });
            //builder.HasIndex(t => t.Status);
            //builder.HasIndex(t => t.CreatedAt);
            //builder.HasIndex(t => t.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // BookingId is optional (deposits/withdrawals have no booking)
            builder.HasOne(t => t.Booking)
                .WithOne(b => b.Transaction)
                .HasForeignKey<Transaction>(t => t.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }

}