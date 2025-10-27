using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 传感器处理器基类
    /// 适用于倾斜仪、振动传感器、方向传感器、激光设备、CM设备等
    /// </summary>
    public abstract class SensorHandlerBase<TSensorData> : SimpleChannelInboundHandler<byte[]>
        where TSensorData : class
    {
        protected readonly ILogger _logger;
        protected readonly string _projectId;
        protected readonly string _dataPath;
        protected readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        protected readonly ConcurrentDictionary<string, DateTime> _lastUpdateTime;
        protected readonly BlockingCollection<TSensorData> _dataQueue;

        protected const string REGIST_MSG = "registMsg";
        protected const string SLAVE_ID_FIELD = "slaveId:";

        protected SensorHandlerBase(
            ILogger logger,
            string projectId,
            string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastUpdateTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<TSensorData>(new ConcurrentQueue<TSensorData>());

            // 启动数据处理线程
            Task.Run(() => ProcessDataQueue());
        }

        /// <summary>
        /// 获取设备类型名称（用于日志和存储）
        /// </summary>
        protected abstract string DeviceTypeName { get; }

        /// <summary>
        /// 创建传感器数据对象
        /// </summary>
        protected abstract TSensorData CreateSensorData(string deviceId, byte[] rawData, string jsonData);

        /// <summary>
        /// 获取传感器数据的设备ID
        /// </summary>
        protected abstract string GetDeviceId(TSensorData sensorData);

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
            try
            {
                // 尝试转换为字符串（很多传感器使用文本/JSON协议）
                string msgStr = Encoding.UTF8.GetString(msgBytes);
                
                _logger.LogDebug($"[{DeviceTypeName}] 接收数据: {msgStr}");

                // 检查是否是注册消息
                if (msgStr.Contains(REGIST_MSG) || msgStr.Equals(REGIST_MSG))
                {
                    HandleRegistration(msgStr, ctx);
                }
                // 检查是否包含 slaveId 字段
                else if (msgStr.Contains(SLAVE_ID_FIELD))
                {
                    HandleJsonData(msgBytes, msgStr, ctx);
                }
                else
                {
                    // 处理其他格式的数据
                    HandleRawData(msgBytes, msgStr, ctx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理数据失败");
            }
        }

        protected virtual void HandleRegistration(string msgStr, IChannelHandlerContext ctx)
        {
            try
            {
                _logger.LogInformation($"[{DeviceTypeName}] 收到设备注册消息");
                
                // 提取设备ID（如果有）
                string deviceId = ExtractDeviceId(msgStr);
                
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _deviceChannels.TryAdd(deviceId, ctx);
                    _logger.LogInformation($"[{DeviceTypeName}] 设备注册成功，设备ID: {deviceId}");
                    
                    SendRegistrationResponse(ctx, deviceId);
                }
                else
                {
                    // 使用远程地址作为临时ID
                    deviceId = ctx.Channel.RemoteAddress.ToString();
                    _deviceChannels.TryAdd(deviceId, ctx);
                    _logger.LogInformation($"[{DeviceTypeName}] 设备注册成功（使用远程地址），设备ID: {deviceId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理注册消息失败");
            }
        }

        protected virtual void SendRegistrationResponse(IChannelHandlerContext ctx, string deviceId)
        {
            try
            {
                string response = "OK";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                var buffer = Unpooled.WrappedBuffer(responseBytes);
                ctx.WriteAndFlushAsync(buffer);
                
                _logger.LogDebug($"[{DeviceTypeName}] 发送注册响应，设备ID: {deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 发送注册响应失败");
            }
        }

        protected virtual void HandleJsonData(byte[] msgBytes, string jsonStr, IChannelHandlerContext ctx)
        {
            try
            {
                _logger.LogInformation($"[{DeviceTypeName}] 收到JSON数据，长度: {msgBytes.Length}");

                // 尝试从JSON中提取设备ID
                string deviceId = ExtractDeviceIdFromJson(jsonStr);
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = GetDeviceIdFromContext(ctx);
                }

                var sensorData = CreateSensorData(deviceId, msgBytes, jsonStr);
                _dataQueue.Add(sensorData);
                
                _logger.LogDebug($"[{DeviceTypeName}] 数据已加入处理队列，设备ID: {deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理JSON数据失败");
            }
        }

        protected virtual void HandleRawData(byte[] msgBytes, string msgStr, IChannelHandlerContext ctx)
        {
            try
            {
                _logger.LogInformation($"[{DeviceTypeName}] 收到原始数据，长度: {msgBytes.Length}");

                string deviceId = GetDeviceIdFromContext(ctx);
                var sensorData = CreateSensorData(deviceId, msgBytes, msgStr);
                _dataQueue.Add(sensorData);
                
                _logger.LogDebug($"[{DeviceTypeName}] 数据已加入处理队列，设备ID: {deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理原始数据失败");
            }
        }

        protected virtual string ExtractDeviceId(string msgStr)
        {
            // 子类可以重写此方法实现特定的ID提取逻辑
            return string.Empty;
        }

        protected virtual string ExtractDeviceIdFromJson(string jsonStr)
        {
            try
            {
                // 尝试提取 slaveId 字段
                if (jsonStr.Contains(SLAVE_ID_FIELD))
                {
                    int startIndex = jsonStr.IndexOf(SLAVE_ID_FIELD) + SLAVE_ID_FIELD.Length;
                    int endIndex = jsonStr.IndexOf(",", startIndex);
                    if (endIndex == -1)
                    {
                        endIndex = jsonStr.IndexOf("}", startIndex);
                    }
                    
                    if (endIndex > startIndex)
                    {
                        string slaveId = jsonStr.Substring(startIndex, endIndex - startIndex).Trim().Trim('"');
                        return slaveId;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{DeviceTypeName}] 从JSON提取设备ID失败");
            }
            
            return string.Empty;
        }

        protected virtual string GetDeviceIdFromContext(IChannelHandlerContext ctx)
        {
            // 从已注册的设备中查找
            foreach (var kvp in _deviceChannels)
            {
                if (kvp.Value == ctx)
                {
                    return kvp.Key;
                }
            }
            
            // 如果未找到，使用远程地址作为临时ID
            return ctx.Channel.RemoteAddress.ToString();
        }

        protected virtual void ProcessDataQueue()
        {
            _logger.LogInformation($"[{DeviceTypeName}] 数据处理线程已启动");

            foreach (var sensorData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    string deviceId = GetDeviceId(sensorData);
                    SaveSensorData(sensorData);
                    PublishToMqtt(sensorData);
                    _lastUpdateTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{DeviceTypeName}] 处理队列数据失败");
                }
            }
        }

        protected virtual void SaveSensorData(TSensorData sensorData)
        {
            try
            {
                string deviceId = GetDeviceId(sensorData);
                DateTime now = DateTime.Now;
                string dateFolder = now.ToString("yyyyMMdd");
                string fileName = now.ToString("HHmmss") + ".json";
                
                string folderPath = Path.Combine(_dataPath, _projectId, deviceId, DeviceTypeName.ToLower(), dateFolder);
                Directory.CreateDirectory(folderPath);
                
                string filePath = Path.Combine(folderPath, fileName);
                string jsonData = JsonSerializer.Serialize(sensorData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);

                _logger.LogInformation($"[{DeviceTypeName}] 数据已保存到文件: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 保存数据失败");
            }
        }

        protected virtual void PublishToMqtt(TSensorData sensorData)
        {
            try
            {
                string deviceId = GetDeviceId(sensorData);
                string topic = $"/sensor/{DeviceTypeName.ToLower()}/{deviceId}/data";
                
                var mqttMessage = new
                {
                    DeviceId = deviceId,
                    DeviceType = DeviceTypeName,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = sensorData
                };

                string payload = JsonSerializer.Serialize(mqttMessage);
                _logger.LogDebug($"[{DeviceTypeName}] MQTT消息准备发送 - Topic: {topic}");
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

