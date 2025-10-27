using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    public interface ISmsConfigRepository
    {
        Task<SmsConfigEntity?> GetByProjectIdAsync(string projectId);
        Task<string> AddOrUpdateAsync(SmsConfigEntity entity);
    }

    public class SmsConfigRepository : ISmsConfigRepository
    {
        private readonly RadarDbContext _context;

        public SmsConfigRepository(RadarDbContext context)
        {
            _context = context;
        }

        public async Task<SmsConfigEntity?> GetByProjectIdAsync(string projectId)
        {
            return await _context.SmsConfigs
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);
        }

        public async Task<string> AddOrUpdateAsync(SmsConfigEntity entity)
        {
            var existing = await GetByProjectIdAsync(entity.ProjectId);
            
            if (existing != null)
            {
                existing.Enable = entity.Enable;
                existing.NotifyChannel = entity.NotifyChannel;
                existing.AccessKeyId = entity.AccessKeyId;
                existing.AccessKeySecret = entity.AccessKeySecret;
                existing.SignName = entity.SignName;
                existing.TemplateCode = entity.TemplateCode;
                existing.Provider = entity.Provider;
                existing.ApiKey = entity.ApiKey;
                existing.ApiSecret = entity.ApiSecret;
                existing.TemplateContent = entity.TemplateContent;
                existing.UpdateTime = DateTime.Now;
                
                _context.SmsConfigs.Update(existing);
                await _context.SaveChangesAsync();
                return existing.Id;
            }
            else
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.CreateTime = DateTime.Now;
                
                await _context.SmsConfigs.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity.Id;
            }
        }
    }
}

