namespace RadarSystem.Core.Interfaces
{
    public interface ILayerService
    {
        Task<string> AddLayerAsync(object request);
        Task DeleteLayerAsync(string oid);
        Task EnableLayerAsync(string oid, bool enable);
        Task ShowLayerAsync(string oid, bool show);
        Task<List<object>> GetLayersAsync(string orgid);
    }
}

