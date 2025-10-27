using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;
// using RadarSystem.Core.Models;

namespace RadarSystem.Data.Repositories
{
    /// <summary>
    /// 报警规则仓库实现 - 临时简化版本
    /// </summary>
    public class AlarmRuleRepository
    {
        private readonly RadarDbContext _context;
        private readonly ILogger<AlarmRuleRepository> _logger;

        public AlarmRuleRepository(RadarDbContext context, ILogger<AlarmRuleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> AddAsync(AlarmRuleEntity entity)
        {
            try
            {
                _context.AlarmRules.Add(entity);
                await _context.SaveChangesAsync();
                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加报警规则失败");
                throw;
            }
        }

        public async Task<AlarmRuleEntity?> GetByIdAsync(string id)
        {
            try
            {
                return await _context.AlarmRules
                    .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报警规则失败: {Id}", id);
                throw;
            }
        }

        public async Task<List<AlarmRuleEntity>> GetByProjectIdAsync(string projectId)
        {
            try
            {
                return await _context.AlarmRules
                    .Where(a => a.ProjectId == projectId && !a.IsDeleted)
                    .OrderByDescending(a => a.CreateTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取项目报警规则失败: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(AlarmRuleEntity entity)
        {
            try
            {
                _context.AlarmRules.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新报警规则失败");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var entity = await _context.AlarmRules.FindAsync(id);
                if (entity != null)
                {
                    entity.IsDeleted = true;
                    entity.UpdateTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除报警规则失败: {Id}", id);
                return false;
            }
        }
    }
}