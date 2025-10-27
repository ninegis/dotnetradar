using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 报警设备处理器 - 端口: 11113
    /// </summary>
    public class AlarmHandler : AlarmHandlerBase
    {
        public AlarmHandler(ILogger<AlarmHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath) { }

        protected override string DeviceTypeName => "Alarm";
    }

    /// <summary>
    /// 报警设备通用处理器 - 端口: 11130
    /// </summary>
    public class AlarmDeviceHandler : AlarmHandlerBase
    {
        public AlarmDeviceHandler(ILogger<AlarmDeviceHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath) { }

        protected override string DeviceTypeName => "AlarmDevice";
    }

    /// <summary>
    /// 4G报警设备处理器 - 端口: 11132
    /// </summary>
    public class AlarmDevice4GHandler : AlarmHandlerBase
    {
        public AlarmDevice4GHandler(ILogger<AlarmDevice4GHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath) { }

        protected override string DeviceTypeName => "AlarmDevice4G";
    }

    /// <summary>
    /// 交通雷达处理器 - 端口: 11133
    /// </summary>
    public class TrafficRadarHandler : SensorHandlerBase<RadarSystem.Communication.Models.TrafficRadarData>
    {
        public TrafficRadarHandler(ILogger<TrafficRadarHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath) { }

        protected override string DeviceTypeName => "TrafficRadar";

        protected override RadarSystem.Communication.Models.TrafficRadarData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new RadarSystem.Communication.Models.TrafficRadarData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData
            };
        }

        protected override string GetDeviceId(RadarSystem.Communication.Models.TrafficRadarData sensorData) => sensorData.DeviceId;
    }

    /// <summary>
    /// 测斜计处理器 - 端口: 11134
    /// </summary>
    public class InclinometerHandler : SensorHandlerBase<RadarSystem.Communication.Models.InclinometerData>
    {
        public InclinometerHandler(ILogger<InclinometerHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath) { }

        protected override string DeviceTypeName => "Inclinometer";

        protected override RadarSystem.Communication.Models.InclinometerData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new RadarSystem.Communication.Models.InclinometerData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData
            };
        }

        protected override string GetDeviceId(RadarSystem.Communication.Models.InclinometerData sensorData) => sensorData.DeviceId;
    }
}

