using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 俯仰电机数据处理器
    /// 端口: 11127
    /// </summary>
    public class MotorPitchHandler : MotorHandlerBase
    {
        public MotorPitchHandler(ILogger<MotorPitchHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "MotorPitch";
    }
}

