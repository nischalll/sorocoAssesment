using SorocoSystemMonitor.Interfaces;
using SorocoSystemMonitor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace SorocoSystemMonitor.Plugins;

public class ApiPosterPlugin : IMonitorPlugin
{
    private readonly ILogger<ApiPosterPlugin> _logger;
    private readonly ApiSettings _apiSettings;
    private readonly ApiPosterSettings _pluginSettings;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public string Name => "ApiPoster";
    public bool IsEnabled { get; private set; }

    public ApiPosterPlugin(ILogger<ApiPosterPlugin> logger, IOptions<AppConfiguration> configuration, HttpClient httpClient)
    {
        _logger = logger;
        _apiSettings = configuration.Value.ApiSettings;
        _pluginSettings = configuration.Value.Plugins.ApiPoster;
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            WriteIndented = false
        };
        
        IsEnabled = _apiSettings.EnableApiIntegration && _pluginSettings.Enabled;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (!IsEnabled)
            {
                _logger.LogInformation("ApiPoster plugin is disabled");
                return;
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(_apiSettings.TimeoutSeconds);
            
            var testResponse = await _httpClient.GetAsync(_apiSettings.Endpoint);
            if (testResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("ApiPoster plugin initialized successfully. Endpoint: {Endpoint}", _apiSettings.Endpoint);
            }
            else
            {
                _logger.LogWarning("API endpoint test returned status: {StatusCode}", testResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ApiPoster plugin");
            IsEnabled = false;
        }
    }

    public async Task ProcessMetricsAsync(SystemMetrics metrics)
    {
        if (!IsEnabled)
            return;

        var retryCount = 0;
        var maxRetries = _pluginSettings.RetryAttempts;

        while (retryCount <= maxRetries)
        {
            try
            {
                var payload = metrics.ToApiPayload();
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Posting metrics to API: {Json}", json);

                var response = await _httpClient.PostAsync(_apiSettings.Endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Metrics successfully posted to API. Status: {StatusCode}", response.StatusCode);
                    return;
                }
                else
                {
                    _logger.LogWarning("API request failed with status: {StatusCode}", response.StatusCode);
                    
                    if (retryCount < maxRetries)
                    {
                        retryCount++;
                        var delay = _pluginSettings.RetryDelaySeconds * 1000;
                        _logger.LogInformation("Retrying API request in {Delay}ms (attempt {RetryCount}/{MaxRetries})", delay, retryCount, maxRetries);
                        await Task.Delay(delay);
                    }
                    else
                    {
                        _logger.LogError("API request failed after {MaxRetries} retry attempts", maxRetries);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP request failed (attempt {RetryCount}/{MaxRetries})", retryCount);
                
                if (retryCount < maxRetries)
                {
                    retryCount++;
                    var delay = _pluginSettings.RetryDelaySeconds * 1000;
                    await Task.Delay(delay);
                }
                else
                {
                    _logger.LogError(ex, "API request failed after {MaxRetries} retry attempts", maxRetries);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "API request timed out (attempt {RetryCount}/{MaxRetries})", retryCount);
                
                if (retryCount < maxRetries)
                {
                    retryCount++;
                    var delay = _pluginSettings.RetryDelaySeconds * 1000;
                    await Task.Delay(delay);
                }
                else
                {
                    _logger.LogError(ex, "API request failed after {MaxRetries} retry attempts", maxRetries);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during API request");
                break;
            }
        }
    }

    public Task ShutdownAsync()
    {
        _logger.LogInformation("ApiPoster plugin shutdown completed");
        return Task.CompletedTask;
    }
}

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var result = new StringBuilder();
        result.Append(char.ToLowerInvariant(name[0]));

        for (int i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(name[i]));
            }
            else
            {
                result.Append(name[i]);
            }
        }

        return result.ToString();
    }
} 