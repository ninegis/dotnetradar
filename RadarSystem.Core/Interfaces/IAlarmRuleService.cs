namespace RadarSystem.Core.Interfaces
{
    public interface IAlarmRuleService
    {
        Task<List<object>> GetAlarmRulesAsync(string projectId);
        Task<string> AddAlarmRuleAsync(object request);
        Task UpdateAlarmRuleAsync(object request);
        Task RemoveAlarmRuleAsync(string id, string projectId);
    }
}

