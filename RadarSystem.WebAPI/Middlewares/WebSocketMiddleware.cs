using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace RadarSystem.WebAPI.Middlewares;

/// <summary>
/// WebSocket中间件 - 处理实时数据推送
/// </summary>
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<WebSocket, byte> _sockets = new();
    private readonly ILogger<WebSocketMiddleware> _logger;

    public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 处理WebSocket请求
        if (context.Request.Path == "/wss" || context.Request.Path == "/wss/")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _sockets.TryAdd(webSocket, 0);
                _logger.LogInformation("WebSocket连接已建立，当前连接数: {Count}", _sockets.Count);
                
                try
                {
                    await HandleWebSocketAsync(webSocket);
                }
                finally
                {
                    _sockets.TryRemove(webSocket, out _);
                    _logger.LogInformation("WebSocket连接已关闭，当前连接数: {Count}", _sockets.Count);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("WebSocket连接请求无效");
            }
        }
        else
        {
            await _next(context);
        }
    }

    private async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var cts = new CancellationTokenSource();
        
        try
        {
            // ✅ 不发送欢迎消息（MQTT客户端不期望）
            // MQTT over WebSocket客户端会自己发送CONNECT包
            
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), 
                        cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("客户端请求关闭WebSocket连接");
                        if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                        {
                            await webSocket.CloseAsync(
                                WebSocketCloseStatus.NormalClosure, 
                                "连接已关闭", 
                                cts.Token);
                        }
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text || result.MessageType == WebSocketMessageType.Binary)
                    {
                        // ✅ 处理MQTT消息（可能是文本或二进制）
                        _logger.LogDebug("收到消息: {Type}, 长度: {Length}", result.MessageType, result.Count);
                        
                        // MQTT协议处理（暂时简单回显）
                        if (webSocket.State == WebSocketState.Open)
                        {
                            await webSocket.SendAsync(
                                new ArraySegment<byte>(buffer, 0, result.Count),
                                result.MessageType,
                                result.EndOfMessage,
                                cts.Token);
                        }
                    }
                }
                catch (WebSocketException wsEx)
                {
                    // 检查是否是正常的关闭操作
                    if (wsEx.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                        wsEx.WebSocketErrorCode == WebSocketError.InvalidState)
                    {
                        _logger.LogInformation("WebSocket连接已关闭: {Reason}", wsEx.Message);
                    }
                    else
                    {
                        _logger.LogWarning(wsEx, "WebSocket操作异常: {WebSocketError}", wsEx.WebSocketErrorCode);
                    }
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("WebSocket操作已取消");
        }
        catch (WebSocketException ex)
        {
            // 检查是否是客户端主动关闭连接
            if (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                ex.WebSocketErrorCode == WebSocketError.InvalidState)
            {
                _logger.LogInformation("客户端关闭了WebSocket连接: {Reason}", ex.Message);
            }
            else
            {
                _logger.LogWarning(ex, "WebSocket连接异常: {WebSocketError}", ex.WebSocketErrorCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理WebSocket消息时发生错误");
        }
        finally
        {
            // 确保连接正确关闭
            if (webSocket.State == WebSocketState.Open || 
                webSocket.State == WebSocketState.CloseReceived ||
                webSocket.State == WebSocketState.CloseSent)
            {
                try
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, 
                        "连接关闭", 
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "关闭WebSocket连接时发生异常（可忽略）");
                }
            }
            cts.Dispose();
        }
    }

    /// <summary>
    /// 广播消息给所有连接的客户端
    /// </summary>
    public static async Task BroadcastAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        var buffer = Encoding.UTF8.GetBytes(message);
        var sendTasks = _sockets.Keys
            .Where(s => s.State == WebSocketState.Open)
            .Select(async s =>
            {
                try
                {
                    await s.SendAsync(
                        new ArraySegment<byte>(buffer), 
                        WebSocketMessageType.Text, 
                        true, 
                        CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    // 客户端连接已断开，忽略此异常
                }
                catch (Exception)
                {
                    // 其他异常，忽略
                }
            });
        
        await Task.WhenAll(sendTasks);
    }

    /// <summary>
    /// 广播JSON对象给所有连接的客户端
    /// </summary>
    public static async Task BroadcastJsonAsync(object data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        await BroadcastAsync(json);
    }

    /// <summary>
    /// 获取当前WebSocket连接数
    /// </summary>
    public static int GetConnectionCount()
    {
        return _sockets.Keys.Count(s => s.State == WebSocketState.Open);
    }
}

