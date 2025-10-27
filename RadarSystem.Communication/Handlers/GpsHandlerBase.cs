using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// GPS 处理器基类
    /// 适用于所有使用 NMEA 0183 协议的 GPS/GNSS 设备
    /// </summary>
    public abstract class GpsHandlerBase<TGpsData> : SimpleChannelInboundHandler<string>
        where TGpsData : class
    {
        protected readonly ILogger _logger;
        protected readonly string _projectId;
        protected readonly string _dataPath;
        protected readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannels;
        protected readonly ConcurrentDictionary<string, DateTime> _lastUpdateTime;
        protected readonly BlockingCollection<TGpsData> _dataQueue;

        protected GpsHandlerBase(
            ILogger logger,
            string projectId,
            string dataPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectId = projectId;
            _dataPath = dataPath;
            _deviceChannels = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _lastUpdateTime = new ConcurrentDictionary<string, DateTime>();
            _dataQueue = new BlockingCollection<TGpsData>(new ConcurrentQueue<TGpsData>());

            // 启动数据处理线程
            Task.Run(() => ProcessDataQueue());
        }

        /// <summary>
        /// 获取设备类型名称（用于日志）
        /// </summary>
        protected abstract string DeviceTypeName { get; }

        /// <summary>
        /// 创建 GPS 数据对象
        /// </summary>
        protected abstract TGpsData CreateGpsData(string deviceId, NmeaData nmeaData);

        /// <summary>
        /// 获取 GPS 数据的设备ID
        /// </summary>
        protected abstract string GetDeviceId(TGpsData gpsData);

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

        protected override void ChannelRead0(IChannelHandlerContext ctx, string nmeaSentence)
        {
            try
            {
                HandleNmeaSentence(nmeaSentence, ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理 NMEA 语句时发生异常: {nmeaSentence}");
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, $"[{DeviceTypeName}] 通道异常");
            context.CloseAsync();
        }

        protected virtual void HandleNmeaSentence(string sentence, IChannelHandlerContext ctx)
        {
            if (string.IsNullOrWhiteSpace(sentence) || !sentence.StartsWith("$"))
                return;

            // 提取语句类型
            string[] parts = sentence.Split(',');
            if (parts.Length < 2)
                return;

            string sentenceType = parts[0].Substring(1); // 去掉 $

            // 根据语句类型处理
            switch (sentenceType)
            {
                case "GPGGA": // GPS Fix Data
                    HandleGPGGA(parts, ctx);
                    break;

                case "GPRMC": // Recommended Minimum data
                    HandleGPRMC(parts, ctx);
                    break;

                case "GPGSA": // GPS DOP and active satellites
                    HandleGPGSA(parts, ctx);
                    break;

                case "GPGSV": // GPS Satellites in view
                    HandleGPGSV(parts, ctx);
                    break;

                case "GPVTG": // Track made good and Ground speed
                    HandleGPVTG(parts, ctx);
                    break;

                default:
                    _logger.LogDebug($"[{DeviceTypeName}] 未处理的 NMEA 语句类型: {sentenceType}");
                    break;
            }
        }

        /// <summary>
        /// 处理 GPGGA 语句（GPS Fix Data）
        /// $GPGGA,时间,纬度,N/S,经度,E/W,定位质量,卫星数,HDOP,海拔,M,大地水准面高度,M,,*校验和
        /// </summary>
        protected virtual void HandleGPGGA(string[] parts, IChannelHandlerContext ctx)
        {
            try
            {
                if (parts.Length < 15)
                {
                    _logger.LogWarning($"[{DeviceTypeName}] GPGGA 数据不完整: {string.Join(",", parts)}");
                    return;
                }

                var nmeaData = new NmeaData
                {
                    Timestamp = DateTime.Now,
                    SentenceType = "GPGGA"
                };

                // 时间 (hhmmss.ss)
                if (!string.IsNullOrEmpty(parts[1]))
                {
                    nmeaData.TimeUtc = ParseNmeaTime(parts[1]);
                }

                // 纬度 (ddmm.mmmm)
                if (!string.IsNullOrEmpty(parts[2]) && !string.IsNullOrEmpty(parts[3]))
                {
                    nmeaData.Latitude = ParseLatitude(parts[2], parts[3]);
                }

                // 经度 (dddmm.mmmm)
                if (!string.IsNullOrEmpty(parts[4]) && !string.IsNullOrEmpty(parts[5]))
                {
                    nmeaData.Longitude = ParseLongitude(parts[4], parts[5]);
                }

                // 定位质量
                if (!string.IsNullOrEmpty(parts[6]))
                {
                    nmeaData.FixQuality = int.Parse(parts[6]);
                }

                // 卫星数量
                if (!string.IsNullOrEmpty(parts[7]))
                {
                    nmeaData.SatelliteCount = int.Parse(parts[7]);
                }

                // HDOP
                if (!string.IsNullOrEmpty(parts[8]))
                {
                    nmeaData.Hdop = float.Parse(parts[8], CultureInfo.InvariantCulture);
                }

                // 海拔高度
                if (!string.IsNullOrEmpty(parts[9]))
                {
                    nmeaData.Altitude = float.Parse(parts[9], CultureInfo.InvariantCulture);
                }

                _logger.LogInformation($"[{DeviceTypeName}] GPGGA - 纬度: {nmeaData.Latitude:F6}, 经度: {nmeaData.Longitude:F6}, 高度: {nmeaData.Altitude:F2}m, 卫星数: {nmeaData.SatelliteCount}");

                // 创建设备ID（从远程地址提取）
                string deviceId = ctx.Channel.RemoteAddress.ToString();
                var gpsData = CreateGpsData(deviceId, nmeaData);
                _dataQueue.Add(gpsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 处理 GPGGA 语句失败: {string.Join(",", parts)}");
            }
        }

        /// <summary>
        /// 处理 GPRMC 语句（Recommended Minimum data）
        /// </summary>
        protected virtual void HandleGPRMC(string[] parts, IChannelHandlerContext ctx)
        {
            // TODO: 实现 GPRMC 处理
            _logger.LogDebug($"[{DeviceTypeName}] GPRMC: {string.Join(",", parts)}");
        }

        /// <summary>
        /// 处理 GPGSA 语句（GPS DOP and active satellites）
        /// </summary>
        protected virtual void HandleGPGSA(string[] parts, IChannelHandlerContext ctx)
        {
            // TODO: 实现 GPGSA 处理
            _logger.LogDebug($"[{DeviceTypeName}] GPGSA: {string.Join(",", parts)}");
        }

        /// <summary>
        /// 处理 GPGSV 语句（GPS Satellites in view）
        /// </summary>
        protected virtual void HandleGPGSV(string[] parts, IChannelHandlerContext ctx)
        {
            // TODO: 实现 GPGSV 处理
            _logger.LogDebug($"[{DeviceTypeName}] GPGSV: {string.Join(",", parts)}");
        }

        /// <summary>
        /// 处理 GPVTG 语句（Track made good and Ground speed）
        /// </summary>
        protected virtual void HandleGPVTG(string[] parts, IChannelHandlerContext ctx)
        {
            // TODO: 实现 GPVTG 处理
            _logger.LogDebug($"[{DeviceTypeName}] GPVTG: {string.Join(",", parts)}");
        }

        /// <summary>
        /// 解析 NMEA 时间 (hhmmss.ss) 到 TimeSpan
        /// </summary>
        protected TimeSpan? ParseNmeaTime(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr) || timeStr.Length < 6)
                return null;

            int hours = int.Parse(timeStr.Substring(0, 2));
            int minutes = int.Parse(timeStr.Substring(2, 2));
            int seconds = int.Parse(timeStr.Substring(4, 2));
            int milliseconds = 0;

            if (timeStr.Length > 7 && timeStr[6] == '.')
            {
                string msStr = timeStr.Substring(7).PadRight(3, '0').Substring(0, 3);
                milliseconds = int.Parse(msStr);
            }

            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        /// 解析纬度 (ddmm.mmmm, N/S) 到十进制度
        /// </summary>
        protected double ParseLatitude(string latStr, string nsIndicator)
        {
            if (string.IsNullOrEmpty(latStr))
                return 0;

            double lat = double.Parse(latStr, CultureInfo.InvariantCulture);
            int degrees = (int)(lat / 100);
            double minutes = lat - (degrees * 100);
            double decimalDegrees = degrees + (minutes / 60.0);

            if (nsIndicator == "S")
                decimalDegrees = -decimalDegrees;

            return decimalDegrees;
        }

        /// <summary>
        /// 解析经度 (dddmm.mmmm, E/W) 到十进制度
        /// </summary>
        protected double ParseLongitude(string lonStr, string ewIndicator)
        {
            if (string.IsNullOrEmpty(lonStr))
                return 0;

            double lon = double.Parse(lonStr, CultureInfo.InvariantCulture);
            int degrees = (int)(lon / 100);
            double minutes = lon - (degrees * 100);
            double decimalDegrees = degrees + (minutes / 60.0);

            if (ewIndicator == "W")
                decimalDegrees = -decimalDegrees;

            return decimalDegrees;
        }

        protected virtual void ProcessDataQueue()
        {
            _logger.LogInformation($"[{DeviceTypeName}] 数据处理线程已启动");

            foreach (var gpsData in _dataQueue.GetConsumingEnumerable())
            {
                try
                {
                    string deviceId = GetDeviceId(gpsData);
                    SaveGpsData(gpsData);
                    PublishToMqtt(gpsData);
                    _lastUpdateTime.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{DeviceTypeName}] 处理队列数据失败");
                }
            }
        }

        protected virtual void SaveGpsData(TGpsData gpsData)
        {
            try
            {
                string deviceId = GetDeviceId(gpsData);
                DateTime now = DateTime.Now;
                string dateFolder = now.ToString("yyyyMMdd");
                string fileName = now.ToString("HHmmss") + ".json";
                
                string folderPath = Path.Combine(_dataPath, _projectId, deviceId, "gps", dateFolder);
                Directory.CreateDirectory(folderPath);
                
                string filePath = Path.Combine(folderPath, fileName);
                string jsonData = JsonSerializer.Serialize(gpsData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);

                _logger.LogInformation($"[{DeviceTypeName}] GPS 数据已保存到文件: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DeviceTypeName}] 保存 GPS 数据失败");
            }
        }

        protected virtual void PublishToMqtt(TGpsData gpsData)
        {
            try
            {
                string deviceId = GetDeviceId(gpsData);
                string topic = $"/gps/{DeviceTypeName.ToLower()}/{deviceId}/data";
                
                var mqttMessage = new
                {
                    DeviceId = deviceId,
                    DeviceType = DeviceTypeName,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = gpsData
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

    /// <summary>
    /// NMEA 数据结构
    /// </summary>
    public class NmeaData
    {
        public DateTime Timestamp { get; set; }
        public string SentenceType { get; set; } = string.Empty;
        public TimeSpan? TimeUtc { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float Altitude { get; set; }
        public int FixQuality { get; set; }
        public int SatelliteCount { get; set; }
        public float Hdop { get; set; }
        public float Speed { get; set; }
        public float Direction { get; set; }
    }
}

