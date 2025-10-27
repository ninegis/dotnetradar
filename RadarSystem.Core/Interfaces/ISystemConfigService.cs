namespace RadarSystem.Core.Interfaces
{
    public interface ISystemConfigService
    {
        Task<object> GetDiskStorageAsync();
        Task UpdateDiskStorageAsync(object request);
        Task<object> GetDiskThresholdAsync();
    }
}

