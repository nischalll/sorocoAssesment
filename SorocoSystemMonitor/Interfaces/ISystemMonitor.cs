using SorocoSystemMonitor.Models;

namespace SorocoSystemMonitor.Interfaces;

public interface ISystemMonitor
{
    Task<SystemMetrics> GetSystemMetricsAsync();
    Task<double> GetCpuUsageAsync();
    Task<(long Used, long Total)> GetMemoryUsageAsync();
    Task<(long Used, long Total)> GetDiskUsageAsync();
    bool IsSupported { get; }
} 