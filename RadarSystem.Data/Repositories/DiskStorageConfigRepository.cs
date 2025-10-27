using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface IDiskStorageConfigRepository
    {
        Task<DiskStorageConfigEntity?> GetCurrentAsync();
        Task UpdateAsync(DiskStorageConfigEntity entity);
    }

    public class DiskStorageConfigRepository : IDiskStorageConfigRepository
    {
        private readonly RadarDbContext _context;

        public DiskStorageConfigRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<DiskStorageConfigEntity?> GetCurrentAsync()
        {
            return await _context.DiskStorageConfigs
                .OrderByDescending(x => x.UpdateTime)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(DiskStorageConfigEntity entity)
        {
            var existing = await GetCurrentAsync();
            
            if (existing != null)
            {
                existing.DiscSpacePercentage = entity.DiscSpacePercentage;
                existing.DeleteFile = entity.DeleteFile;
                existing.TotalSpace = entity.TotalSpace;
                existing.UsedSpace = entity.UsedSpace;
                existing.AvailableSpace = entity.AvailableSpace;
                existing.WarningThreshold = entity.WarningThreshold;
                existing.ErrorThreshold = entity.ErrorThreshold;
                existing.UpdateTime = DateTime.Now;
                
                _context.DiskStorageConfigs.Update(existing);
            }
            else
            {
                entity.UpdateTime = DateTime.Now;
                await _context.DiskStorageConfigs.AddAsync(entity);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}

