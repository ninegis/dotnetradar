using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using System;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// GPS V1 设备数据处理器
    /// 端口: 11109
    /// 协议: NMEA 0183
    /// </summary>
    public class GpsV1Handler : GpsHandlerBase<GpsV1Data>
    {
        public GpsV1Handler(
            ILogger<GpsV1Handler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "GPS V1";

        protected override GpsV1Data CreateGpsData(string deviceId, NmeaData nmeaData)
        {
            return new GpsV1Data
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = nmeaData.Timestamp,
                Latitude = nmeaData.Latitude,
                Longitude = nmeaData.Longitude,
                Altitude = nmeaData.Altitude,
                Speed = nmeaData.Speed,
                Direction = nmeaData.Direction,
                SatelliteCount = nmeaData.SatelliteCount,
                GpsStatus = GetGpsStatusString(nmeaData.FixQuality),
                RawData = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(nmeaData))
            };
        }

        protected override string GetDeviceId(GpsV1Data gpsData) => gpsData.DeviceId;

        private string GetGpsStatusString(int fixQuality)
        {
            return fixQuality switch
            {
                0 => "Invalid",
                1 => "GPS Fix",
                2 => "DGPS Fix",
                3 => "PPS Fix",
                4 => "Real Time Kinematic",
                5 => "Float RTK",
                6 => "Estimated",
                7 => "Manual Input",
                8 => "Simulation",
                _ => "Unknown"
            };
        }
    }
}

