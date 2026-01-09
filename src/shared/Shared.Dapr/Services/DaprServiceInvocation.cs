using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace OpenFindBearings.Shared.Dapr.Services;

/// <summary>
/// Dapr 服务调用封装
/// </summary>
public interface IDaprServiceInvocation
{
    Task<T?> InvokeAsync<T>(HttpMethod method, string appId, string methodName, object? data = null);
    Task<T?> InvokeAsync<T>(string appId, string methodName, object? data = null);
}

public class DaprServiceInvocation : IDaprServiceInvocation
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprServiceInvocation> _logger;

    public DaprServiceInvocation(DaprClient daprClient, ILogger<DaprServiceInvocation> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<T?> InvokeAsync<T>(HttpMethod method, string appId, string methodName, object? data = null)
    {
        try
        {
            _logger.LogDebug("调用服务: {AppId}, 方法: {MethodName}", appId, methodName);

            // Dapr 1.13.0 使用不同的 API 签名
            return await _daprClient.InvokeMethodAsync<T>(
                method,
                appId,
                methodName,
                cancellationToken: default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "服务调用失败: {AppId}, 方法: {MethodName}", appId, methodName);
            throw;
        }
    }

    public async Task<T?> InvokeAsync<T>(string appId, string methodName, object? data = null)
    {
        return await InvokeAsync<T>(HttpMethod.Post, appId, methodName, data);
    }
}
