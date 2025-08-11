using SorocoSystemMonitor.Services;
using SorocoSystemMonitor.Models;

class TestMonitor
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Testing System Monitor...");
            Console.WriteLine("Press Ctrl+C to stop");
            Console.WriteLine();

            var monitor = new WindowsSystemMonitor();
            
            if (!monitor.IsSupported)
            {
                Console.WriteLine("ERROR: System monitor not supported!");
                return;
            }

            if (!monitor.ValidateCounters())
            {
                Console.WriteLine("ERROR: Performance counters not working!");
                return;
            }

            Console.WriteLine("System monitor initialized successfully!");
            Console.WriteLine("Starting monitoring...");
            Console.WriteLine();

            var lastCpu = 0.0;
            var lastRam = 0.0;

            while (true)
            {
                try
                {
                    var metrics = await monitor.GetSystemMetricsAsync();
                    
                    Console.Clear();
                    Console.WriteLine($"=== System Monitor Test ===");
                    Console.WriteLine($"Time: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine();
                    Console.WriteLine($"CPU Usage: {metrics.CpuUsagePercent:F1}%");
                    Console.WriteLine($"RAM Usage: {metrics.RamUsedBytes / (1024.0 * 1024.0):F1}MB / {metrics.RamTotalBytes / (1024.0 * 1024.0):F1}MB ({metrics.RamUsagePercent:F1}%)");
                    Console.WriteLine($"Disk Usage: {metrics.DiskUsedBytes / (1024.0 * 1024.0):F1}MB / {metrics.DiskTotalBytes / (1024.0 * 1024.0):F1}MB ({metrics.DiskUsagePercent:F1}%)");
                    Console.WriteLine();
                    
                    if (lastCpu > 0)
                    {
                        var cpuDiff = metrics.CpuUsagePercent - lastCpu;
                        var ramDiff = metrics.RamUsagePercent - lastRam;
                        Console.WriteLine($"CPU Change: {cpuDiff:F1}%");
                        Console.WriteLine($"RAM Change: {ramDiff:F1}%");
                    }
                    
                    lastCpu = metrics.CpuUsagePercent;
                    lastRam = metrics.RamUsagePercent;
                    
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await Task.Delay(5000);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
} 