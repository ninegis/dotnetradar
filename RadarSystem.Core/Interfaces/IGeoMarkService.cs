namespace RadarSystem.Core.Interfaces
{
    public interface IGeoMarkService
    {
        Task<string> AddGeoMarkAsync(object request);
        Task RemoveGeoMarkAsync(string id, string projectId);
        Task<List<object>> GetGeoMarksAsync(string projectId);
    }
}

