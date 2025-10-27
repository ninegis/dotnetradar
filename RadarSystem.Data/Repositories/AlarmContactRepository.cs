using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface IAlarmContactRepository
    {
        Task<List<AlarmContactEntity>> GetByProjectIdAsync(string projectId);
        Task<AlarmContactEntity?> GetByIdAsync(string id);
        Task<string> AddAsync(AlarmContactEntity entity);
        Task UpdateAsync(AlarmContactEntity entity);
        Task DeleteAsync(string id);
        Task<List<AlarmContactEntity>> GetByAlarmLevelAsync(string projectId, int alarmLevel);
    }

    public class AlarmContactRepository : IAlarmContactRepository
    {
        private readonly RadarDbContext _context;

        public AlarmContactRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlarmContactEntity>> GetByProjectIdAsync(string projectId)
        {
            return await _context.AlarmContacts
                .Where(x => x.ProjectId == projectId && !x.IsDeleted)
                .OrderBy(x => x.CreateTime)
                .ToListAsync();
        }

        public async Task<AlarmContactEntity?> GetByIdAsync(string id)
        {
            return await _context.AlarmContacts
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<string> AddAsync(AlarmContactEntity entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.CreateTime = DateTime.Now;
            entity.IsDeleted = false;
            
            await _context.AlarmContacts.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            return entity.Id;
        }

        public async Task UpdateAsync(AlarmContactEntity entity)
        {
            entity.UpdateTime = DateTime.Now;
            _context.AlarmContacts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdateTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<AlarmContactEntity>> GetByAlarmLevelAsync(string projectId, int alarmLevel)
        {
            return await _context.AlarmContacts
                .Where(x => x.ProjectId == projectId 
                    && x.AlarmLevel == alarmLevel 
                    && x.Enable 
                    && !x.IsDeleted)
                .ToListAsync();
        }
    }
}

