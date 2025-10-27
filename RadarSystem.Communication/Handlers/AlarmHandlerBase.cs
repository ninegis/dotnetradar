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
    /// 报警设备处理器基类
    /// </summary>
    public abstract class AlarmHandlerBase : SimpleChannelInboundHandler<byte[]>
    {
        protected readonly ILogger _logger;
        protected readonly string _projectId;
        protected readonly string _dataPath;
        protected readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        protected readonly ConcurrentDictionary<string, DateTime> _lastUpdateTime;
        protected readonly BlockingCollection<AlarmDeviceData> _dataQueue;

        protected AlarmHandlerBase(ILogger logger, string projectId, string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastUpdateTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<AlarmDeviceData>(new ConcurrentQueue<AlarmDeviceData>());
            Task.Run(() => ProcessDataQueue());
        }

        protected abstract string DeviceTypeName { get; }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogInformation($"[{DeviceTypeName}] 连接建立，远程地址: {context.Channel.RemoteAddress}");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation($"[{DeviceTypeName}] 连接断开，远程地址: {context.Channel.RemoteAddress}");
            base.ChannelInactive(context);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, byte[] msg)
        {
            try
            {
                string msgStr = Encoding.UTF8.GetString(msg);
                _logger.LogInformation($"[{DeviceTypeName}] 收到报警数据: {msgStr}");

                string deviceId = ctx.Channel.RemoteAddress.ToString();
                var alarmData = new AlarmDeviceData
                {
                    DeviceId = deviceId,
                    SlaveId = deviceId,
                    Timestamp = DateTime.Now,
                    RawData = msg,
                    AlarmType = "Unknown",
                    AlarmLevel = "Info",
                    AlarmMessage = msgStr
                };

                _dataQueue.Add(alarmData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理数据失败");
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, $"[{DeviceTypeName}] 通道异常");
            context.CloseAsync();
        }

        protected virtual void ProcessDataQueue()
        {
            _logger.LogInformation($"[{DeviceTypeName}] 数据处理线程已启动");
            foreach (var alarmData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    SaveAlarmData(alarmData);
                    PublishToMqtt(alarmData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{DeviceTypeName}] 处理队列数据失败");
                }
            }
        }

        protected virtual void SaveAlarmData(AlarmDeviceData alarmData)
        {
            try
            {
                DateTime now = DateTime.Now;
                string dateFolder = now.ToString("yyyyMMdd");
                string fileName = now.ToString("HHmmss") + ".json";
                string folderPath = Path.Combine(_dataPath, _projectId, alarmData.DeviceId, "alarm", dateFolder);
                Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, fileName);
                string jsonData = JsonSerializer.Serialize(alarmData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);
                _logger.LogInformation($"[{DeviceTypeName}] 数据已保存: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 保存数据失败");
            }
        }

        protected virtual void PublishToMqtt(AlarmDeviceData alarmData)
        {
            try
            {
                string topic = $"/alarm/{DeviceTypeName.ToLower()}/{alarmData.DeviceId}/data";
                var mqttMessage = new
                {
                    DeviceId = alarmData.DeviceId,
                    DeviceType = DeviceTypeName,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = alarmData
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

