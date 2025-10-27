namespace RadarSystem.Core.Interfaces
{
    public interface IAlarmContactService
    {
        Task<List<object>> GetContactsAsync(string projectId);
        Task<string> AddContactAsync(object request);
        Task UpdateContactAsync(object request);
        Task RemoveContactAsync(string id, string projectId);
        Task UpdateSmsConfigAsync(object request);
    }
}

