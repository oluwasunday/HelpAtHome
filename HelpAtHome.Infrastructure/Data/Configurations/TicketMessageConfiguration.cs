using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
    {
        public void Configure(EntityTypeBuilder<TicketMessage> builder)
        {
            builder.ToTable("TicketMessages");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Message).IsRequired().HasMaxLength(5000);
            builder.Property(m => m.AttachmentUrl).HasMaxLength(500);
            builder.Property(m => m.IsInternal).HasDefaultValue(false);
            builder.Property(m => m.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(m => m.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.HasIndex(m => m.TicketId);
            builder.HasIndex(m => m.SenderUserId);
            //builder.HasIndex(m => m.IsDeleted);

            builder.HasOne(m => m.Ticket)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}