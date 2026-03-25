using HelpAtHome.Core.Entities;
using HelpAtHome.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpAtHome.Infrastructure.Data.Configurations
{
    public class VerificationDocumentConfiguration : IEntityTypeConfiguration<VerificationDocument>
    {
        public void Configure(EntityTypeBuilder<VerificationDocument> builder)
        {
            builder.ToTable("VerificationDocuments");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.DocumentUrl).IsRequired().HasMaxLength(500);
            builder.Property(v => v.DocumentNumber).HasMaxLength(100);
            builder.Property(v => v.ReviewNote).HasMaxLength(1000);
            builder.Property(v => v.ReviewedByAdminId).HasMaxLength(100);

            builder.Property(v => v.DocumentType).HasConversion<int>();
            builder.Property(v => v.Status).HasConversion<int>().HasDefaultValue(VerificationStatus.Pending);
            builder.Property(v => v.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(v => v.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ── Indexes ──────────────────────────────────────────────────
            builder.HasIndex(v => v.CaregiverProfileId);
            builder.HasIndex(v => v.AgencyId);
            //builder.HasIndex(v => v.Status);
            //builder.HasIndex(v => v.DocumentType);
            // Composite for admin review queue
            builder.HasIndex(v => new { v.Status, v.DocumentType, v.CreatedAt });
            //builder.HasIndex(v => v.IsDeleted);

            // ── Relationships ────────────────────────────────────────────
            // A document belongs to either a CaregiverProfile OR an Agency, not both.
            builder.HasOne(v => v.CaregiverProfile)
                .WithMany(c => c.Documents)
                .HasForeignKey(v => v.CaregiverProfileId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(v => v.Agency)
                .WithMany(a => a.Documents)
                .HasForeignKey(v => v.AgencyId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        }
    }

}