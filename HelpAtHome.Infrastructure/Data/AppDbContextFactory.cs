using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HelpAtHome.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory used exclusively by EF Core tooling (dotnet ef migrations add, etc.).
    /// Bypasses the full app startup so that the ConfigurationValidator does not block migrations.
    /// Never called at runtime.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Walk up from Infrastructure project to the solution root so we can load
            // the Api's .env file (which holds the real connection string locally).
            var basePath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "HelpAtHome.Api"));

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddIniFile(".env", optional: true)   // local secrets — gitignored
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection not found. " +
                    "Copy HelpAtHome.Api/.env.example → HelpAtHome.Api/.env and fill in the value.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
