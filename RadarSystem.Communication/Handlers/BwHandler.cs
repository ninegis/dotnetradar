using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using RadarSystem.Communication.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 北纬设备数据处理器
    /// 端口: 11112
    /// 协议: 自定义文本协议
    /// </summary>
    public class BwHandler : SimpleChannelInboundHandler<byte[]>
    {
        private readonly ILogger<BwHandler> _logger;
        private readonly string _projectId;
        private readonly string _dataPath;
        private readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        private readonly ConcurrentDictionary<string, DateTime> _lastHeartbeatTime;
        private readonly BlockingCollection<BwV1Data> _dataQueue;

        private const string REGIST_MSG = "registMsg";

        public BwHandler(
            ILogger<BwHandler> logger,
            string projectId,
            string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastHeartbeatTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<BwV1Data>(new ConcurrentQueue<BwV1Data>());

            // 启动数据处理线程
            Task.Run(() => ProcessDataQueue());
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var remoteAddress = context.Channel.RemoteAddress;
            _logger.LogInformation($"[北纬] 连接建立，远程地址: {remoteAddress}");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            var remoteAddress = context.Channel.RemoteAddress;
            _logger.LogInformation($"[北纬] 连接断开，远程地址: {remoteAddress}");
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
                _logger.LogError(ex, "[北纬] 处理数据时发生异常");
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "[北纬] 通道异常");
            context.CloseAsync();
        }

        private void HandleData(byte[] msgBytes, IChannelHandlerContext ctx)
        {
            try
            {
                string msgStr = Encoding.UTF8.GetString(msgBytes);
                _logger.LogDebug($"[北纬] 接收数据: {msgStr}");

                if (msgStr.Contains(REGIST_MSG))
                {
                    HandleRegistration(msgStr, ctx);
                }
                else
                {
                    HandlePositionData(msgBytes, msgStr, ctx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[北纬] 处理数据失败");
            }
        }

        private void HandleRegistration(string msgStr, IChannelHandlerContext ctx)
        {
            try
            {
                _logger.LogInformation("[北纬] 收到设备注册消息");
                
                string slaveIdHex = msgStr.Replace(REGIST_MSG, "").Trim();
                
                if (!string.IsNullOrEmpty(slaveIdHex))
                {
                    int slaveIdDecimal = Convert.ToInt32(slaveIdHex, 16);
                    string deviceId = slaveIdDecimal.ToString();
                    
                    _deviceChannels.TryAdd(deviceId, ctx);
                    _logger.LogInformation($"[北纬] 设备注册成功，SlaveId: {deviceId} (Hex: {slaveIdHex})");
                    
                    SendRegistrationResponse(ctx, deviceId);
                }
                else
                {
                    _logger.LogWarning("[北纬] 注册消息中未找到 SlaveId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[北纬] 处理注册消息失败");
            }
        }

        private void SendRegistrationResponse(IChannelHandlerContext ctx, string deviceId)
        {
            try
            {
                string response = "OK";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                var buffer = Unpooled.WrappedBuffer(responseBytes);
                ctx.WriteAndFlushAsync(buffer);
                
                _logger.LogDebug($"[北纬] 发送注册响应，设备ID: {deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[北纬] 发送注册响应失败");
            }
        }

        private void HandlePositionData(byte[] msgBytes, string msgStr, IChannelHandlerContext ctx)
        {
            try
            {
                string hexString = ByteUtil.Bytes2Str(msgBytes).ToUpper();
                _logger.LogInformation($"[北纬] 收到定位数据，长度: {msgBytes.Length}");

                var bwData = new BwV1Data
                {
                    DeviceId = GetDeviceIdFromContext(ctx),
                    SlaveId = GetDeviceIdFromContext(ctx),
                    Timestamp = DateTime.Now,
                    RawData = msgBytes
                };

                _dataQueue.Add(bwData);
                _logger.LogDebug($"[北纬] 定位数据已加入处理队列");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[北纬] 处理定位数据失败");
            }
        }

        private string GetDeviceIdFromContext(IChannelHandlerContext ctx)
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

        private void ProcessDataQueue()
        {
            _logger.LogInformation("[北纬] 数据处理线程已启动");

            foreach (var bwData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    SaveBwData(bwData);
                    PublishToMqtt(bwData);
                    _lastHeartbeatTime.AddOrUpdate(bwData.DeviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[北纬] 处理队列数据失败");
                }
            }
        }

        private void SaveBwData(BwV1Data bwData)
        {
            try
            {
                DateTime now = DateTime.Now;
                string dateFolder = now.ToString("yyyyMMdd");
                string fileName = now.ToString("HHmmss") + ".json";
                
                string folderPath = Path.Combine(_dataPath, _projectId, bwData.DeviceId, "bw", dateFolder);
                Directory.CreateDirectory(folderPath);
                
                string filePath = Path.Combine(folderPath, fileName);
                string jsonData = JsonSerializer.Serialize(bwData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);

                _logger.LogInformation($"[北纬] 数据已保存到文件: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[北纬] 保存数据失败");
            }
        }

        private void PublishToMqtt(BwV1Data bwData)
        {
            try
            {
                string topic = $"/bw/{bwData.DeviceId}/data";
                
                var mqttMessage = new
                {
                    DeviceId = bwData.DeviceId,
                    DeviceType = "BW",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = bwData
                };

                string payload = JsonSerializer.Serialize(mqttMessage);
                _logger.LogDebug($"[北纬] MQTT消息准备发送 - Topic: {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[北纬] 发送MQTT消息失败");
            }
        }

        public void Dispose()
        {
            _dataQueue?.Dispose();
        }
    }
}

