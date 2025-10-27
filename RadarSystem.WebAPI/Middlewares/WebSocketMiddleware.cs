using System.Net.WebSockets;
using System.Text;

namespace RadarSystem.WebAPI.Middlewares;

/// <summary>
/// WebSocket中间件 - 处理实时数据推送
/// </summary>
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly List<WebSocket> _sockets = new();
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
                _logger.LogInformation("WebSocket连接已建立，当前连接数: {Count}", _sockets.Count + 1);
                
                _sockets.Add(webSocket);
                
                try
                {
                    await HandleWebSocketAsync(webSocket);
                }
                finally
                {
                    _sockets.Remove(webSocket);
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
        
        try
        {
            // 发送欢迎消息
            var welcome = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "welcome",
                message = "WebSocket连接成功",
                timestamp = DateTime.Now
            }));
            await webSocket.SendAsync(
                new ArraySegment<byte>(welcome), 
                WebSocketMessageType.Text, 
                true, 
                CancellationToken.None);

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("客户端请求关闭WebSocket连接");
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, 
                        "连接已关闭", 
                        CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogDebug("收到WebSocket消息: {Message}", message);
                    
                    // 回显消息（测试用）
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, result.Count),
                        result.MessageType,
                        result.EndOfMessage,
                        CancellationToken.None);
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket连接异常");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理WebSocket消息时发生错误");
        }
    }

    /// <summary>
    /// 广播消息给所有连接的客户端
    /// </summary>
    public static async Task BroadcastAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var sendTasks = _sockets
            .Where(s => s.State == WebSocketState.Open)
            .Select(s => s.SendAsync(
                new ArraySegment<byte>(buffer), 
                WebSocketMessageType.Text, 
                true, 
                CancellationToken.None));
        
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
        return _sockets.Count(s => s.State == WebSocketState.Open);
    }
}

