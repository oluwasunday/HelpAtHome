using HelpAtHome.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Comment).HasMaxLength(2000);
            builder.Property(r => r.AdminNote).HasMaxLength(1000);

            // Rating must be 1–5
            builder.Property(r => r.Rating).IsRequired();
            builder.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5"));

            builder.Property(r => r.IsVisible).HasDefaultValue(true);
            builder.Property(r => r.IsFlagged).HasDefaultValue(false);
            builder.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(r => r.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(r => r.BookingId);
            builder.HasIndex(r => r.RevieweeUserId);
            // Prevent a user from reviewing the same booking twice in the same direction
            builder.HasIndex(r => new { r.BookingId, r.ReviewerUserId, r.IsByClient }).IsUnique();
            //builder.HasIndex(r => r.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            builder.HasOne(r => r.Booking)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Reviewee)
                .WithMany()
                .HasForeignKey(r => r.RevieweeUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}