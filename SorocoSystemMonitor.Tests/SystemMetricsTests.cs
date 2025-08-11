using Xunit;
using SorocoSystemMonitor.Models;

namespace SorocoSystemMonitor.Tests;

public class SystemMetricsTests
{
    [Fact]
    public void SystemMetrics_Constructor_SetsTimestamp()
    {
        var metrics = new SystemMetrics();
        
        Assert.True(metrics.Timestamp > DateTime.UtcNow.AddSeconds(-1));
        Assert.True(metrics.Timestamp <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void SystemMetrics_Properties_SetAndGetCorrectly()
    {
        var metrics = new SystemMetrics
        {
            CpuUsagePercent = 25.5,
            RamUsedBytes = 8 * 1024 * 1024,
            RamTotalBytes = 16 * 1024 * 1024,
            DiskUsedBytes = 100 * 1024 * 1024,
            DiskTotalBytes = 200 * 1024 * 1024
        };

        Assert.Equal(25.5, metrics.CpuUsagePercent);
        Assert.Equal(8.0, metrics.RamUsedMb);
        Assert.Equal(16.0, metrics.RamTotalMb);
        Assert.Equal(100.0, metrics.DiskUsedMb);
        Assert.Equal(200.0, metrics.DiskTotalMb);
    }

    [Fact]
    public void SystemMetrics_RamUsagePercent_CalculatesCorrectly()
    {
        var metrics = new SystemMetrics
        {
            RamUsedBytes = 8 * 1024 * 1024,
            RamTotalBytes = 16 * 1024 * 1024
        };

        Assert.Equal(50.0, metrics.RamUsagePercent);
    }

    [Fact]
    public void SystemMetrics_DiskUsagePercent_CalculatesCorrectly()
    {
        var metrics = new SystemMetrics
        {
            DiskUsedBytes = 75 * 1024 * 1024,
            DiskTotalBytes = 100 * 1024 * 1024
        };

        Assert.Equal(75.0, metrics.DiskUsagePercent);
    }

    [Fact]
    public void SystemMetrics_ToApiPayload_ReturnsCorrectFormat()
    {
        var metrics = new SystemMetrics
        {
            CpuUsagePercent = 30.0,
            RamUsedBytes = 8 * 1024 * 1024,
            RamTotalBytes = 16 * 1024 * 1024,
            DiskUsedBytes = 100 * 1024 * 1024,
            DiskTotalBytes = 200 * 1024 * 1024
        };

        var payload = metrics.ToApiPayload();
        var payloadType = payload.GetType();

        Assert.NotNull(payload);
        
        var properties = payloadType.GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();
        
        Assert.Contains("cpu", propertyNames);
        Assert.Contains("ram_used", propertyNames);
        Assert.Contains("ram_total", propertyNames);
        Assert.Contains("ram_percent", propertyNames);
        Assert.Contains("disk_used", propertyNames);
        Assert.Contains("disk_total", propertyNames);
        Assert.Contains("disk_percent", propertyNames);
        Assert.Contains("timestamp", propertyNames);
    }

    [Fact]
    public void SystemMetrics_ToString_ReturnsReadableFormat()
    {
        var metrics = new SystemMetrics
        {
            CpuUsagePercent = 25.5,
            RamUsedBytes = 8 * 1024 * 1024,
            RamTotalBytes = 16 * 1024 * 1024,
            DiskUsedBytes = 100 * 1024 * 1024,
            DiskTotalBytes = 200 * 1024 * 1024
        };

        var result = metrics.ToString();

        Assert.Contains("CPU: 25.5%", result);
        Assert.Contains("RAM: 8.0MB/16.0MB (50.0%)", result);
        Assert.Contains("Disk: 100.0MB/200.0MB (50.0%)", result);
    }

    [Fact]
    public void SystemMetrics_ZeroTotalBytes_HandlesGracefully()
    {
        var metrics = new SystemMetrics
        {
            RamTotalBytes = 0,
            DiskTotalBytes = 0
        };

        Assert.Equal(0.0, metrics.RamUsagePercent);
        Assert.Equal(0.0, metrics.DiskUsagePercent);
    }
} 