using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RadarSystem.Data.TDengine
{
    /// <summary>
    /// TDengine 数据仓库接口
    /// </summary>
    public interface ITDengineRepository
    {
        // 雷达数据
        Task SaveRadarDataAsync(RadarDataRecord record);
        Task<List<RadarDataRecord>> QueryRadarDataAsync(string deviceId, DateTime startTime, DateTime endTime);

        // GPS 数据
        Task SaveGpsDataAsync(GpsDataRecord record);
        Task<List<GpsDataRecord>> QueryGpsDataAsync(string deviceId, DateTime startTime, DateTime endTime);

        // 传感器数据
        Task SaveSensorDataAsync(SensorDataRecord record);
        Task<List<SensorDataRecord>> QuerySensorDataAsync(string deviceId, DateTime startTime, DateTime endTime);

        // 电机数据
        Task SaveMotorDataAsync(MotorDataRecord record);
        Task<List<MotorDataRecord>> QueryMotorDataAsync(string deviceId, DateTime startTime, DateTime endTime);

        // 报警数据
        Task SaveAlarmDataAsync(AlarmDataRecord record);
        Task<List<AlarmDataRecord>> QueryAlarmDataAsync(string deviceId, DateTime startTime, DateTime endTime);

        // 批量操作
        Task SaveBatchAsync<T>(List<T> records) where T : class;

        // 统计查询
        Task<long> GetRecordCountAsync(string tableName, DateTime startTime, DateTime endTime);
        Task<Dictionary<string, object>> GetStatisticsAsync(string tableName, string deviceId, DateTime startTime, DateTime endTime);
    }

    // 数据记录基类
    public abstract class DataRecordBase
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }

    // 雷达数据记录
    public class RadarDataRecord : DataRecordBase
    {
        public string SlaveId { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public int DataLength { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    // GPS 数据记录
    public class GpsDataRecord : DataRecordBase
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public int Satellites { get; set; }
        public double Hdop { get; set; }
        public string FixQuality { get; set; } = string.Empty;
        public double Speed { get; set; }
        public double Course { get; set; }
    }

    // 传感器数据记录
    public class SensorDataRecord : DataRecordBase
    {
        public string SensorType { get; set; } = string.Empty;
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double Value3 { get; set; }
        public float Temperature { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RawJson { get; set; } = string.Empty;
    }

    // 电机数据记录
    public class MotorDataRecord : DataRecordBase
    {
        public double Azimuth { get; set; }
        public double Elevation { get; set; }
        public string MotorStatus { get; set; } = string.Empty;
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
    }

    // 报警数据记录
    public class AlarmDataRecord : DataRecordBase
    {
        public string AlarmType { get; set; } = string.Empty;
        public string AlarmLevel { get; set; } = string.Empty;
        public string AlarmMessage { get; set; } = string.Empty;
        public double AlarmValue { get; set; }
        public double Threshold { get; set; }
        public bool IsResolved { get; set; }
    }
}

