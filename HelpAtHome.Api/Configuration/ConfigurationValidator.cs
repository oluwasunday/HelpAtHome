namespace HelpAtHome.Api.Configuration
{
    public static class ConfigurationValidator
    {
        // ── Keys that make the app non-functional if absent ──────────────────
        // These must be set before startup. If any are missing the app exits
        // immediately with a clear message listing every absent key.
        private static readonly (string Key, string Description)[] RequiredKeys =
        [
            // Database
            ("ConnectionStrings:DefaultConnection", "MySQL connection string"),
            ("MongoDb:ConnectionString",            "MongoDB connection string"),

            // Authentication
            ("Jwt:Key", "JWT signing key (min 32 chars)"),

            // Email — Brevo HTTP API
            ("Email:SenderEmail", "Sender e-mail address"),
            ("Email:ApiKey",      "Brevo HTTP API key"),

            // Email — Brevo SMTP relay (MailKit)
            ("EmailSettings:Mail",     "SMTP sender address"),
            ("EmailSettings:Login",    "SMTP login username"),
            ("EmailSettings:Password", "SMTP password"),
            ("EmailSettings:ApiKey",   "Brevo SMTP API key"),

            // Payment gateway
            ("Paystack:SecretKey",     "Paystack secret key"),
            ("Paystack:PublicKey",     "Paystack public key"),
            ("Paystack:WebhookSecret", "Paystack webhook secret"),

            // File / image storage
            ("Cloudinary:CloudName", "Cloudinary cloud name"),
            ("Cloudinary:ApiKey",    "Cloudinary API key"),
            ("Cloudinary:ApiSecret", "Cloudinary API secret"),
        ];

        /// <summary>
        /// Validates that all required configuration values are present.
        /// Throws <see cref="InvalidOperationException"/> listing every missing key
        /// along with the corresponding environment variable name to set.
        /// </summary>
        public static void Validate(IConfiguration config)
        {
            var keys = RequiredKeys.Select(e => e.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missing = RequiredKeys
                .Where(entry => string.IsNullOrWhiteSpace(config[entry.Key]))
                .ToList();

            if (missing.Count == 0) return;

            var lines = missing.Select(e =>
                $"  {e.Key,-45} ({e.Description})\n" +
                $"    → env var: {ToEnvVarName(e.Key)}");

            throw new InvalidOperationException(
                $"\n\n{'─',60}\n" +
                $"  MISSING REQUIRED CONFIGURATION ({missing.Count} value(s))\n" +
                $"{'─',60}\n\n" +
                string.Join("\n\n", lines) +
                $"\n\n{'─',60}\n" +
                $"  Copy HelpAtHome.Api/.env.example → HelpAtHome.Api/.env\n" +
                $"  and fill in the values above, then restart the application.\n" +
                $"{'─',60}\n");
        }

        // Converts "Section:SubSection:Key" → "Section__SubSection__Key"
        // which is the .NET environment variable naming convention.
        private static string ToEnvVarName(string configKey)
            => configKey.Replace(":", "__");
    }
}
