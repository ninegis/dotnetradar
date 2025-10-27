using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 倾斜仪（Qxz）数据处理器
    /// 端口: 11126
    /// 协议: JSON/文本协议
    /// </summary>
    public class QxzHandler : SensorHandlerBase<InclinometerData>
    {
        public QxzHandler(
            ILogger<QxzHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Qxz";

        protected override InclinometerData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new InclinometerData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData,
                // TODO: 解析JSON数据提取 AngleX, AngleY, Temperature
                // 需要根据实际JSON格式实现
            };
        }

        protected override string GetDeviceId(InclinometerData sensorData) => sensorData.DeviceId;
    }
}

