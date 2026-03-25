using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            builder.ToTable("SupportTickets");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Subject).IsRequired().HasMaxLength(500);
            builder.Property(t => t.Description).IsRequired().HasMaxLength(5000);
            builder.Property(t => t.ResolutionNote).HasMaxLength(3000);
            builder.Property(t => t.AssignedToAdminId).HasMaxLength(100);

            builder.Property(t => t.Status).HasConversion<int>().HasDefaultValue(TicketStatus.Open);
            builder.Property(t => t.Priority).HasConversion<int>().HasDefaultValue(TicketPriority.Medium);
            builder.Property(t => t.IsDispute).HasDefaultValue(false);
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(t => t.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(t => t.TicketNumber).IsUnique();
            builder.HasIndex(t => t.RaisedByUserId);
            builder.HasIndex(t => t.BookingId);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.IsDispute);
            builder.HasIndex(t => t.Priority);
            builder.HasIndex(t => t.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(t => t.RaisedBy)
                .WithMany(u => u.SupportTickets)
                .HasForeignKey(t => t.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Booking)
                .WithMany(b => b.Disputes)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }

}