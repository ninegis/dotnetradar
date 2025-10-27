using System;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RadarSystem.Core.Models;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 雷达数据 Netty 处理器
    /// 专门用于处理雷达数据的接收和发送
    /// </summary>
    public class RadarDataNettyHandler : ChannelHandlerAdapter
    {
        private readonly ILogger<RadarDataNettyHandler> _logger;
        private readonly Action<ReceivedRadarData>? _onRadarDataReceived;

        public RadarDataNettyHandler(
            ILogger<RadarDataNettyHandler> logger,
            Action<ReceivedRadarData>? onRadarDataReceived = null)
        {
            _logger = logger;
            _onRadarDataReceived = onRadarDataReceived;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogInformation("雷达数据通道已激活: {RemoteAddress}", context.Channel.RemoteAddress);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation("雷达数据通道已断开: {RemoteAddress}", context.Channel.RemoteAddress);
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    // 读取字节数据
                    int readableBytes = buffer.ReadableBytes;
                    byte[] data = new byte[readableBytes];
                    buffer.ReadBytes(data);

                    _logger.LogDebug("接收到雷达数据，长度: {Length} 字节", readableBytes);

                    // 尝试解析为雷达数据
                    ProcessRadarData(data);
                }
                else if (message is string jsonString)
                {
                    _logger.LogDebug("接收到雷达数据 JSON: {Json}", jsonString);

                    // 解析 JSON 为雷达数据对象
                    var radarData = JsonConvert.DeserializeObject<ReceivedRadarData>(jsonString);
                    if (radarData != null)
                    {
                        _onRadarDataReceived?.Invoke(radarData);
                    }
                }
                else
                {
                    _logger.LogWarning("接收到未知类型的消息: {Type}", message.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理雷达数据时发生错误");
            }
            finally
            {
                // 释放消息资源
                if (message is IByteBuffer buf)
                {
                    buf.Release();
                }
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "雷达数据处理器发生异常");
            context.CloseAsync();
        }

        /// <summary>
        /// 处理雷达数据
        /// </summary>
        private void ProcessRadarData(byte[] data)
        {
            try
            {
                // 这里可以根据实际的雷达数据协议进行解析
                // 示例：假设数据格式为 [Header(4字节)] [DeviceId(16字节)] [Timestamp(8字节)] [Data(剩余)]

                if (data.Length < 28)
                {
                    _logger.LogWarning("雷达数据长度不足，无法解析");
                    return;
                }

                // 解析数据头
                int header = BitConverter.ToInt32(data, 0);

                // 解析设备ID
                string deviceId = System.Text.Encoding.UTF8.GetString(data, 4, 16).TrimEnd('\0');

                // 解析时间戳
                long timestamp = BitConverter.ToInt64(data, 20);
                DateTime receiveTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

                // 解析雷达数据
                byte[] radarDataBytes = new byte[data.Length - 28];
                Array.Copy(data, 28, radarDataBytes, 0, radarDataBytes.Length);

                // 创建雷达数据对象
                var radarData = new ReceivedRadarData
                {
                    DeviceId = deviceId,
                    ReceiveTime = receiveTime,
                    ImageData = radarDataBytes,
                    DataType = "Binary"
                };

                _logger.LogInformation("解析雷达数据成功: DeviceId={DeviceId}, DataLength={Length}", 
                    deviceId, radarDataBytes.Length);

                // 触发回调
                _onRadarDataReceived?.Invoke(radarData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析雷达数据失败");
            }
        }

        /// <summary>
        /// 发送雷达数据
        /// </summary>
        public static async Task SendRadarDataAsync(IChannel channel, ReceivedRadarData radarData)
        {
            try
            {
                // 构建数据包
                // [Header(4字节)] [DeviceId(16字节)] [Timestamp(8字节)] [Data(剩余)]
                int totalLength = 28 + radarData.ImageData.Length;
                byte[] packet = new byte[totalLength];

                // 写入头部
                BitConverter.GetBytes(0x52444152).CopyTo(packet, 0); // "RDAR" in hex

                // 写入设备ID
                byte[] deviceIdBytes = System.Text.Encoding.UTF8.GetBytes(radarData.DeviceId);
                Array.Copy(deviceIdBytes, 0, packet, 4, Math.Min(deviceIdBytes.Length, 16));

                // 写入时间戳
                long timestamp = new DateTimeOffset(radarData.ReceiveTime).ToUnixTimeMilliseconds();
                BitConverter.GetBytes(timestamp).CopyTo(packet, 20);

                // 写入雷达数据
                Array.Copy(radarData.ImageData, 0, packet, 28, radarData.ImageData.Length);

                // 发送数据
                IByteBuffer buffer = Unpooled.WrappedBuffer(packet);
                await channel.WriteAndFlushAsync(buffer);
            }
            catch (Exception ex)
            {
                throw new Exception("发送雷达数据失败", ex);
            }
        }
    }

    /// <summary>
    /// 雷达数据 Netty 服务
    /// 封装了雷达数据的接收和发送功能
    /// </summary>
    public class RadarDataNettyService : IDisposable
    {
        private readonly ILogger<RadarDataNettyService> _logger;
        private readonly NettyServerService? _server;
        private readonly NettyClientService? _client;

        public event EventHandler<ReceivedRadarData>? RadarDataReceived;

        /// <summary>
        /// 创建服务器模式的雷达数据服务
        /// </summary>
        public RadarDataNettyService(
            ILogger<RadarDataNettyService> logger,
            NettyServerConfiguration serverConfig)
        {
            _logger = logger;
            _server = new NettyServerService(
                logger as ILogger<NettyServerService> ?? throw new ArgumentNullException(nameof(logger)),
                serverConfig);

            _server.DataReceived += OnServerDataReceived;
        }

        /// <summary>
        /// 创建客户端模式的雷达数据服务
        /// </summary>
        public RadarDataNettyService(
            ILogger<RadarDataNettyService> logger,
            NettyClientConfiguration clientConfig)
        {
            _logger = logger;
            _client = new NettyClientService(
                logger as ILogger<NettyClientService> ?? throw new ArgumentNullException(nameof(logger)),
                clientConfig);

            _client.DataReceived += OnClientDataReceived;
        }

        /// <summary>
        /// 启动服务（服务器模式）
        /// </summary>
        public async Task StartServerAsync()
        {
            if (_server == null)
            {
                throw new InvalidOperationException("当前服务不是服务器模式");
            }

            await _server.StartAsync();
            _logger.LogInformation("雷达数据 Netty 服务器已启动");
        }

        /// <summary>
        /// 连接服务器（客户端模式）
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            if (_client == null)
            {
                throw new InvalidOperationException("当前服务不是客户端模式");
            }

            bool connected = await _client.ConnectAsync();
            if (connected)
            {
                _logger.LogInformation("雷达数据 Netty 客户端已连接");
            }
            return connected;
        }

        /// <summary>
        /// 发送雷达数据
        /// </summary>
        public async Task<bool> SendRadarDataAsync(ReceivedRadarData radarData)
        {
            if (_client != null)
            {
                return await _client.SendJsonAsync(radarData);
            }

            _logger.LogWarning("服务器模式不支持主动发送数据");
            return false;
        }

        private void OnServerDataReceived(object? sender, DataReceivedEventArgs e)
        {
            ProcessReceivedData(e.Data);
        }

        private void OnClientDataReceived(object? sender, DataReceivedEventArgs e)
        {
            ProcessReceivedData(e.Data);
        }

        private void ProcessReceivedData(object data)
        {
            try
            {
                if (data is string jsonString)
                {
                    var radarData = JsonConvert.DeserializeObject<ReceivedRadarData>(jsonString);
                    if (radarData != null)
                    {
                        RadarDataReceived?.Invoke(this, radarData);
                    }
                }
                else if (data is IByteBuffer buffer)
                {
                    // 处理二进制数据
                    int readableBytes = buffer.ReadableBytes;
                    byte[] bytes = new byte[readableBytes];
                    buffer.ReadBytes(bytes);

                    // 这里可以根据实际协议解析
                    _logger.LogDebug("接收到雷达数据字节流，长度: {Length}", readableBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理接收到的雷达数据时发生错误");
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
            _client?.Dispose();
        }
    }
}

