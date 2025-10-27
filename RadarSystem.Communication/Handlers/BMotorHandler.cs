using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// B型电机数据处理器
    /// 端口: 11115
    /// </summary>
    public class BMotorHandler : MotorHandlerBase
    {
        public BMotorHandler(ILogger<BMotorHandler> logger, string projectId, string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "BMotor";
    }
}

