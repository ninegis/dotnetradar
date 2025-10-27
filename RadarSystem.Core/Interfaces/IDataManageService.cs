namespace RadarSystem.Core.Interfaces
{
    public interface IDataManageService
    {
        Task RestoreDataAsync(object request);
        Task GenerateDataAsync(object request);
    }
}

