using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Templates
{
    public class AppSettingsTemplate
    {
        public DatabaseConfig Database { get; set; } = new();
        public LoggingConfig Logging { get; set; } = new();
        public JwtConfig Jwt { get; set; } = new();
        public List<ExternalServiceConfig> ExternalServices { get; set; } = new();
    }

    public class DatabaseConfig
    {
        public string ConnectionString { get; set; } = "Server=localhost;Database={DatabaseName};Trusted_Connection=True;";
        public string Provider { get; set; } = "SqlServer";
        public bool UseInMemory { get; set; } = false;
    }

    public class LoggingConfig
    {
        public string Level { get; set; } = "Information";
        public bool IncludeScopes { get; set; } = true;
        public List<LogOutput> Outputs { get; set; } = new()
        {
            new LogOutput { Type = "Console" },
            new LogOutput { Type = "File", Path = "logs/app.log" }
        };
    }

    public class LogOutput
    {
        public string Type { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class JwtConfig
    {
        public string Secret { get; set; } = "your-secret-key-here";
        public string Issuer { get; set; } = "your-issuer";
        public string Audience { get; set; } = "your-audience";
        public int ExpiryInMinutes { get; set; } = 60;
    }

    public class ExternalServiceConfig
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutInSeconds { get; set; } = 30;
    }
}
