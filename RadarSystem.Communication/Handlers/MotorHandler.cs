using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 电机数据处理器
    /// 端口: 11114
    /// </summary>
    public class MotorHandler : MotorHandlerBase
    {
        public MotorHandler(ILogger<MotorHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Motor";
    }
}

