using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 激光设备数据处理器
    /// 端口: 11131
    /// 协议: JSON/文本协议
    /// </summary>
    public class LaserHandler : SensorHandlerBase<LaserData>
    {
        public LaserHandler(
            ILogger<LaserHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Laser";

        protected override LaserData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new LaserData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData,
                // TODO: 解析JSON数据提取距离、强度等
                // 需要根据实际JSON格式实现
            };
        }

        protected override string GetDeviceId(LaserData sensorData) => sensorData.DeviceId;
    }
}

