namespace RadarSystem.Core.Interfaces
{
    public interface IAlarmRecordService
    {
        Task<int> GetAlarmRecordCountAsync(object request);
        Task<List<object>> GetAlarmRecordsAsync(object request);
        Task AddAlarmMessageAsync(object request);
    }
}

