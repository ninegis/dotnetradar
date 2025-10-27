using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// CM 设备数据处理器
    /// 端口: 11124
    /// 协议: JSON/文本协议
    /// </summary>
    public class CmHandler : SensorHandlerBase<InclinometerData>
    {
        public CmHandler(
            ILogger<CmHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "CM";

        protected override InclinometerData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new InclinometerData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData,
                // TODO: 解析JSON数据
                // 需要根据实际JSON格式实现
            };
        }

        protected override string GetDeviceId(InclinometerData sensorData) => sensorData.DeviceId;
    }
}

