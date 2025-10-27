using System;
using System.Collections.Concurrent;
using System.Net;
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
    /// 设备 Netty 服务器基类
    /// 所有设备 Netty 服务器的通用实现
    /// </summary>
    public abstract class DeviceNettyServerBase : IDisposable
    {
        protected readonly ILogger _logger;
        protected readonly DeviceNettyConfiguration _config;
        protected readonly MqttService _mqttService;
        protected IEventLoopGroup? _bossGroup;
        protected IEventLoopGroup? _workerGroup;
        protected IChannel? _boundChannel;
        protected bool _isRunning = false;

        // 设备通道映射
        protected readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannelMap;
        
        // 设备ID映射（slaveId -> deviceId）
        protected readonly ConcurrentDictionary<string, string> _deviceIdMap;
        
        // 心跳时间记录
        protected readonly ConcurrentDictionary<string, long> _heartbeatTimeMap;

        public event EventHandler<DeviceDataReceivedEventArgs>? DataReceived;
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

        protected DeviceNettyServerBase(
            ILogger logger,
            DeviceNettyConfiguration config,
            MqttService mqttService)
        {
            _logger = logger;
            _config = config;
            _mqttService = mqttService;
            _deviceChannelMap = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _deviceIdMap = new ConcurrentDictionary<string, string>();
            _heartbeatTimeMap = new ConcurrentDictionary<string, long>();
        }

        /// <summary>
        /// 获取设备类型名称
        /// </summary>
        protected abstract string DeviceTypeName { get; }

        /// <summary>
        /// 创建解码器
        /// </summary>
        protected abstract IChannelHandler CreateDecoder();

        /// <summary>
        /// 创建处理器
        /// </summary>
        protected abstract IChannelHandler CreateHandler();

        /// <summary>
        /// 启动 Netty 服务器
        /// </summary>
        public virtual async Task StartAsync()
        {
            if (_isRunning)
            {
                _logger.LogWarning("{DeviceType} Netty 服务器已经在运行", DeviceTypeName);
                return;
            }

            if (!_config.Enable)
            {
                _logger.LogInformation("{DeviceType} Netty 服务器已禁用", DeviceTypeName);
                return;
            }

            try
            {
                _logger.LogInformation("正在启动 {DeviceType} Netty 服务器，端口: {Port}", DeviceTypeName, _config.Port);

                _bossGroup = new MultithreadEventLoopGroup(1);
                _workerGroup = new MultithreadEventLoopGroup();

                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(_bossGroup, _workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 128)
                    .Handler(new LoggingHandler($"{DeviceTypeName}-SRV"))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast("logger", new LoggingHandler($"{DeviceTypeName}-CONN"));
                        pipeline.AddLast("decoder", CreateDecoder());
                        pipeline.AddLast("handler", CreateHandler());
                    }));

                _boundChannel = await bootstrap.BindAsync(_config.Port);
                _isRunning = true;

                _logger.LogInformation("{DeviceType} Netty 服务器启动成功，监听端口: {Port}", DeviceTypeName, _config.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动 {DeviceType} Netty 服务器失败", DeviceTypeName);
                throw;
            }
        }

        /// <summary>
        /// 停止 Netty 服务器
        /// </summary>
        public virtual async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                _logger.LogInformation("正在停止 {DeviceType} Netty 服务器", DeviceTypeName);

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
                _logger.LogInformation("{DeviceType} Netty 服务器已停止", DeviceTypeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止 {DeviceType} Netty 服务器时发生错误", DeviceTypeName);
            }
        }

        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        internal virtual void HandleData(byte[] data, IChannelHandlerContext context)
        {
            try
            {
                string hexString = BitConverter.ToString(data).Replace("-", "").ToUpper();
                _logger.LogDebug("{DeviceType} 接收到数据: {Length} 字节", DeviceTypeName, data.Length);

                // 触发数据接收事件
                DataReceived?.Invoke(this, new DeviceDataReceivedEventArgs(DeviceTypeName, data, hexString));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理 {DeviceType} 数据时发生错误", DeviceTypeName);
            }
        }

        /// <summary>
        /// 发送命令到设备
        /// </summary>
        protected virtual void SendCommand(IChannelHandlerContext context, string hexCommand)
        {
            try
            {
                _logger.LogDebug("向 {DeviceType} 发送命令: {Command}", DeviceTypeName, hexCommand);
                
                byte[] commandBytes = HexStringToBytes(hexCommand);
                IByteBuffer buffer = Unpooled.WrappedBuffer(commandBytes);
                context.WriteAndFlushAsync(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送命令到 {DeviceType} 失败", DeviceTypeName);
            }
        }

        /// <summary>
        /// 发送心跳到 MQTT
        /// </summary>
        protected virtual void SendHeartbeatToMqtt(string deviceId)
        {
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            if (_heartbeatTimeMap.TryGetValue(deviceId, out long lastTime))
            {
                if ((currentTime - lastTime) / 1000 < 30)
                {
                    return; // 30秒内不重复发送
                }
            }

            var heartbeat = new
            {
                deviceId = deviceId,
                deviceType = DeviceTypeName,
                heartBeatClock = "60",
                time = DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            string json = JsonConvert.SerializeObject(heartbeat);
            _mqttService.PublishAsync("/dev/heartbeat", json).Wait();

            _heartbeatTimeMap[deviceId] = currentTime;
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        protected byte[] HexStringToBytes(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            
            return bytes;
        }

        /// <summary>
        /// 字节数组转十六进制字符串
        /// </summary>
        protected string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// 触发客户端连接事件
        /// </summary>
        internal void OnClientConnected(IChannelHandlerContext context)
        {
            _logger.LogInformation("{DeviceType} 客户端连接: {RemoteAddress}", DeviceTypeName, context.Channel.RemoteAddress);
            ClientConnected?.Invoke(this, new ClientConnectedEventArgs(context));
        }

        /// <summary>
        /// 触发客户端断开事件
        /// </summary>
        internal void OnClientDisconnected(IChannelHandlerContext context)
        {
            _logger.LogInformation("{DeviceType} 客户端断开: {RemoteAddress}", DeviceTypeName, context.Channel.RemoteAddress);
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(context));
        }

        public virtual void Dispose()
        {
            StopAsync().Wait();
        }
    }

    /// <summary>
    /// 设备 Netty 配置
    /// </summary>
    public class DeviceNettyConfiguration
    {
        public int Port { get; set; }
        public bool Enable { get; set; } = true;
        public string ProjectId { get; set; } = "PROJECT001";
        public string DataPath { get; set; } = "../..";
        public string ApiPort { get; set; } = "80";
    }

    /// <summary>
    /// 设备数据接收事件参数
    /// </summary>
    public class DeviceDataReceivedEventArgs : EventArgs
    {
        public string DeviceType { get; }
        public byte[] Data { get; }
        public string HexString { get; }

        public DeviceDataReceivedEventArgs(string deviceType, byte[] data, string hexString)
        {
            DeviceType = deviceType;
            Data = data;
            HexString = hexString;
        }
    }

    /// <summary>
    /// 通用设备处理器
    /// </summary>
    internal class GenericDeviceHandler : ChannelHandlerAdapter
    {
        private readonly DeviceNettyServerBase _server;
        private readonly ILogger _logger;

        public GenericDeviceHandler(DeviceNettyServerBase server, ILogger logger)
        {
            _server = server;
            _logger = logger;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _server.OnClientConnected(context);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _server.OnClientDisconnected(context);
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is byte[] data)
                {
                    _server.HandleData(data, context);
                }
            }
            finally
            {
                if (message is IByteBuffer buffer)
                {
                    buffer.Release();
                }
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "设备处理器发生异常");
            context.CloseAsync();
        }
    }

    /// <summary>
    /// 通用设备解码器
    /// </summary>
    public class GenericDeviceDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (input.ReadableBytes < 4)
            {
                return;
            }

            int availableBytes = input.ReadableBytes;
            byte[] data = new byte[availableBytes];
            input.ReadBytes(data);

            output.Add(data);
        }
    }
}

