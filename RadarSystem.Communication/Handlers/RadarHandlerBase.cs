using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 雷达处理器基类
    /// 适用于所有使用 5A5A/3C3C 协议的雷达设备
    /// </summary>
    public abstract class RadarHandlerBase<TRadarData> : SimpleChannelInboundHandler<byte[]>
        where TRadarData : class
    {
        protected readonly ILogger _logger;
        protected readonly string _projectId;
        protected readonly string _dataPath;
        protected readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        protected readonly ConcurrentDictionary<string, DateTime> _lastHeartbeatTime;
        protected readonly BlockingCollection<TRadarData> _dataQueue;

        // 协议常量
        protected const string SAR_COMMAND_PREFIX = "5A5A";
        protected const string SAR_RESPONSE_PREFIX = "3C3C";
        protected const string COMMAND_HEARTBEAT = "0000";
        protected const string COMMAND_TIMESYNC = "1000";
        protected const string COMMAND_DEFOIMAGE = "0302"; // 形变图
        protected const string COMMAND_SCATIMAGE = "0301"; // 散斑图
        protected const string COMMAND_CONFIMAGE = "0303"; // 相干图
        protected const string COMMAND_MOVEIMAGE = "0304"; // 动目标图
        protected const string RESPONSE_POINT = "0305"; // 监测点
        protected const string RESPONSE_POLYGON = "0306"; // 监测区域

        protected RadarHandlerBase(
            ILogger logger,
            string projectId,
            string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastHeartbeatTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<TRadarData>(new ConcurrentQueue<TRadarData>());

            // 启动数据处理线程
            Task.Run(() => ProcessDataQueue());
        }

        /// <summary>
        /// 获取设备类型名称（用于日志）
        /// </summary>
        protected abstract string DeviceTypeName { get; }

        /// <summary>
        /// 获取支持的图像类型
        /// </summary>
        protected abstract string[] SupportedImageTypes { get; }

        /// <summary>
        /// 创建雷达数据对象
        /// </summary>
        protected abstract TRadarData CreateRadarData(byte[] msgBytes, string deviceId, string imageType, string imageTypeName, string filePath);

        /// <summary>
        /// 获取雷达数据的原始字节
        /// </summary>
        protected abstract byte[] GetRawData(TRadarData radarData);

        /// <summary>
        /// 获取雷达数据的文件路径
        /// </summary>
        protected abstract string GetFilePath(TRadarData radarData);

        /// <summary>
        /// 获取雷达数据的设备ID
        /// </summary>
        protected abstract string GetDeviceId(TRadarData radarData);

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var remoteAddress = context.Channel.RemoteAddress;
            _logger.LogInformation($"[{DeviceTypeName}] 连接建立，远程地址: {remoteAddress}");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            var remoteAddress = context.Channel.RemoteAddress;
            _logger.LogInformation($"[{DeviceTypeName}] 连接断开，远程地址: {remoteAddress}");
            base.ChannelInactive(context);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, byte[] msg)
        {
            try
            {
                HandleData(msg, ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理数据时发生异常");
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, $"[{DeviceTypeName}] 通道异常");
            context.CloseAsync();
        }

        protected virtual void HandleData(byte[] msgBytes, IChannelHandlerContext ctx)
        {
            string hexString = ByteUtil.Bytes2Str(msgBytes).ToUpper();
            
            if (hexString.Length < 16)
            {
                _logger.LogWarning($"[{DeviceTypeName}] 数据长度不足: {hexString.Length}");
                return;
            }

            // 解析协议头
            string prefix = hexString.Substring(0, 4);
            string slaveIdHex = hexString.Substring(4, 8);
            string commandType = hexString.Substring(12, 4);
            int slaveId = ByteUtil.StringToInt(slaveIdHex);

            _logger.LogDebug($"[{DeviceTypeName}] 接收数据 - 前缀: {prefix}, SlaveID: {slaveId}, 命令: {commandType}, 长度: {msgBytes.Length}");

            // 根据命令类型处理数据
            if (SAR_COMMAND_PREFIX.Equals(prefix))
            {
                HandleCommand(msgBytes, ctx, slaveId.ToString(), commandType, hexString);
            }
            else if (SAR_RESPONSE_PREFIX.Equals(prefix))
            {
                HandleResponse(msgBytes, ctx, slaveId.ToString(), commandType, hexString);
            }
        }

        protected virtual void HandleCommand(byte[] msgBytes, IChannelHandlerContext ctx, string deviceId, string commandType, string hexString)
        {
            _deviceChannels.TryAdd(deviceId, ctx);

            switch (commandType)
            {
                case COMMAND_HEARTBEAT:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到心跳包，设备ID: {deviceId}");
                    HandleHeartbeat(ctx, deviceId);
                    break;

                case COMMAND_TIMESYNC:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到时间同步请求，设备ID: {deviceId}");
                    HandleTimeSync(ctx, deviceId);
                    break;

                case COMMAND_DEFOIMAGE:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到形变图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "00", "形变图");
                    break;

                case COMMAND_SCATIMAGE:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到散斑图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "61", "散斑图");
                    break;

                case COMMAND_CONFIMAGE:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到相干图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "02", "相干图");
                    break;

                case COMMAND_MOVEIMAGE:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到动目标图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "06", "动目标图");
                    break;

                default:
                    HandleOtherCommand(msgBytes, ctx, deviceId, commandType, hexString);
                    break;
            }
        }

        protected virtual void HandleResponse(byte[] msgBytes, IChannelHandlerContext ctx, string deviceId, string commandType, string hexString)
        {
            switch (commandType)
            {
                case RESPONSE_POINT:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到监测点数据，设备ID: {deviceId}");
                    HandleImageData(msgBytes, deviceId, "63", "监测点");
                    break;

                case RESPONSE_POLYGON:
                    _logger.LogInformation($"[{DeviceTypeName}] 收到监测区域数据，设备ID: {deviceId}");
                    HandleImageData(msgBytes, deviceId, "64", "监测区域");
                    break;

                default:
                    _logger.LogDebug($"[{DeviceTypeName}] 收到响应，命令类型: {commandType}, 设备ID: {deviceId}");
                    break;
            }
        }

        /// <summary>
        /// 处理其他命令（由子类实现）
        /// </summary>
        protected virtual void HandleOtherCommand(byte[] msgBytes, IChannelHandlerContext ctx, string deviceId, string commandType, string hexString)
        {
            _logger.LogWarning($"[{DeviceTypeName}] 未知命令类型: {commandType}, 设备ID: {deviceId}");
        }

        protected virtual void HandleHeartbeat(IChannelHandlerContext ctx, string deviceId)
        {
            _lastHeartbeatTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
            SendHeartbeatResponse(ctx, deviceId);
        }

        protected virtual void SendHeartbeatResponse(IChannelHandlerContext ctx, string deviceId)
        {
            try
            {
                string slaveIdHex = ByteUtil.IntToHexString(int.Parse(deviceId), 4).PadLeft(8, '0');
                string response = $"3C3C{slaveIdHex}000000" + "00000000";
                
                byte[] responseBytes = ByteUtil.HexString2Bytes(response);
                var buffer = Unpooled.WrappedBuffer(responseBytes);
                ctx.WriteAndFlushAsync(buffer);

                _logger.LogDebug($"[{DeviceTypeName}] 发送心跳响应，设备ID: {deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 发送心跳响应失败，设备ID: {deviceId}");
            }
        }

        protected virtual void HandleTimeSync(IChannelHandlerContext ctx, string deviceId)
        {
            try
            {
                DateTime now = DateTime.Now;
                string timestamp = now.ToString("yyyyMMddHHmmss");
                string timestampHex = ByteUtil.String2HexString(timestamp);
                int dataLength = timestampHex.Length / 2;
                string lengthHex = ByteUtil.IntToHexString(dataLength, 4).PadLeft(8, '0');
                string slaveIdHex = ByteUtil.IntToHexString(int.Parse(deviceId), 4).PadLeft(8, '0');
                
                string response = $"3C3C{slaveIdHex}100000{lengthHex}{timestampHex}";
                
                byte[] responseBytes = ByteUtil.HexString2Bytes(response);
                var buffer = Unpooled.WrappedBuffer(responseBytes);
                ctx.WriteAndFlushAsync(buffer);

                _logger.LogInformation($"[{DeviceTypeName}] 发送时间同步响应，设备ID: {deviceId}, 时间: {timestamp}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 发送时间同步响应失败，设备ID: {deviceId}");
            }
        }

        protected virtual void HandleImageData(byte[] msgBytes, string deviceId, string imageType, string imageTypeName)
        {
            try
            {
                string filePath = GetFilePathInternal(imageType, _projectId, deviceId);
                var radarData = CreateRadarData(msgBytes, deviceId, imageType, imageTypeName, filePath);
                
                _dataQueue.Add(radarData);
                _logger.LogInformation($"[{DeviceTypeName}] {imageTypeName}数据已加入处理队列，设备ID: {deviceId}, 文件路径: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理{imageTypeName}数据失败，设备ID: {deviceId}");
            }
        }

        protected virtual string GetFilePathInternal(string imageType, string projectId, string deviceId)
        {
            DateTime now = DateTime.Now;
            string dateFolder = now.ToString("yyyyMMdd");
            string fileName = now.ToString("HHmmss") + ".dat";
            
            string folderPath = Path.Combine(_dataPath, projectId, deviceId, imageType, dateFolder);
            Directory.CreateDirectory(folderPath);
            
            return Path.Combine(folderPath, fileName);
        }

        protected virtual void ProcessDataQueue()
        {
            _logger.LogInformation($"[{DeviceTypeName}] 数据处理线程已启动");

            foreach (var radarData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    byte[] rawData = GetRawData(radarData);
                    string filePath = GetFilePath(radarData);
                    string deviceId = GetDeviceId(radarData);

                    File.WriteAllBytes(filePath, rawData);
                    _logger.LogInformation($"[{DeviceTypeName}] 数据已保存到文件: {filePath}");

                    PublishToMqtt(radarData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{DeviceTypeName}] 处理队列数据失败");
                }
            }
        }

        protected virtual void PublishToMqtt(TRadarData radarData)
        {
            try
            {
                string deviceId = GetDeviceId(radarData);
                string topic = $"/radar/{DeviceTypeName.ToLower()}/{deviceId}/data";
                
                var mqttMessage = new
                {
                    DeviceId = deviceId,
                    DeviceType = DeviceTypeName,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    FilePath = GetFilePath(radarData)
                };

                string payload = JsonSerializer.Serialize(mqttMessage);
                _logger.LogDebug($"[{DeviceTypeName}] MQTT消息准备发送 - Topic: {topic}, Payload: {payload}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 发送MQTT消息失败");
            }
        }

        public void Dispose()
        {
            _dataQueue?.Dispose();
        }
    }
}

