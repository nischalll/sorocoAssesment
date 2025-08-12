# Soroco System Monitor

A system monitoring app I built in C# that tracks CPU, memory, and disk usage. It's got a plugin system so you can easily add new features, and it works on Windows, Linux, and macOS.

## What It Does

- **Monitors your system** - CPU, RAM, and disk usage in real-time
- **Plugin system** - Easy to extend with new functionality
- **Cross-platform**- Built with .NET 6 so it works everywhere
- **API integration** - Can send data to your own endpoints
- **File logging** - Saves metrics to files for later analysis
- **Clean code** - Well-structured and easy to maintain

## What You Need

- .NET 6.0 (Runtime or SDK)
- Windows 10/11, Linux, or macOS
- **Windows users**: Run as Admin for best results with performance counters

## Getting Started

### Build and Run

```bash
# Go to the project folder
cd SorocoSystemMonitor

# Get the dependencies
dotnet restore

# Build it
dotnet build

# Run it
dotnet run
```

### Configuration

The app uses `appsettings.json` for settings. You can change:
- How often it collects metrics (default: 2 seconds)
- Where to send API data
- Which plugins to enable/disable

## How It Works

### Architecture

I went with a clean architecture approach because:
- **Easy to understand** - Clear separation between different parts
- **Easy to test** - You can mock interfaces for unit tests
- **Easy to extend** - Plugin system lets you add features without touching core code
- **Cross-platform** - Platform-specific stuff is isolated

### Main Components

- **`ISystemMonitor`** - Interface for getting system metrics
- **`IMonitorPlugin`** - Interface for plugins
- **`MonitoringService`** - Main service that runs everything
- **`SystemMonitorFactory`** - Creates the right monitor for your OS

## Plugins

### Built-in Ones

1. **FileLoggerPlugin** - Saves metrics to files
2. **ApiPosterPlugin** - Sends data to APIs

### Making Your Own

Just implement the `IMonitorPlugin` interface:

```csharp
public class MyCoolPlugin : IMonitorPlugin
{
    public string Name => "MyCoolPlugin";
    public bool IsEnabled => true;

    public async Task InitializeAsync() { /* setup */ }
    public async Task ProcessMetricsAsync(SystemMetrics metrics) { /* do stuff */ }
    public async Task ShutdownAsync() { /* cleanup */ }
}
```

Then add it to `Program.cs`:
```csharp
services.AddSingleton<IMonitorPlugin, MyCoolPlugin>();
```

## Configuration

### appsettings.json Example

```json
{
  "MonitoringSettings": {
    "IntervalSeconds": 2,
    "EnableConsoleOutput": true,
    "EnableFileLogging": true,
    "LogFilePath": "system_monitor.log"
  },
  "ApiSettings": {
    "Endpoint": "https://your-api.com/metrics",
    "TimeoutSeconds": 30,
    "EnableApiIntegration": true
  },
  "Plugins": {
    "FileLogger": { "Enabled": true, "LogLevel": "Information" },
    "ApiPoster": { "Enabled": true, "RetryAttempts": 3, "RetryDelaySeconds": 5 }
  }
}
```

## API Data Format

When it sends data to your API, it looks like this:

```json
{
  "cpu": 25.5,
  "ram_used": 8192.0,
  "ram_total": 16384.0,
  "ram_percent": 50.0,
  "disk_used": 512000.0,
  "disk_total": 1000000.0,
  "disk_percent": 51.2,
  "timestamp": "2025-01-27T10:30:00Z"
}
```

## Platform Support

### What Works Now

- **Windows** - Full support using PerformanceCounters and WMI
- **Linux** - Framework is ready, just needs implementation
- **macOS** - Same as Linux

### Adding New Platforms

1. Implement `ISystemMonitor` for your platform
2. Add it to `SystemMonitorFactory`
3. Use whatever APIs your OS provides (like `/proc` on Linux)

## Problems I Ran Into

### 1. Platform-Specific APIs

**The Issue**: Different OSes give you system info in completely different ways.

**What I Did**: Created an interface that hides all the platform-specific stuff, so each platform can do its own thing.

### 2. Windows Performance Counters

**The Issue**: PerformanceCounters can be finicky and need admin rights.

**What I Did**: Added proper error handling and clear error messages so you know what's wrong.

### 3. Plugin Lifecycle

**The Issue**: Making sure plugins start and stop properly, especially when the app shuts down.

**What I Did**: Used proper async patterns and made sure everything gets cleaned up.

## Current Limitations

### Windows Stuff
- Some metrics need admin privileges
- PerformanceCounters can be fragile

### API Stuff
- Network problems = lost data
- Your API needs to accept POST with JSON

### Resource Stuff
- Monitoring adds a tiny bit of overhead
- Log files can get big over time
- HTTP requests use some network bandwidth

## Testing

### Run Tests
```bash
dotnet test
```

### Manual Testing
1. Run it and check the console output
2. Look for log files
3. Test API integration (try httpbin.org)
4. Play with different config options

## When Things Go Wrong

### Common Problems

1. **"Access Denied" with Performance Counters**
   - Run as Administrator
   - Check Windows permissions

2. **API won't connect**
   - Make sure your endpoint URL is right
   - Check your internet connection
   - Look at timeout settings

3. **Can't write log files**
   - Check folder permissions
   - Make sure you have disk space

### Getting Help

Check the console output and log files - they usually tell you what's wrong.
