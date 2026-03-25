using HelpAtHome.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HelpAtHome.Infrastructure.Data
{
    /// <summary>
    /// Inherits IdentityDbContext so Identity tables (AspNetUsers, AspNetRoles,
    /// AspNetUserRoles, etc.) are managed in the same MySQL database alongside
    /// our custom domain tables.
    /// Generic params: <User, IdentityRole<Guid>, Guid> — user type, role type, PK type
    /// </summary>
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── Custom domain DbSets (Identity adds AspNetUsers etc. automatically) ──
        public DbSet<CaregiverProfile> CaregiverProfiles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketMessage> TicketMessages { get; set; }
        public DbSet<EmergencyAlert> EmergencyAlerts { get; set; }
        public DbSet<FamilyAccess> FamilyAccesses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<VerificationDocument> VerificationDocuments { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<CaregiverService> CaregiverServices { get; set; }
        public DbSet<ClientAddress> ClientAddresses { get; set; }
        public DbSet<AgencyAddress> AgencyAddresses { get; set; }
        public DbSet<CaregiverAddress> CaregiverAddresses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRITICAL: must call base first so Identity configures its own tables
            base.OnModelCreating(modelBuilder);

            // Apply all custom entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // ── Custom User column additions on top of IdentityUser ────────
            modelBuilder.Entity<User>(b => {
                b.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                b.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                b.Property(u => u.Role).HasConversion<int>();
                b.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                b.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                b.HasIndex(u => u.IsDeleted);
                b.HasQueryFilter(u => !u.IsDeleted);

                // One-to-one relationships
                b.HasOne(u => u.CaregiverProfile)
                    .WithOne(c => c.User).HasForeignKey<CaregiverProfile>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(u => u.ClientProfile)
                    .WithOne(c => c.User).HasForeignKey<ClientProfile>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Soft-delete query filters ──────────────────────────────────
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<CaregiverProfile>().HasQueryFilter(c => !c.IsDeleted);
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            // Auto-update UpdatedAt on BaseEntity children
            foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Modified))
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }   

            // Auto-update UpdatedAt on User (which does not extend BaseEntity)
            foreach (var entry in ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Modified))
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(ct);
        }
    }
}
