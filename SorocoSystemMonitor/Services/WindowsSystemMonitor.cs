using SorocoSystemMonitor.Interfaces;
using SorocoSystemMonitor.Models;
using System.Diagnostics;

namespace SorocoSystemMonitor.Services;

public class WindowsSystemMonitor : ISystemMonitor, IDisposable
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _ramCounter;
    private readonly bool _isDisposed = false;
    private bool _isInitialized = false;

    public bool IsSupported => _isInitialized;

    public bool ValidateCounters()
    {
        try
        {
            var testCpu = _cpuCounter.NextValue();
            var testRam = _ramCounter.NextValue();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public WindowsSystemMonitor()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            _cpuCounter.NextValue();
            _ramCounter.NextValue();
            
            Thread.Sleep(1000);
            
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _isInitialized = false;
            throw new InvalidOperationException("Failed to initialize Windows performance counters. Ensure running with appropriate permissions.", ex);
        }
    }

    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("System monitor not properly initialized");

        var metrics = new SystemMetrics();
        
        metrics.CpuUsagePercent = await GetCpuUsageAsync();
        var (ramUsed, ramTotal) = await GetMemoryUsageAsync();
        var (diskUsed, diskTotal) = await GetDiskUsageAsync();
        
        metrics.RamUsedBytes = ramUsed;
        metrics.RamTotalBytes = ramTotal;
        metrics.DiskUsedBytes = diskUsed;
        metrics.DiskTotalBytes = diskTotal;
        
        return metrics;
    }

    public Task<double> GetCpuUsageAsync()
    {
        try
        {
            var cpuUsage = _cpuCounter.NextValue();
            return Task.FromResult(Math.Round(Math.Max(0, Math.Min(100, cpuUsage)), 2));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read CPU usage", ex);
        }
    }

    public Task<(long Used, long Total)> GetMemoryUsageAsync()
    {
        try
        {
            var availableMB = _ramCounter.NextValue();
            var totalMemory = GetTotalPhysicalMemory();
            var availableBytes = (long)(availableMB * 1024 * 1024);
            var usedBytes = totalMemory - availableBytes;
            
            return Task.FromResult((usedBytes, totalMemory));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read memory usage", ex);
        }
    }

    private long GetTotalPhysicalMemory()
    {
        try
        {
            var searcher = new System.Management.ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (System.Management.ManagementObject obj in searcher.Get())
            {
                return Convert.ToInt64(obj["TotalPhysicalMemory"]);
            }
        }
        catch
        {
            try
            {
                var searcher2 = new System.Management.ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
                long totalMemory = 0;
                foreach (System.Management.ManagementObject obj in searcher2.Get())
                {
                    totalMemory += Convert.ToInt64(obj["Capacity"]);
                }
                if (totalMemory > 0)
                    return totalMemory;
            }
            catch
            {
                // Fallback to default value
            }
        }
        return 16L * 1024 * 1024 * 1024;
    }

    public Task<(long Used, long Total)> GetDiskUsageAsync()
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\");
            var totalSize = driveInfo.TotalSize;
            var availableSpace = driveInfo.AvailableFreeSpace;
            var usedSpace = totalSize - availableSpace;
            
            return Task.FromResult((usedSpace, totalSize));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read disk usage", ex);
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
        }
    }
} 