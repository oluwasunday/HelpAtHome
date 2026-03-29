namespace HelpAtHome.Api.Configuration
{
    /// <summary>
    /// Loads KEY=VALUE pairs from a .env file into process environment variables.
    /// Call this before WebApplication.CreateBuilder() so the .NET configuration
    /// system picks them up through its built-in AddEnvironmentVariables() source.
    ///
    /// Priority order (highest → lowest):
    ///   1. OS / shell environment variables  (production, Docker, CI)
    ///   2. .env file                          (local development)
    ///   3. appsettings.json                   (defaults and non-sensitive values)
    /// </summary>
    public static class EnvironmentLoader
    {
        public static void Load(string? path = null)
        {
            path ??= Path.Combine(Directory.GetCurrentDirectory(), ".env");

            if (!File.Exists(path)) return;

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();

                // Skip blank lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0) continue;

                var key   = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                // Strip surrounding single or double quotes
                if (value.Length >= 2 &&
                    ((value.StartsWith('"')  && value.EndsWith('"')) ||
                     (value.StartsWith('\'') && value.EndsWith('\''))))
                {
                    value = value[1..^1];
                }

                // Never overwrite a variable already set by the OS or shell —
                // this guarantees production env vars always take precedence.
                if (Environment.GetEnvironmentVariable(key) is null)
                    Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
