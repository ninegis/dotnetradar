namespace RadarSystem.Core.Interfaces
{
    public interface ISystemLogService
    {
        Task<object> GetAddressByIpAsync(string ipAddress);
        Task AddRadarLogAsync(object request);
    }
}

