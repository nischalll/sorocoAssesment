using SorocoSystemMonitor.Models;

namespace SorocoSystemMonitor.Interfaces;

public interface IMonitorPlugin
{
    string Name { get; }
    bool IsEnabled { get; }
    Task ProcessMetricsAsync(SystemMetrics metrics);
    Task InitializeAsync();
    Task ShutdownAsync();
} 