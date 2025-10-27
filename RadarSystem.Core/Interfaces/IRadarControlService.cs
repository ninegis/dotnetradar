namespace RadarSystem.Core.Interfaces
{
    public interface IRadarControlService
    {
        Task ControlRadarAsync(object request);
        Task SetParamControlAsync(object request);
        Task SetMimoLiteParamControlAsync(object request);
        Task UpdateTiltMotorPitchAsync(object request);
    }
}

