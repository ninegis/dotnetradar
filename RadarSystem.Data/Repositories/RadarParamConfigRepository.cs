using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface IRadarParamConfigRepository
    {
        Task<RadarParamConfigEntity?> GetAsync(string projectId, string deviceId, string paramType);
        Task<string> AddOrUpdateAsync(RadarParamConfigEntity entity);
        Task<List<RadarParamConfigEntity>> GetByDeviceAsync(string projectId, string deviceId);
    }

    public class RadarParamConfigRepository : IRadarParamConfigRepository
    {
        private readonly RadarDbContext _context;

        public RadarParamConfigRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<RadarParamConfigEntity?> GetAsync(string projectId, string deviceId, string paramType)
        {
            return await _context.RadarParamConfigs
                .FirstOrDefaultAsync(x => x.ProjectId == projectId 
                    && x.DeviceId == deviceId 
                    && x.ParamType == paramType);
        }

        public async Task<string> AddOrUpdateAsync(RadarParamConfigEntity entity)
        {
            var existing = await GetAsync(entity.ProjectId, entity.DeviceId, entity.ParamType);
            
            if (existing != null)
            {
                existing.ParametersJson = entity.ParametersJson;
                existing.UpdateTime = DateTime.Now;
                
                _context.RadarParamConfigs.Update(existing);
                await _context.SaveChangesAsync();
                return existing.Id;
            }
            else
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.CreateTime = DateTime.Now;
                
                await _context.RadarParamConfigs.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity.Id;
            }
        }

        public async Task<List<RadarParamConfigEntity>> GetByDeviceAsync(string projectId, string deviceId)
        {
            return await _context.RadarParamConfigs
                .Where(x => x.ProjectId == projectId && x.DeviceId == deviceId)
                .ToListAsync();
        }
    }
}

