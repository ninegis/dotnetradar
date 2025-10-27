using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// DotNetty 客户端服务（对应 Java Netty）
    /// </summary>
    public class NettyClientService : IDisposable
    {
        private readonly ILogger<NettyClientService> _logger;
        private readonly NettyClientConfiguration _config;
        private IEventLoopGroup? _group;
        private IChannel? _channel;
        private bool _isConnected = false;

        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        public bool IsConnected => _isConnected;

        public NettyClientService(ILogger<NettyClientService> logger, NettyClientConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            if (_isConnected)
            {
                _logger.LogWarning("Netty 客户端已经连接");
                return true;
            }

            try
            {
                _logger.LogInformation("正在连接到 Netty 服务器: {Host}:{Port}", _config.Host, _config.Port);

                // 创建事件循环组
                _group = new MultithreadEventLoopGroup();

                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(_group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.SoKeepalive, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // 添加日志处理器
                        pipeline.AddLast(new LoggingHandler("CLI-CONN"));

                        // 添加编解码器
                        if (_config.UseFrameDecoder)
                        {
                            // 使用长度字段解码器
                            pipeline.AddLast("frameDecoder", new LengthFieldBasedFrameDecoder(
                                maxFrameLength: _config.MaxFrameLength,
                                lengthFieldOffset: 0,
                                lengthFieldLength: 4,
                                lengthAdjustment: 0,
                                initialBytesToStrip: 4));

                            pipeline.AddLast("frameEncoder", new LengthFieldPrepender(4));
                        }

                        // 添加字符串编解码器（如果需要）
                        if (_config.UseStringCodec)
                        {
                            pipeline.AddLast("stringDecoder", new StringDecoder(Encoding.UTF8));
                            pipeline.AddLast("stringEncoder", new StringEncoder(Encoding.UTF8));
                        }

                        // 添加业务处理器
                        pipeline.AddLast("handler", new NettyClientHandler(this, _logger));
                    }));

                // 连接到服务器
                _channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_config.Host), _config.Port));
                _isConnected = true;

                _logger.LogInformation("Netty 客户端连接成功: {Host}:{Port}", _config.Host, _config.Port);
                Connected?.Invoke(this, EventArgs.Empty);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接 Netty 服务器失败");
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!_isConnected)
            {
                return;
            }

            try
            {
                _logger.LogInformation("正在断开 Netty 客户端连接");

                if (_channel != null)
                {
                    await _channel.CloseAsync();
                }

                if (_group != null)
                {
                    await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                }

                _isConnected = false;
                _logger.LogInformation("Netty 客户端已断开连接");
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开 Netty 客户端连接时发生错误");
            }
        }

        /// <summary>
        /// 发送字节数据
        /// </summary>
        public async Task<bool> SendAsync(byte[] data)
        {
            if (!_isConnected || _channel == null)
            {
                _logger.LogWarning("Netty 客户端未连接，无法发送数据");
                return false;
            }

            try
            {
                IByteBuffer buffer = Unpooled.WrappedBuffer(data);
                await _channel.WriteAndFlushAsync(buffer);
                _logger.LogDebug("发送数据成功，长度: {Length}", data.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送数据失败");
                return false;
            }
        }

        /// <summary>
        /// 发送字符串数据
        /// </summary>
        public async Task<bool> SendAsync(string message)
        {
            if (!_isConnected || _channel == null)
            {
                _logger.LogWarning("Netty 客户端未连接，无法发送数据");
                return false;
            }

            try
            {
                if (_config.UseStringCodec)
                {
                    // 如果使用字符串编解码器，直接发送字符串
                    await _channel.WriteAndFlushAsync(message);
                }
                else
                {
                    // 否则转换为字节数组发送
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await SendAsync(data);
                }

                _logger.LogDebug("发送消息成功: {Message}", message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息失败");
                return false;
            }
        }

        /// <summary>
        /// 发送 JSON 对象
        /// </summary>
        public async Task<bool> SendJsonAsync<T>(T obj)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj);
                return await SendAsync(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送 JSON 对象失败");
                return false;
            }
        }

        /// <summary>
        /// 触发数据接收事件
        /// </summary>
        internal void OnDataReceived(IChannelHandlerContext context, object data)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(context, data));
        }

        /// <summary>
        /// 触发连接断开事件
        /// </summary>
        internal void OnDisconnected()
        {
            _isConnected = false;
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            DisconnectAsync().Wait();
        }
    }

    /// <summary>
    /// Netty 客户端处理器（对应 Java Netty 的 ChannelInboundHandlerAdapter）
    /// </summary>
    internal class NettyClientHandler : ChannelHandlerAdapter
    {
        private readonly NettyClientService _service;
        private readonly ILogger _logger;

        public NettyClientHandler(NettyClientService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogInformation("连接已建立: {RemoteAddress}", context.Channel.RemoteAddress);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation("连接已断开: {RemoteAddress}", context.Channel.RemoteAddress);
            _service.OnDisconnected();
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                _logger.LogDebug("接收到数据: {MessageType}", message.GetType().Name);
                _service.OnDataReceived(context, message);
            }
            finally
            {
                // 释放消息资源
                if (message is IByteBuffer buffer)
                {
                    buffer.Release();
                }
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "Netty 客户端处理器发生异常");
            context.CloseAsync();
        }
    }

    /// <summary>
    /// Netty 客户端配置
    /// </summary>
    public class NettyClientConfiguration
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8080;
        public bool UseFrameDecoder { get; set; } = true;
        public bool UseStringCodec { get; set; } = false;
        public int MaxFrameLength { get; set; } = 1024 * 1024; // 1MB
        public int ConnectTimeoutSeconds { get; set; } = 30;
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectDelaySeconds { get; set; } = 5;
    }
}

