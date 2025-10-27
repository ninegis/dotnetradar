using System;
using System.Collections.Generic;

namespace RadarSystem.Communication.Models
{
    /// <summary>
    /// GPS V1 设备数据模型
    /// </summary>
    public class GpsV1Data
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public float Speed { get; set; }
        public float Direction { get; set; }
        public int SatelliteCount { get; set; }
        public string GpsStatus { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 北纬 V1 设备数据模型
    /// </summary>
    public class BwV1Data
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public float Accuracy { get; set; }
        public string PositionMode { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// GPS 设备数据模型
    /// </summary>
    public class GpsData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public float Pdop { get; set; }
        public float Hdop { get; set; }
        public float Vdop { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// MIMO Lite 雷达数据模型
    /// </summary>
    public class MimoLiteRadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CommandType { get; set; } = string.Empty; // 0000=心跳, 1000=时间同步, 0302=形变图, 0301=散斑图, etc.
        public string ImageType { get; set; } = string.Empty; // 00=形变, 61=散斑, 02=相干, 06=动目标, 63=监测点
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public int DataLength { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// MIMO 雷达数据模型
    /// </summary>
    public class MimoRadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CommandType { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public int DataLength { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 建筑物雷达数据模型
    /// </summary>
    public class BuildingRadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CommandType { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public int DataLength { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 建筑物 2D 雷达数据模型
    /// </summary>
    public class Building2DRadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CommandType { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public int DataLength { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 报警设备数据模型
    /// </summary>
    public class AlarmDeviceData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string AlarmType { get; set; } = string.Empty;
        public string AlarmLevel { get; set; } = string.Empty;
        public string AlarmMessage { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 电机控制数据模型
    /// </summary>
    public class MotorData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float Azimuth { get; set; } // 方位角
        public float Elevation { get; set; } // 俯仰角
        public string MotorStatus { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 倾斜仪数据模型
    /// </summary>
    public class InclinometerData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float AngleX { get; set; }
        public float AngleY { get; set; }
        public float Temperature { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 振动传感器数据模型
    /// </summary>
    public class VibrationData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }
        public float AccelerationZ { get; set; }
        public float Frequency { get; set; }
        public float Amplitude { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 激光设备数据模型
    /// </summary>
    public class LaserData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float Distance { get; set; }
        public float Intensity { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 方向传感器数据模型
    /// </summary>
    public class OrientationData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 交通雷达数据模型
    /// </summary>
    public class TrafficRadarData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int VehicleCount { get; set; }
        public float AverageSpeed { get; set; }
        public List<VehicleInfo> Vehicles { get; set; } = new List<VehicleInfo>();
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// 车辆信息
    /// </summary>
    public class VehicleInfo
    {
        public int VehicleId { get; set; }
        public float Speed { get; set; }
        public float Distance { get; set; }
        public string Direction { get; set; } = string.Empty;
    }

    /// <summary>
    /// 心跳数据模型
    /// </summary>
    public class HeartbeatData
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SlaveId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public int HeartbeatInterval { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }
}
