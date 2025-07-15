using Microsoft.JSInterop;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace good.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;
    private readonly ILogger<LocalStorageService>? _logger;

    public LocalStorageService(IJSRuntime js, ILogger<LocalStorageService>? logger = null)
    {
        _js = js;
        _logger = logger;
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to set item in localStorage for key {Key}", key);
            throw;
        }
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", key);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get item from localStorage for key {Key}", key);
            return default;
        }
    }
}