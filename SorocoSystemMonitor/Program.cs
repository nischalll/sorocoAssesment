using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SorocoSystemMonitor.Interfaces;
using SorocoSystemMonitor.Services;
using SorocoSystemMonitor.Plugins;
using SorocoSystemMonitor.Models;

namespace SorocoSystemMonitor;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Soroco System Monitor ===");
            Console.WriteLine("Starting application...");
            Console.WriteLine();

            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppConfiguration>(context.Configuration);
                
                services.AddSingleton<ISystemMonitor>(provider =>
                {
                    try
                    {
                        return SystemMonitorFactory.CreateSystemMonitor();
                    }
                    catch (Exception ex)
                    {
                        var logger = provider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Failed to create system monitor for current platform");
                        throw;
                    }
                });

                services.AddHttpClient<ApiPosterPlugin>();

                services.AddSingleton<IMonitorPlugin, FileLoggerPlugin>();
                services.AddSingleton<IMonitorPlugin, ApiPosterPlugin>();

                services.AddHostedService<MonitoringService>();
            });
} 