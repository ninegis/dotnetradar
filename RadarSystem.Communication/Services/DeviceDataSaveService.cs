using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Data.TDengine;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 设备数据保存服务 - 统一处理各类设备数据到TDengine的保存
    /// </summary>
    public class DeviceDataSaveService
    {
        private readonly ITDengineRepository _tdRepository;
        private readonly ILogger<DeviceDataSaveService> _logger;
        
        public DeviceDataSaveService(
            ITDengineRepository tdRepository,
            ILogger<DeviceDataSaveService> logger)
        {
            _tdRepository = tdRepository ?? throw new ArgumentNullException(nameof(tdRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        #region 雷达数据保存
        
        /// <summary>
        /// 保存雷达数据（通用）
        /// </summary>
        public async Task SaveRadarDataAsync(
            string deviceId,
            string deviceType,
            string slaveId,
            string command,
            string imageType,
            int dataLength,
            string filePath,
            string projectId = "PROJECT001")
        {
            try
            {
                var record = new RadarDataRecord
                {
                    Timestamp = DateTime.Now,
                    DeviceId = deviceId,
                    DeviceType = deviceType,
                    SlaveId = slaveId,
                    Command = command,
                    ImageType = imageType,
                    DataLength = dataLength,
                    FilePath = filePath,
                    ProjectId = projectId
                };
                
                await _tdRepository.SaveRadarDataAsync(record);
                _logger.LogDebug("雷达数据已保存到TDengine: {DeviceId}/{DeviceType}", deviceId, deviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存雷达数据到TDengine失败: {DeviceId}", deviceId);
                // 不抛出异常，避免影响主流程
            }
        }
        
        #endregion
        
        #region GPS数据保存
        
        /// <summary>
        /// 保存GPS数据
        /// </summary>
        public async Task SaveGpsDataAsync(
            string deviceId,
            double latitude,
            double longitude,
            double altitude,
            int satellites,
            double hdop,
            string fixQuality,
            double speed = 0,
            double course = 0,
            string projectId = "PROJECT001",
            string deviceType = "GPS")
        {
            try
            {
                var record = new GpsDataRecord
                {
                    Timestamp = DateTime.Now,
                    DeviceId = deviceId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Altitude = altitude,
                    Satellites = satellites,
                    Hdop = hdop,
                    FixQuality = fixQuality,
                    Speed = speed,
                    Course = course,
                    ProjectId = projectId,
                    DeviceType = deviceType
                };
                
                await _tdRepository.SaveGpsDataAsync(record);
                _logger.LogDebug("GPS数据已保存到TDengine: {DeviceId}", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存GPS数据到TDengine失败: {DeviceId}", deviceId);
            }
        }
        
        #endregion
        
        #region 传感器数据保存
        
        /// <summary>
        /// 保存传感器数据（通用）
        /// </summary>
        public async Task SaveSensorDataAsync(
            string deviceId,
            string sensorType,
            double value1,
            double value2 = 0,
            double value3 = 0,
            float temperature = 0,
            string status = "normal",
            string rawJson = "",
            string projectId = "PROJECT001")
        {
            try
            {
                var record = new SensorDataRecord
                {
                    Timestamp = DateTime.Now,
                    DeviceId = deviceId,
                    SensorType = sensorType,
                    Value1 = value1,
                    Value2 = value2,
                    Value3 = value3,
                    Temperature = temperature,
                    Status = status,
                    RawJson = rawJson,
                    ProjectId = projectId,
                    DeviceType = sensorType
                };
                
                await _tdRepository.SaveSensorDataAsync(record);
                _logger.LogDebug("传感器数据已保存到TDengine: {DeviceId}/{SensorType}", deviceId, sensorType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存传感器数据到TDengine失败: {DeviceId}", deviceId);
            }
        }
        
        #endregion
        
        #region 电机数据保存
        
        /// <summary>
        /// 保存电机数据
        /// </summary>
        public async Task SaveMotorDataAsync(
            string deviceId,
            double azimuth,
            double elevation,
            string motorStatus,
            double positionX = 0,
            double positionY = 0,
            double positionZ = 0,
            string projectId = "PROJECT001")
        {
            try
            {
                var record = new MotorDataRecord
                {
                    Timestamp = DateTime.Now,
                    DeviceId = deviceId,
                    Azimuth = azimuth,
                    Elevation = elevation,
                    MotorStatus = motorStatus,
                    PositionX = positionX,
                    PositionY = positionY,
                    PositionZ = positionZ,
                    ProjectId = projectId,
                    DeviceType = "Motor"
                };
                
                await _tdRepository.SaveMotorDataAsync(record);
                _logger.LogDebug("电机数据已保存到TDengine: {DeviceId}", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存电机数据到TDengine失败: {DeviceId}", deviceId);
            }
        }
        
        #endregion
        
        #region 报警数据保存
        
        /// <summary>
        /// 保存报警数据
        /// </summary>
        public async Task SaveAlarmDataAsync(
            string deviceId,
            string alarmType,
            string alarmLevel,
            string alarmMessage,
            double alarmValue,
            double threshold,
            bool isResolved = false,
            string projectId = "PROJECT001")
        {
            try
            {
                var record = new AlarmDataRecord
                {
                    Timestamp = DateTime.Now,
                    DeviceId = deviceId,
                    AlarmType = alarmType,
                    AlarmLevel = alarmLevel,
                    AlarmMessage = alarmMessage,
                    AlarmValue = alarmValue,
                    Threshold = threshold,
                    IsResolved = isResolved,
                    ProjectId = projectId,
                    DeviceType = "Alarm"
                };
                
                await _tdRepository.SaveAlarmDataAsync(record);
                _logger.LogDebug("报警数据已保存到TDengine: {DeviceId}/{AlarmType}", deviceId, alarmType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存报警数据到TDengine失败: {DeviceId}", deviceId);
            }
        }
        
        #endregion
    }
}

