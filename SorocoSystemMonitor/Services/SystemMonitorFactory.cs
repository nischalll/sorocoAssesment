using SorocoSystemMonitor.Interfaces;
using System.Runtime.InteropServices;

namespace SorocoSystemMonitor.Services;

public static class SystemMonitorFactory
{
    public static ISystemMonitor CreateSystemMonitor()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsSystemMonitor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new NotImplementedException("Linux system monitoring not yet implemented");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new NotImplementedException("macOS system monitoring not yet implemented");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system");
        }
    }
} 