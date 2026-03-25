using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title).IsRequired().HasMaxLength(300);
            builder.Property(n => n.Body).IsRequired().HasMaxLength(2000);
            builder.Property(n => n.Type).HasMaxLength(50);
            builder.Property(n => n.ReferenceId).HasMaxLength(100);

            builder.Property(n => n.IsRead).HasDefaultValue(false);
            builder.Property(n => n.IsSentPush).HasDefaultValue(false);
            builder.Property(n => n.IsSentEmail).HasDefaultValue(false);
            builder.Property(n => n.IsSentSms).HasDefaultValue(false);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(n => n.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.IsRead);
            // Composite: fetch unread notifications for a user quickly
            builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            //builder.HasIndex(n => n.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}