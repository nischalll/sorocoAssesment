using SorocoSystemMonitor.Interfaces;
using SorocoSystemMonitor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SorocoSystemMonitor.Services;

public class MonitoringService : BackgroundService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly ISystemMonitor _systemMonitor;
    private readonly IEnumerable<IMonitorPlugin> _plugins;
    private readonly MonitoringSettings _settings;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public MonitoringService(
        ILogger<MonitoringService> logger,
        ISystemMonitor systemMonitor,
        IEnumerable<IMonitorPlugin> plugins,
        IOptions<AppConfiguration> configuration)
    {
        _logger = logger;
        _systemMonitor = systemMonitor;
        _plugins = plugins;
        _settings = configuration.Value.MonitoringSettings;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting system monitoring service...");

            await InitializePluginsAsync();

            await StartMonitoringLoopAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in monitoring service execution");
            throw;
        }
    }

    private async Task InitializePluginsAsync()
    {
        _logger.LogInformation("Initializing plugins...");

        var enabledPlugins = _plugins.Where(p => p.IsEnabled).ToList();
        _logger.LogInformation("Found {PluginCount} enabled plugins", enabledPlugins.Count);

        foreach (var plugin in enabledPlugins)
        {
            try
            {
                await plugin.InitializeAsync();
                _logger.LogInformation("Plugin {PluginName} initialized successfully", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize plugin {PluginName}", plugin.Name);
            }
        }
    }

    private async Task StartMonitoringLoopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting monitoring loop with {Interval} second interval", _settings.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var metrics = await _systemMonitor.GetSystemMetricsAsync();

                if (_settings.EnableConsoleOutput)
                {
                    Console.WriteLine(metrics.ToString());
                    Console.WriteLine($"Debug - Raw CPU: {metrics.CpuUsagePercent:F2}%, RAM: {metrics.RamUsedBytes / (1024.0 * 1024.0):F1}MB / {metrics.RamTotalBytes / (1024.0 * 1024.0):F1}MB");
                }

                await ProcessMetricsThroughPluginsAsync(metrics);

                await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monitoring iteration");
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("Monitoring loop stopped");
    }

    private async Task ProcessMetricsThroughPluginsAsync(SystemMetrics metrics)
    {
        var enabledPlugins = _plugins.Where(p => p.IsEnabled).ToList();
        
        if (!enabledPlugins.Any())
            return;

        var tasks = enabledPlugins.Select(async plugin =>
        {
            try
            {
                await plugin.ProcessMetricsAsync(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin {PluginName} failed to process metrics", plugin.Name);
            }
        });

        await Task.WhenAll(tasks);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping monitoring service...");

        _cancellationTokenSource.Cancel();

        await ShutdownPluginsAsync();

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Monitoring service stopped");
    }

    private async Task ShutdownPluginsAsync()
    {
        _logger.LogInformation("Shutting down plugins...");

        var enabledPlugins = _plugins.Where(p => p.IsEnabled).ToList();
        var tasks = enabledPlugins.Select(async plugin =>
        {
            try
            {
                await plugin.ShutdownAsync();
                _logger.LogInformation("Plugin {PluginName} shutdown completed", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down plugin {PluginName}", plugin.Name);
            }
        });

        await Task.WhenAll(tasks);
    }

    public override void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        base.Dispose();
    }
} 