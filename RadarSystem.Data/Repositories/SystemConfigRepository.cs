using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface ISystemConfigRepository
    {
        Task<SystemConfigEntity?> GetByKeyAsync(string key);
        Task<List<SystemConfigEntity>> GetByCategoryAsync(string category);
        Task<string> AddOrUpdateAsync(SystemConfigEntity entity);
        Task DeleteAsync(string key);
    }

    public class SystemConfigRepository : ISystemConfigRepository
    {
        private readonly RadarDbContext _context;

        public SystemConfigRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<SystemConfigEntity?> GetByKeyAsync(string key)
        {
            return await _context.SystemConfigs
                .FirstOrDefaultAsync(x => x.ConfigKey == key);
        }

        public async Task<List<SystemConfigEntity>> GetByCategoryAsync(string category)
        {
            return await _context.SystemConfigs
                .Where(x => x.Category == category)
                .ToListAsync();
        }

        public async Task<string> AddOrUpdateAsync(SystemConfigEntity entity)
        {
            var existing = await GetByKeyAsync(entity.ConfigKey);
            
            if (existing != null)
            {
                existing.ConfigValue = entity.ConfigValue;
                existing.Description = entity.Description;
                existing.UpdateTime = DateTime.Now;
                
                _context.SystemConfigs.Update(existing);
                await _context.SaveChangesAsync();
                return existing.Id;
            }
            else
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.CreateTime = DateTime.Now;
                
                await _context.SystemConfigs.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity.Id;
            }
        }

        public async Task DeleteAsync(string key)
        {
            var entity = await GetByKeyAsync(key);
            if (entity != null)
            {
                _context.SystemConfigs.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}

