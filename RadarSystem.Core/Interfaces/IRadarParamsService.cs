namespace RadarSystem.Core.Interfaces
{
    public interface IRadarParamsService
    {
        Task UpdateRadarParamAsync(object request);
        Task UpdateMimoLiteParamAsync(object request);
        Task UpdateAlgoParamAsync(object request);
        Task UpdateMimoLiteAlgoParamAsync(object request);
        Task UpdateSpeedTargetAsync(object request);
        Task UpdateColorBarAsync(object request);
        Task UpdateHiddenAnalysisAsync(object request);
    }
}

