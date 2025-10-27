using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 电机处理器基类
    /// 适用于电机、B型电机、俯仰电机等控制设备
    /// </summary>
    public abstract class MotorHandlerBase : SimpleChannelInboundHandler<byte[]>
    {
        protected readonly ILogger _logger;
        protected readonly string _projectId;
        protected readonly string _dataPath;
        protected readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        protected readonly ConcurrentDictionary<string, DateTime> _lastUpdateTime;
        protected readonly BlockingCollection<MotorData> _dataQueue;

        protected const string REGIST_MSG = "registMsg";

        protected MotorHandlerBase(
            ILogger logger,
            string projectId,
            string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastUpdateTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<MotorData>(new ConcurrentQueue<MotorData>());

            // 启动数据处理线程
            Task.Run(() => ProcessDataQueue());
        }

        /// <summary>
        /// 获取设备类型名称
        /// </summary>
        protected abstract string DeviceTypeName { get; }

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
                string msgStr = Encoding.UTF8.GetString(msgBytes);
                _logger.LogDebug($"[{DeviceTypeName}] 接收数据: {msgStr}");

                if (msgStr.Contains(REGIST_MSG) || msgStr.Equals(REGIST_MSG))
                {
                    HandleRegistration(msgStr, ctx);
                }
                else
                {
                    HandleMotorData(msgBytes, msgStr, ctx);
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
                
                string deviceId = ctx.Channel.RemoteAddress.ToString();
                _deviceChannels.TryAdd(deviceId, ctx);
                _logger.LogInformation($"[{DeviceTypeName}] 设备注册成功，设备ID: {deviceId}");
                
                SendRegistrationResponse(ctx, deviceId);
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

        protected virtual void HandleMotorData(byte[] msgBytes, string msgStr, IChannelHandlerContext ctx)
        {
            try
            {
                _logger.LogInformation($"[{DeviceTypeName}] 收到电机数据，长度: {msgBytes.Length}");

                string deviceId = GetDeviceIdFromContext(ctx);
                var motorData = new MotorData
                {
                    DeviceId = deviceId,
                    SlaveId = deviceId,
                    Timestamp = DateTime.Now,
                    RawData = msgBytes,
                    MotorStatus = "Active"
                    // TODO: 解析方位角、俯仰角等数据
                };

                _dataQueue.Add(motorData);
                _logger.LogDebug($"[{DeviceTypeName}] 数据已加入处理队列");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理电机数据失败");
            }
        }

        protected virtual string GetDeviceIdFromContext(IChannelHandlerContext ctx)
        {
            foreach (var kvp in _deviceChannels)
            {
                if (kvp.Value == ctx)
                {
                    return kvp.Key;
                }
            }
            
            return ctx.Channel.RemoteAddress.ToString();
        }

        protected virtual void ProcessDataQueue()
        {
            _logger.LogInformation($"[{DeviceTypeName}] 数据处理线程已启动");

            foreach (var motorData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    SaveMotorData(motorData);
                    PublishToMqtt(motorData);
                    _lastUpdateTime.AddOrUpdate(motorData.DeviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{DeviceTypeName}] 处理队列数据失败");
                }
            }
        }

        protected virtual void SaveMotorData(MotorData motorData)
        {
            try
            {
                DateTime now = DateTime.Now;
                string dateFolder = now.ToString("yyyyMMdd");
                string fileName = now.ToString("HHmmss") + ".json";
                
                string folderPath = Path.Combine(_dataPath, _projectId, motorData.DeviceId, "motor", dateFolder);
                Directory.CreateDirectory(folderPath);
                
                string filePath = Path.Combine(folderPath, fileName);
                string jsonData = JsonSerializer.Serialize(motorData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);

                _logger.LogInformation($"[{DeviceTypeName}] 数据已保存到文件: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 保存数据失败");
            }
        }

        protected virtual void PublishToMqtt(MotorData motorData)
        {
            try
            {
                string topic = $"/motor/{DeviceTypeName.ToLower()}/{motorData.DeviceId}/data";
                
                var mqttMessage = new
                {
                    DeviceId = motorData.DeviceId,
                    DeviceType = DeviceTypeName,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = motorData
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

