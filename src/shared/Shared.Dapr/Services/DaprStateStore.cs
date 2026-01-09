using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace OpenFindBearings.Shared.Dapr.Services;

/// <summary>
/// Dapr 状态存储封装
/// </summary>
public interface IDaprStateStore
{
    Task<T?> GetAsync<T>(string key, string storeName = "statestore") where T : class;
    Task SetAsync<T>(string key, T value, string storeName = "statestore", StateOptions? options = null) where T : class;
    Task DeleteAsync(string key, string storeName = "statestore");
    Task<bool> ExistsAsync(string key, string storeName = "statestore");
}

public class DaprStateStore : IDaprStateStore
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprStateStore> _logger;

    public DaprStateStore(DaprClient daprClient, ILogger<DaprStateStore> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, string storeName = "statestore") where T : class
    {
        try
        {
            var result = await _daprClient.GetStateAsync<T>(storeName, key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取状态失败: Key={Key}, Store={Store}", key, storeName);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, string storeName = "statestore", StateOptions? options = null) where T : class
    {
        try
        {
            await _daprClient.SaveStateAsync(storeName, key, value, options);
            _logger.LogDebug("保存状态成功: Key={Key}, Store={Store}", key, storeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存状态失败: Key={Key}, Store={Store}", key, storeName);
            throw;
        }
    }

    public async Task DeleteAsync(string key, string storeName = "statestore")
    {
        try
        {
            await _daprClient.DeleteStateAsync(storeName, key);
            _logger.LogDebug("删除状态成功: Key={Key}, Store={Store}", key, storeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除状态失败: Key={Key}, Store={Store}", key, storeName);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, string storeName = "statestore")
    {
        try
        {
            var (value, etag) = await _daprClient.GetStateAndETagAsync<string>(storeName, key);
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }
}
