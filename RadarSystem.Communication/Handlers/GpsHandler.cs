using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using System;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// GPS 设备数据处理器
    /// 端口: 11111
    /// 协议: NMEA 0183
    /// </summary>
    public class GpsHandler : GpsHandlerBase<GpsData>
    {
        public GpsHandler(
            ILogger<GpsHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "GPS";

        protected override GpsData CreateGpsData(string deviceId, NmeaData nmeaData)
        {
            return new GpsData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = nmeaData.Timestamp,
                Latitude = nmeaData.Latitude,
                Longitude = nmeaData.Longitude,
                Altitude = nmeaData.Altitude,
                Pdop = 0, // TODO: 从 GPGSA 中提取
                Hdop = nmeaData.Hdop,
                Vdop = 0, // TODO: 从 GPGSA 中提取
                RawData = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(nmeaData))
            };
        }

        protected override string GetDeviceId(GpsData gpsData) => gpsData.DeviceId;
    }
}

