using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 振动传感器数据处理器
    /// 端口: 9993
    /// 协议: JSON/文本协议
    /// </summary>
    public class VibrationHandler : SensorHandlerBase<VibrationData>
    {
        public VibrationHandler(
            ILogger<VibrationHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Vibration";

        protected override VibrationData CreateSensorData(string deviceId, byte[] rawData, string jsonData)
        {
            return new VibrationData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = System.DateTime.Now,
                RawData = rawData,
                // TODO: 解析JSON数据提取加速度、频率、振幅等
                // 需要根据实际JSON格式实现
            };
        }

        protected override string GetDeviceId(VibrationData sensorData) => sensorData.DeviceId;
    }
}

