using SorocoSystemMonitor.Interfaces;
using SorocoSystemMonitor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SorocoSystemMonitor.Plugins;

public class FileLoggerPlugin : IMonitorPlugin
{
    private readonly ILogger<FileLoggerPlugin> _logger;
    private readonly MonitoringSettings _settings;
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();

    public string Name => "FileLogger";
    public bool IsEnabled { get; private set; }

    public FileLoggerPlugin(ILogger<FileLoggerPlugin> logger, IOptions<AppConfiguration> configuration)
    {
        _logger = logger;
        _settings = configuration.Value.MonitoringSettings;
        _logFilePath = _settings.LogFilePath;
        IsEnabled = _settings.EnableFileLogging;
    }

    public Task InitializeAsync()
    {
        try
        {
            if (!IsEnabled)
            {
                _logger.LogInformation("FileLogger plugin is disabled");
                return Task.CompletedTask;
            }

            var logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var header = $"=== System Monitor Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===";
            File.AppendAllText(_logFilePath, header + Environment.NewLine);

            _logger.LogInformation("FileLogger plugin initialized successfully. Log file: {LogFilePath}", _logFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FileLogger plugin");
            IsEnabled = false;
        }

        return Task.CompletedTask;
    }

    public Task ProcessMetricsAsync(SystemMetrics metrics)
    {
        if (!IsEnabled)
            return Task.CompletedTask;

        try
        {
            var logEntry = $"[{metrics.Timestamp:yyyy-MM-dd HH:mm:ss}] {metrics}";
            
            lock (_lockObject)
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }

            _logger.LogDebug("Metrics logged to file: {LogEntry}", logEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log metrics to file");
        }

        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        try
        {
            if (IsEnabled)
            {
                var footer = $"=== System Monitor Log Ended at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===";
                File.AppendAllText(_logFilePath, footer + Environment.NewLine);
                _logger.LogInformation("FileLogger plugin shutdown completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during FileLogger plugin shutdown");
        }

        return Task.CompletedTask;
    }
} 