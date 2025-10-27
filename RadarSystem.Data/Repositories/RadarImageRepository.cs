using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface IRadarImageRepository
    {
        Task<int> GetCountAsync(string projectId, string deviceId, DateTime startTime, DateTime endTime, string? type, string? status);
        Task<List<RadarImageEntity>> GetListAsync(string projectId, string deviceId, DateTime startTime, DateTime endTime, string? type, string? status, int count);
        Task<RadarImageEntity?> GetByIdAsync(string id);
        Task<string> AddAsync(RadarImageEntity entity);
        Task UpdateAsync(RadarImageEntity entity);
        Task DeleteAsync(string id);
    }

    public class RadarImageRepository : IRadarImageRepository
    {
        private readonly RadarDbContext _context;

        public RadarImageRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetCountAsync(string projectId, string deviceId, DateTime startTime, DateTime endTime, string? type, string? status)
        {
            var query = _context.RadarImages
                .Where(x => x.ProjectId == projectId 
                    && x.DeviceId == deviceId
                    && x.CaptureTime >= startTime
                    && x.CaptureTime <= endTime
                    && !x.IsDeleted);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(x => x.ImageType == type);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => x.Status == status);

            return await query.CountAsync();
        }

        public async Task<List<RadarImageEntity>> GetListAsync(string projectId, string deviceId, DateTime startTime, DateTime endTime, string? type, string? status, int count)
        {
            var query = _context.RadarImages
                .Where(x => x.ProjectId == projectId 
                    && x.DeviceId == deviceId
                    && x.CaptureTime >= startTime
                    && x.CaptureTime <= endTime
                    && !x.IsDeleted);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(x => x.ImageType == type);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => x.Status == status);

            return await query
                .OrderByDescending(x => x.CaptureTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<RadarImageEntity?> GetByIdAsync(string id)
        {
            return await _context.RadarImages
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<string> AddAsync(RadarImageEntity entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.CreateTime = DateTime.Now;
            entity.IsDeleted = false;
            
            await _context.RadarImages.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            return entity.Id;
        }

        public async Task UpdateAsync(RadarImageEntity entity)
        {
            entity.UpdateTime = DateTime.Now;
            _context.RadarImages.Update(entity);
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
    }
}

