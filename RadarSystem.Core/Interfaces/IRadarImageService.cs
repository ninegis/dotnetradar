namespace RadarSystem.Core.Interfaces
{
    public interface IRadarImageService
    {
        Task<int> GetImageCountAsync(string projectId, string deviceId, string startDateTime, string endDateTime, string? type, string? status);
        Task<List<object>> GetImageListAsync(string projectId, string deviceId, string startDateTime, string endDateTime, string? type, string? status, int count);
        Task<byte[]> GetImageResourceAsync(string url, string filename);
        Task<string> GenerateImageAsync(object request);
    }
}

