namespace SorocoSystemMonitor.Models;

public class AppConfiguration
{
    public MonitoringSettings MonitoringSettings { get; set; } = new();
    public ApiSettings ApiSettings { get; set; } = new();
    public PluginSettings Plugins { get; set; } = new();
}

public class MonitoringSettings
{
    public int IntervalSeconds { get; set; } = 2;
    public bool EnableConsoleOutput { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public string LogFilePath { get; set; } = "system_monitor.log";
    public int CpuSamplingIntervalMs { get; set; } = 100;
}

public class ApiSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableApiIntegration { get; set; } = true;
}

public class PluginSettings
{
    public FileLoggerSettings FileLogger { get; set; } = new();
    public ApiPosterSettings ApiPoster { get; set; } = new();
}

public class FileLoggerSettings
{
    public bool Enabled { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
}

public class ApiPosterSettings
{
    public bool Enabled { get; set; } = true;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
} 