using RadarSystem.WebAPI.Models;

namespace RadarSystem.WebAPI.Services
{
    // ==================== Service接口定义 ====================
    
    public interface IAlarmRecordService
    {
        Task<int> GetAlarmRecordCountAsync(object request);
        Task<List<object>> GetAlarmRecordsAsync(object request);
    }

    public interface ILayerService
    {
        Task<string> AddLayerAsync(object request);
        Task DeleteLayerAsync(string oid);
        Task EnableLayerAsync(string oid, bool enable);
        Task ShowLayerAsync(string oid, bool show);
        Task<List<object>> GetLayersAsync(string orgid);
    }

    public interface IRadarImageService
    {
        Task<int> GetImageCountAsync(string projectId, string deviceId, string startDateTime, string endDateTime, string? type, string? status);
        Task<List<object>> GetImageListAsync(string projectId, string deviceId, string startDateTime, string endDateTime, string? type, string? status, int count);
        Task<object> GenerateImageAsync(GenerateImageRequest request);
        Task<byte[]> GetImageResourceAsync(string url, string filename);
    }

    public interface IDataManageService
    {
        Task RestoreDataAsync(object request);
        Task GenerateDataAsync(object request);
    }
    
    public interface IGeoMarkService
    {
        Task<object> AddGeoMarkAsync(object request);
        Task RemoveGeoMarkAsync(string id, string projectId);
    }
    
    public interface IAlarmContactService
    {
        Task<object> AddContactAsync(object request);
        Task<object> UpdateContactAsync(object request);
        Task<List<object>> GetContactsAsync(string projectId);
        Task RemoveContactAsync(string id, string projectId);
        Task<object> UpdateSmsConfigAsync(object request);
    }
    
    public interface IRadarParamsService
    {
        Task<object> UpdateRadarParamAsync(object request);
        Task<object> UpdateMimoLiteParamAsync(object request);
        Task<object> UpdateAlgorithmParamAsync(object request);
        Task<object> UpdateMimoLiteAlgorithmParamAsync(object request);
        Task<object> UpdateSpeedTargetAsync(object request);
        Task<object> UpdateColorBarAsync(object request);
        Task<object> UpdateHiddenAnalysisAsync(object request);
    }

    // ==================== Service实现 ====================
    
    public class SimpleAlarmRecordService : IAlarmRecordService
    {
        public Task<int> GetAlarmRecordCountAsync(object request) => Task.FromResult(0);
        public Task<List<object>> GetAlarmRecordsAsync(object request) => Task.FromResult(new List<object>());
    }

    public class SimpleLayerService : ILayerService
    {
        public Task<string> AddLayerAsync(object request) => Task.FromResult("SUCCESS");
        public Task DeleteLayerAsync(string oid) => Task.CompletedTask;
        public Task EnableLayerAsync(string oid, bool enable) => Task.CompletedTask;
        public Task ShowLayerAsync(string oid, bool show) => Task.CompletedTask;
        public Task<List<object>> GetLayersAsync(string orgid) => Task.FromResult(new List<object>());
    }

    public class SimpleRadarImageService : IRadarImageService
    {
        public Task<int> GetImageCountAsync(string projectId, string deviceId, string startDateTime, string endDateTime, string? type, string? status) => Task.FromResult(0);
        public Task<List<object>> GetImageListAsync(string projectId, string deviceId, string startDateTime, string endDateTime, string? type, string? status, int count) => Task.FromResult(new List<object>());
        public Task<object> GenerateImageAsync(GenerateImageRequest request) => Task.FromResult<object>(new { success = true });
        public Task<byte[]> GetImageResourceAsync(string url, string filename) => Task.FromResult(Array.Empty<byte>());
    }

    public class SimpleDataManageService : IDataManageService
    {
        public Task RestoreDataAsync(object request) => Task.CompletedTask;
        public Task GenerateDataAsync(object request) => Task.CompletedTask;
    }
    
    public class SimpleGeoMarkService : IGeoMarkService
    {
        public Task<object> AddGeoMarkAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task RemoveGeoMarkAsync(string id, string projectId) => Task.CompletedTask;
    }
    
    public class SimpleAlarmContactService : IAlarmContactService
    {
        public Task<object> AddContactAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateContactAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<List<object>> GetContactsAsync(string projectId) => Task.FromResult(new List<object>());
        public Task RemoveContactAsync(string id, string projectId) => Task.CompletedTask;
        public Task<object> UpdateSmsConfigAsync(object request) => Task.FromResult<object>(new { success = true });
    }
    
    public class SimpleRadarParamsService : IRadarParamsService
    {
        public Task<object> UpdateRadarParamAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateMimoLiteParamAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateAlgorithmParamAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateMimoLiteAlgorithmParamAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateSpeedTargetAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateColorBarAsync(object request) => Task.FromResult<object>(new { success = true });
        public Task<object> UpdateHiddenAnalysisAsync(object request) => Task.FromResult<object>(new { success = true });
    }
    
    // ==================== 扩展的Service接口（IProjectService和IDeviceService）====================
    
    public interface IProjectServiceExtended : RadarSystem.Core.Interfaces.IProjectService
    {
        Task<object> AddProjectAsync(AddProjectRequest request);
        Task RemoveProjectAsync(string projectId);
        Task<object> SetProjectViewAsync(object request);
        Task<object> UpdateImageAnalysisConfigAsync(object request);
    }
    
    public interface IDeviceServiceExtended : RadarSystem.Core.Interfaces.IDeviceService
    {
        Task<object> AddDeviceAsync(object request);
        Task RemoveDeviceAsync(string deviceId);
        Task<object> GetRadarLastHeartbeatAsync(string deviceId);
        Task<object> GetRadarOnlineStatusByTimeAsync(string deviceId, string datetime);
    }
    
}
