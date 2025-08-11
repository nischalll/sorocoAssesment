using SorocoSystemMonitor.Services;

class TestCpu
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Testing CPU Monitor...");
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
            Console.WriteLine("Starting CPU monitoring...");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    var cpuUsage = await monitor.GetCpuUsageAsync();
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    
                    Console.WriteLine($"[{timestamp}] CPU Usage: {cpuUsage:F1}%");
                    
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