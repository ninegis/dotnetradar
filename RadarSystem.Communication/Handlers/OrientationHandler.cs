using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 方向传感器数据处理器
    /// 端口: 11128
    /// 协议: JSON/文本协议
    /// </summary>
    public class OrientationHandler : SensorHandlerBase<OrientationData>
    {
        public OrientationHandler(
            ILogger<OrientationHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Orientation";

        protected override OrientationData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new OrientationData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData,
                // TODO: 解析JSON数据提取 Yaw, Pitch, Roll
                // 需要根据实际JSON格式实现
            };
        }

        protected override string GetDeviceId(OrientationData sensorData) => sensorData.DeviceId;
    }
}

