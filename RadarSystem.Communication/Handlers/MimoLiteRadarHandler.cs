using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using RadarSystem.Communication.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// MIMO Lite 雷达数据处理器
    /// 端口: 10305
    /// 协议: 5A5A/3C3C 前缀协议
    /// </summary>
    public class MimoLiteRadarHandler : SimpleChannelInboundHandler<byte[]>
    {
        private readonly ILogger<MimoLiteRadarHandler> _logger;
        private readonly string _projectId;
        private readonly string _dataPath;
        private readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        private readonly ConcurrentDictionary<string, DateTime> _lastHeartbeatTime;
        private readonly BlockingCollection<MimoLiteRadarData> _dataQueue;

        // 协议命令定义
        private const string SAR_MIMO_COMMAND_PREFIX = "5A5A";
        private const string SAR_MIMO_RESPONSE_PREFIX = "3C3C";
        private const string SAR_MIMO_COMMAND_HEARTBEAT = "0000";
        private const string SAR_MIMO_COMMAND_TIMESYNC = "1000";
        private const string SAR_MIMO_COMMAND_DEFOIMAGE = "0302"; // 形变图
        private const string SAR_MIMO_COMMAND_SCATIMAGE = "0301"; // 散斑图
        private const string SAR_MIMO_COMMAND_CONFIMAGE = "0303"; // 相干图
        private const string SAR_MIMO_COMMAND_MOVEIMAGE = "0304"; // 动目标图
        private const string SAR_MIMO_RESPONSE_POINT = "0305"; // 监测点
        private const string SAR_MIMO_RESPONSE_POLYGON = "0306"; // 监测区域

        public MimoLiteRadarHandler(
            ILogger<MimoLiteRadarHandler> logger,
            string projectId,
            string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastHeartbeatTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<MimoLiteRadarData>(new ConcurrentQueue<MimoLiteRadarData>());

            // 启动数据处理线程
            Task.Run(() => ProcessDataQueue());
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var remoteAddress = context.Channel.RemoteAddress;
            _logger.LogInformation($"[MIMO Lite] 连接建立，远程地址: {remoteAddress}");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            var remoteAddress = context.Channel.RemoteAddress;
            _logger.LogInformation($"[MIMO Lite] 连接断开，远程地址: {remoteAddress}");
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
                _logger.LogError(ex, "[MIMO Lite] 处理数据时发生异常");
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "[MIMO Lite] 通道异常");
            context.CloseAsync();
        }

        private void HandleData(byte[] msgBytes, IChannelHandlerContext ctx)
        {
            string hexString = ByteUtil.Bytes2Str(msgBytes).ToUpper();
            
            // 协议格式检查
            if (hexString.Length < 16)
            {
                _logger.LogWarning($"[MIMO Lite] 数据长度不足: {hexString.Length}");
                return;
            }

            // 解析协议头
            string prefix = hexString.Substring(0, 4); // 5A5A 或 3C3C
            string slaveIdHex = hexString.Substring(4, 8); // SlaveID (4字节)
            string commandType = hexString.Substring(12, 4); // Command (2字节)
            int slaveId = ByteUtil.StringToInt(slaveIdHex);

            _logger.LogDebug($"[MIMO Lite] 接收数据 - 前缀: {prefix}, SlaveID: {slaveId}, 命令: {commandType}, 长度: {msgBytes.Length}");

            // 根据命令类型处理数据
            if (SAR_MIMO_COMMAND_PREFIX.Equals(prefix))
            {
                HandleCommand(msgBytes, ctx, slaveId.ToString(), commandType, hexString);
            }
            else if (SAR_MIMO_RESPONSE_PREFIX.Equals(prefix))
            {
                HandleResponse(msgBytes, ctx, slaveId.ToString(), commandType, hexString);
            }
        }

        private void HandleCommand(byte[] msgBytes, IChannelHandlerContext ctx, string deviceId, string commandType, string hexString)
        {
            // 记录设备通道
            _deviceChannels.TryAdd(deviceId, ctx);

            switch (commandType)
            {
                case SAR_MIMO_COMMAND_HEARTBEAT:
                    _logger.LogInformation($"[MIMO Lite] 收到心跳包，设备ID: {deviceId}");
                    HandleHeartbeat(ctx, deviceId);
                    break;

                case SAR_MIMO_COMMAND_TIMESYNC:
                    _logger.LogInformation($"[MIMO Lite] 收到时间同步请求，设备ID: {deviceId}");
                    HandleTimeSync(ctx, deviceId);
                    break;

                case SAR_MIMO_COMMAND_DEFOIMAGE:
                    _logger.LogInformation($"[MIMO Lite] 收到形变图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "00", "形变图");
                    break;

                case SAR_MIMO_COMMAND_SCATIMAGE:
                    _logger.LogInformation($"[MIMO Lite] 收到散斑图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "61", "散斑图");
                    break;

                case SAR_MIMO_COMMAND_CONFIMAGE:
                    _logger.LogInformation($"[MIMO Lite] 收到相干图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "02", "相干图");
                    break;

                case SAR_MIMO_COMMAND_MOVEIMAGE:
                    _logger.LogInformation($"[MIMO Lite] 收到动目标图数据，设备ID: {deviceId}, 数据长度: {msgBytes.Length}");
                    HandleImageData(msgBytes, deviceId, "06", "动目标图");
                    break;

                default:
                    _logger.LogWarning($"[MIMO Lite] 未知命令类型: {commandType}, 设备ID: {deviceId}");
                    break;
            }
        }

        private void HandleResponse(byte[] msgBytes, IChannelHandlerContext ctx, string deviceId, string commandType, string hexString)
        {
            switch (commandType)
            {
                case SAR_MIMO_RESPONSE_POINT:
                    _logger.LogInformation($"[MIMO Lite] 收到监测点数据，设备ID: {deviceId}");
                    HandleImageData(msgBytes, deviceId, "63", "监测点");
                    break;

                case SAR_MIMO_RESPONSE_POLYGON:
                    _logger.LogInformation($"[MIMO Lite] 收到监测区域数据，设备ID: {deviceId}");
                    HandleImageData(msgBytes, deviceId, "64", "监测区域");
                    break;

                default:
                    _logger.LogDebug($"[MIMO Lite] 收到响应，命令类型: {commandType}, 设备ID: {deviceId}");
                    break;
            }
        }

        private void HandleHeartbeat(IChannelHandlerContext ctx, string deviceId)
        {
            // 更新心跳时间
            _lastHeartbeatTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);

            // 发送心跳响应
            SendHeartbeatResponse(ctx, deviceId);
        }

        private void SendHeartbeatResponse(IChannelHandlerContext ctx, string deviceId)
        {
            try
            {
                // 构造心跳响应：3C3C + SlaveID + 0000 + 00 + 00000000
                string slaveIdHex = ByteUtil.IntToHexString(int.Parse(deviceId), 4).PadLeft(8, '0');
                string response = $"3C3C{slaveIdHex}000000" + "00000000";
                
                byte[] responseBytes = ByteUtil.HexString2Bytes(response);
                var buffer = Unpooled.WrappedBuffer(responseBytes);
                ctx.WriteAndFlushAsync(buffer);

                _logger.LogDebug($"[MIMO Lite] 发送心跳响应，设备ID: {deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[MIMO Lite] 发送心跳响应失败，设备ID: {deviceId}");
            }
        }

        private void HandleTimeSync(IChannelHandlerContext ctx, string deviceId)
        {
            try
            {
                // 构造时间同步响应：3C3C + SlaveID + 1000 + 00 + Length + Timestamp
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

                _logger.LogInformation($"[MIMO Lite] 发送时间同步响应，设备ID: {deviceId}, 时间: {timestamp}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[MIMO Lite] 发送时间同步响应失败，设备ID: {deviceId}");
            }
        }

        private void HandleImageData(byte[] msgBytes, string deviceId, string imageType, string imageTypeName)
        {
            try
            {
                // 创建数据模型
                var radarData = new MimoLiteRadarData
                {
                    DeviceId = deviceId,
                    SlaveId = deviceId,
                    Timestamp = DateTime.Now,
                    ImageType = imageType,
                    CommandType = imageTypeName,
                    RawData = msgBytes,
                    DataLength = msgBytes.Length
                };

                // 生成文件路径
                string filePath = GetFilePath(imageType, _projectId, deviceId);
                radarData.FilePath = filePath;

                // 提取图像数据（跳过协议头）
                int headerLength = msgBytes[0] == 0x5A ? 12 : 13; // 5A5A=12, 3C3C=13
                if (msgBytes.Length > headerLength)
                {
                    radarData.ImageData = ByteUtil.SubBytes(msgBytes, headerLength, msgBytes.Length - headerLength);
                }

                // 添加到处理队列
                _dataQueue.Add(radarData);

                _logger.LogInformation($"[MIMO Lite] {imageTypeName}数据已加入处理队列，设备ID: {deviceId}, 文件路径: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[MIMO Lite] 处理{imageTypeName}数据失败，设备ID: {deviceId}");
            }
        }

        private string GetFilePath(string imageType, string projectId, string deviceId)
        {
            // 生成文件路径：DataPath/ProjectID/DeviceID/ImageType/YYYYMMDD/HHmmss.dat
            DateTime now = DateTime.Now;
            string dateFolder = now.ToString("yyyyMMdd");
            string fileName = now.ToString("HHmmss") + ".dat";
            
            string folderPath = Path.Combine(_dataPath, projectId, deviceId, imageType, dateFolder);
            Directory.CreateDirectory(folderPath);
            
            return Path.Combine(folderPath, fileName);
        }

        private void ProcessDataQueue()
        {
            _logger.LogInformation("[MIMO Lite] 数据处理线程已启动");

            foreach (var radarData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    // 保存原始数据到文件
                    File.WriteAllBytes(radarData.FilePath, radarData.RawData);
                    _logger.LogInformation($"[MIMO Lite] 数据已保存到文件: {radarData.FilePath}");

                    // TODO: 这里可以添加更多的数据处理逻辑
                    // 例如：解析图像数据、发送到 MQTT、存储到数据库等

                    // 发送到 MQTT（示例）
                    PublishToMqtt(radarData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[MIMO Lite] 处理队列数据失败，设备ID: {radarData.DeviceId}");
                }
            }
        }

        private void PublishToMqtt(MimoLiteRadarData radarData)
        {
            try
            {
                // 构造 MQTT 消息
                var mqttMessage = new
                {
                    DeviceId = radarData.DeviceId,
                    Timestamp = radarData.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    ImageType = radarData.CommandType,
                    FilePath = radarData.FilePath,
                    DataLength = radarData.DataLength
                };

                string topic = $"/radar/mimolite/{radarData.DeviceId}/data";
                string payload = JsonSerializer.Serialize(mqttMessage);

                // TODO: 调用 MQTT 服务发送消息
                _logger.LogDebug($"[MIMO Lite] MQTT消息准备发送 - Topic: {topic}, Payload: {payload}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MIMO Lite] 发送MQTT消息失败");
            }
        }

        public void Dispose()
        {
            _dataQueue?.Dispose();
        }
    }
}

