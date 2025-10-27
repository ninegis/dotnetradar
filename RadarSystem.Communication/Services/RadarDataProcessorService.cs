using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RadarSystem.Communication.Interfaces;
using RadarSystem.Communication.Models;
using RadarSystem.Communication.Utilities;
using RadarSystem.Data.TDengine;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 雷达数据处理服务 - 后台服务
    /// 不验证设备是否在数据库配置，只要数据完整就处理
    /// 参考Java实现：RadarConsumerThread
    /// </summary>
    public class RadarDataProcessorService : BackgroundService
    {
        private readonly BlockingCollection<RadarDataPacket> _dataQueue;
        private readonly IRadarFileStorage _fileStorage;
        private readonly ITDengineRepository _tdRepository;
        private readonly RadarDataValidator _validator;
        private readonly MqttService _mqttService;
        private readonly ILogger<RadarDataProcessorService> _logger;
        private readonly IConfiguration _configuration;
        
        // 统计指标
        private long _totalReceived = 0;
        private long _totalProcessed = 0;
        private long _md5Failed = 0;
        private long _saveFailed = 0;

        public RadarDataProcessorService(
            IRadarFileStorage fileStorage,
            ITDengineRepository tdRepository,
            RadarDataValidator validator,
            MqttService mqttService,
            ILogger<RadarDataProcessorService> logger,
            IConfiguration configuration)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _tdRepository = tdRepository ?? throw new ArgumentNullException(nameof(tdRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // 创建阻塞队列，设置最大容量
            int maxQueueSize = _configuration.GetValue<int>("RadarDataReceiver:QueueMaxSize", 10000);
            _dataQueue = new BlockingCollection<RadarDataPacket>(maxQueueSize);
            
            _logger.LogInformation("雷达数据处理服务初始化完成，队列容量: {MaxSize}", maxQueueSize);
        }

        /// <summary>
        /// 接收雷达数据（外部调用）
        /// 注意：不验证设备是否在数据库配置
        /// </summary>
        public bool ReceiveData(RadarDataPacket packet)
        {
            try
            {
                Interlocked.Increment(ref _totalReceived);
                
                // 尝试添加到队列
                bool added = _dataQueue.TryAdd(packet, TimeSpan.FromSeconds(5));
                
                if (!added)
                {
                    _logger.LogWarning("队列已满，数据被丢弃: {Packet}", packet);
                    return false;
                }
                
                _logger.LogDebug("数据已入队: {Packet}, QueueSize={QueueSize}", 
                    packet, _dataQueue.Count);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收数据失败: {Packet}", packet);
                return false;
            }
        }

        /// <summary>
        /// 后台处理队列
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("雷达数据处理服务已启动");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 从队列中取出数据（阻塞等待）
                    if (_dataQueue.TryTake(out var packet, 100, stoppingToken))
                    {
                        await ProcessDataPacketAsync(packet);
                    }
                    
                    // 每处理1000个数据包输出一次统计信息
                    if (_totalProcessed % 1000 == 0 && _totalProcessed > 0)
                    {
                        LogStatistics();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("收到停止信号，正在退出处理循环");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理数据队列时发生异常");
                    await Task.Delay(1000, stoppingToken);  // 出错后等待1秒
                }
            }
            
            _logger.LogInformation("雷达数据处理服务已停止");
        }

        /// <summary>
        /// 处理单个数据包
        /// </summary>
        private async Task ProcessDataPacketAsync(RadarDataPacket packet)
        {
            try
            {
                _logger.LogDebug("开始处理数据包: {Packet}", packet);
                
                // 步骤1: MD5校验（如果配置启用）
                bool enableMD5Check = _configuration.GetValue<bool>("RadarDataReceiver:EnableMD5Check", true);
                if (enableMD5Check)
                {
                    if (!_validator.ValidateMD5(packet))
                    {
                        Interlocked.Increment(ref _md5Failed);
                        _logger.LogWarning("数据MD5校验失败，已丢弃: {Packet}", packet);
                        return;
                    }
                }
                
                // 步骤2: 生成文件路径（按设备ID和日期）
                string filePath = _fileStorage.GenerateFilePath(packet);
                
                // 步骤3: 保存文件
                await _fileStorage.SaveRadarDataAsync(filePath, packet.RawData);
                
                // 步骤4: 保存到TDengine（如果配置启用）
                bool saveToTDengine = _configuration.GetValue<bool>("RadarDataReceiver:SaveToTDengine", true);
                if (saveToTDengine)
                {
                    await SaveToTDengineAsync(packet, filePath);
                }
                
                // 步骤5: 发送MQTT通知（如果配置启用）
                bool sendMqttNotification = _configuration.GetValue<bool>("RadarDataReceiver:SendMqttNotification", true);
                if (sendMqttNotification)
                {
                    await SendMqttNotificationAsync(packet, filePath);
                }
                
                Interlocked.Increment(ref _totalProcessed);
                
                _logger.LogInformation("数据包处理完成: {FilePath}, Size={Size} bytes", 
                    filePath, packet.DataLength);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _saveFailed);
                _logger.LogError(ex, "处理数据包失败: {Packet}", packet);
            }
        }

        /// <summary>
        /// 保存到TDengine
        /// </summary>
        private async Task SaveToTDengineAsync(RadarDataPacket packet, string filePath)
        {
            try
            {
                var record = new RadarDataRecord
                {
                    Timestamp = packet.ReceiveTime,
                    DeviceId = packet.GetDeviceIdentifier(),
                    DeviceType = "ArcRadar",
                    SlaveId = packet.SlaveId,
                    Command = packet.Command,
                    ImageType = packet.DataType,
                    DataLength = packet.DataLength,
                    FilePath = filePath,
                    ProjectId = packet.ProjectId,
                    RawData = Array.Empty<byte>()  // 原始数据太大，不存储
                };
                
                await _tdRepository.SaveRadarDataAsync(record);
                
                _logger.LogDebug("数据已保存到TDengine: DeviceId={DeviceId}, FilePath={FilePath}", 
                    packet.GetDeviceIdentifier(), filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存到TDengine失败: {Packet}", packet);
                // 不抛出异常，避免影响主流程
            }
        }

        /// <summary>
        /// 发送MQTT通知
        /// </summary>
        private async Task SendMqttNotificationAsync(RadarDataPacket packet, string filePath)
        {
            try
            {
                var notification = new
                {
                    time = packet.ReceiveTime.ToString("yyyyMMddHHmmss.fff"),
                    type = packet.DataType,
                    router = (string?)null,
                    ipv4 = packet.RemoteAddress,
                    image = filePath,
                    deviceId = packet.GetDeviceIdentifier(),
                    slaveId = packet.SlaveId,
                    projectId = packet.ProjectId,
                    dataLength = packet.DataLength
                };
                
                string topic = packet.DataType switch
                {
                    "06" => "/dev/radar/mimo/defo/image",
                    _ => "/dev/radar/defo/image"
                };
                
                string message = JsonConvert.SerializeObject(notification);
                
                await _mqttService.PublishAsync(topic, message);
                
                _logger.LogDebug("MQTT通知已发送: Topic={Topic}, DeviceId={DeviceId}", 
                    topic, packet.GetDeviceIdentifier());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送MQTT通知失败: {Packet}", packet);
                // 不抛出异常，避免影响主流程
            }
        }

        /// <summary>
        /// 输出统计信息
        /// </summary>
        private void LogStatistics()
        {
            _logger.LogInformation("数据处理统计: 接收={Received}, 处理={Processed}, " +
                                  "MD5失败={MD5Failed}, 保存失败={SaveFailed}, 队列={QueueSize}",
                _totalReceived, _totalProcessed, _md5Failed, _saveFailed, _dataQueue.Count);
        }

        /// <summary>
        /// 获取当前队列大小
        /// </summary>
        public int GetQueueSize() => _dataQueue.Count;

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public (long Received, long Processed, long MD5Failed, long SaveFailed) GetStatistics()
        {
            return (_totalReceived, _totalProcessed, _md5Failed, _saveFailed);
        }

        public override void Dispose()
        {
            _dataQueue?.Dispose();
            LogStatistics();
            base.Dispose();
        }
    }
}

