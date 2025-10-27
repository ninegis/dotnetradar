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
    /// DotNetty 服务器服务（对应 Java Netty）
    /// </summary>
    public class NettyServerService : IDisposable
    {
        private readonly ILogger<NettyServerService> _logger;
        private readonly NettyServerConfiguration _config;
        private IEventLoopGroup? _bossGroup;
        private IEventLoopGroup? _workerGroup;
        private IChannel? _boundChannel;
        private bool _isRunning = false;

        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

        public NettyServerService(ILogger<NettyServerService> logger, NettyServerConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// 启动 Netty 服务器
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                _logger.LogWarning("Netty 服务器已经在运行");
                return;
            }

            try
            {
                _logger.LogInformation("正在启动 Netty 服务器，端口: {Port}", _config.Port);

                // 创建事件循环组（对应 Java Netty 的 EventLoopGroup）
                _bossGroup = new MultithreadEventLoopGroup(1);
                _workerGroup = new MultithreadEventLoopGroup();

                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(_bossGroup, _workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // 添加日志处理器
                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));

                        // 添加编解码器
                        if (_config.UseFrameDecoder)
                        {
                            // 使用长度字段解码器（类似 Java Netty 的 LengthFieldBasedFrameDecoder）
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
                        pipeline.AddLast("handler", new NettyServerHandler(this, _logger));
                    }));

                // 绑定端口并启动服务器
                _boundChannel = await bootstrap.BindAsync(_config.Port);
                _isRunning = true;

                _logger.LogInformation("Netty 服务器启动成功，监听端口: {Port}", _config.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动 Netty 服务器失败");
                throw;
            }
        }

        /// <summary>
        /// 停止 Netty 服务器
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                _logger.LogInformation("正在停止 Netty 服务器");

                if (_boundChannel != null)
                {
                    await _boundChannel.CloseAsync();
                }

                if (_workerGroup != null)
                {
                    await _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                }

                if (_bossGroup != null)
                {
                    await _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                }

                _isRunning = false;
                _logger.LogInformation("Netty 服务器已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止 Netty 服务器时发生错误");
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
        /// 触发客户端连接事件
        /// </summary>
        internal void OnClientConnected(IChannelHandlerContext context)
        {
            ClientConnected?.Invoke(this, new ClientConnectedEventArgs(context));
        }

        /// <summary>
        /// 触发客户端断开事件
        /// </summary>
        internal void OnClientDisconnected(IChannelHandlerContext context)
        {
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(context));
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }

    /// <summary>
    /// Netty 服务器处理器（对应 Java Netty 的 ChannelInboundHandlerAdapter）
    /// </summary>
    internal class NettyServerHandler : ChannelHandlerAdapter
    {
        private readonly NettyServerService _service;
        private readonly ILogger _logger;

        public NettyServerHandler(NettyServerService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogInformation("客户端连接: {RemoteAddress}", context.Channel.RemoteAddress);
            _service.OnClientConnected(context);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation("客户端断开: {RemoteAddress}", context.Channel.RemoteAddress);
            _service.OnClientDisconnected(context);
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
            _logger.LogError(exception, "Netty 处理器发生异常");
            context.CloseAsync();
        }
    }

    /// <summary>
    /// Netty 服务器配置
    /// </summary>
    public class NettyServerConfiguration
    {
        public int Port { get; set; } = 8080;
        public bool UseFrameDecoder { get; set; } = true;
        public bool UseStringCodec { get; set; } = false;
        public int MaxFrameLength { get; set; } = 1024 * 1024; // 1MB
        public int BossThreads { get; set; } = 1;
        public int WorkerThreads { get; set; } = Environment.ProcessorCount * 2;
    }

    /// <summary>
    /// 数据接收事件参数
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        public IChannelHandlerContext Context { get; }
        public object Data { get; }

        public DataReceivedEventArgs(IChannelHandlerContext context, object data)
        {
            Context = context;
            Data = data;
        }
    }

    /// <summary>
    /// 客户端连接事件参数
    /// </summary>
    public class ClientConnectedEventArgs : EventArgs
    {
        public IChannelHandlerContext Context { get; }

        public ClientConnectedEventArgs(IChannelHandlerContext context)
        {
            Context = context;
        }
    }

    /// <summary>
    /// 客户端断开事件参数
    /// </summary>
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public IChannelHandlerContext Context { get; }

        public ClientDisconnectedEventArgs(IChannelHandlerContext context)
        {
            Context = context;
        }
    }
}

