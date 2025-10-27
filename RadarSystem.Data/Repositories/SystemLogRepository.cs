using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface ISystemLogRepository
    {
        Task<int> AddAsync(SystemLogEntity entity);
        Task<List<SystemLogEntity>> GetListAsync(string? projectCode, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize);
        Task<int> GetCountAsync(string? projectCode, DateTime? startTime, DateTime? endTime);
    }

    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly RadarDbContext _context;

        public SystemLogRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(SystemLogEntity entity)
        {
            entity.CreateTime = DateTime.Now;
            
            await _context.SystemLogs.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            return entity.Id;
        }

        public async Task<List<SystemLogEntity>> GetListAsync(string? projectCode, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrEmpty(projectCode))
                query = query.Where(x => x.ProjectCode == projectCode);

            if (startTime.HasValue)
                query = query.Where(x => x.CreateTime >= startTime.Value);

            if (endTime.HasValue)
                query = query.Where(x => x.CreateTime <= endTime.Value);

            return await query
                .OrderByDescending(x => x.CreateTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? projectCode, DateTime? startTime, DateTime? endTime)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrEmpty(projectCode))
                query = query.Where(x => x.ProjectCode == projectCode);

            if (startTime.HasValue)
                query = query.Where(x => x.CreateTime >= startTime.Value);

            if (endTime.HasValue)
                query = query.Where(x => x.CreateTime <= endTime.Value);

            return await query.CountAsync();
        }
    }
}

