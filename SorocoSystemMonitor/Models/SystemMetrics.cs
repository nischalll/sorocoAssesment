namespace SorocoSystemMonitor.Models;

public class SystemMetrics
{
    public double CpuUsagePercent { get; set; }
    public long RamUsedBytes { get; set; }
    public long RamTotalBytes { get; set; }
    public long DiskUsedBytes { get; set; }
    public long DiskTotalBytes { get; set; }
    public DateTime Timestamp { get; set; }

    public SystemMetrics()
    {
        Timestamp = DateTime.UtcNow;
    }

    public double RamUsedMb => RamUsedBytes / (1024.0 * 1024.0);

    public double RamTotalMb => RamTotalBytes / (1024.0 * 1024.0);

    public double DiskUsedMb => DiskUsedBytes / (1024.0 * 1024.0);

    public double DiskTotalMb => DiskTotalBytes / (1024.0 * 1024.0);

    public double RamUsagePercent => RamTotalBytes > 0 ? (RamUsedBytes * 100.0) / RamTotalBytes : 0;

    public double DiskUsagePercent => DiskTotalBytes > 0 ? (DiskUsedBytes * 100.0) / DiskTotalBytes : 0;

    public object ToApiPayload()
    {
        return new
        {
            cpu = Math.Round(CpuUsagePercent, 2),
            ram_used = Math.Round(RamUsedMb, 2),
            ram_total = Math.Round(RamTotalMb, 2),
            ram_percent = Math.Round(RamUsagePercent, 2),
            disk_used = Math.Round(DiskUsedMb, 2),
            disk_total = Math.Round(DiskTotalMb, 2),
            disk_percent = Math.Round(DiskUsagePercent, 2),
            timestamp = Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    public override string ToString()
    {
        return $"CPU: {CpuUsagePercent:F1}% | RAM: {RamUsedMb:F1}MB/{RamTotalMb:F1}MB ({RamUsagePercent:F1}%) | Disk: {DiskUsedMb:F1}MB/{DiskTotalMb:F1}MB ({DiskUsagePercent:F1}%) | Time: {Timestamp:HH:mm:ss}";
    }
} 