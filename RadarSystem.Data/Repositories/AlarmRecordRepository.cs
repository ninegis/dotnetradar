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
    /// 报警记录仓库实现 - 临时简化版本
    /// </summary>
    public class AlarmRecordRepository
    {
        private readonly RadarDbContext _context;
        private readonly ILogger<AlarmRecordRepository> _logger;

        public AlarmRecordRepository(RadarDbContext context, ILogger<AlarmRecordRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 临时简化实现，避免编译错误
        public async Task<string> AddAsync(AlarmRecordEntity entity)
        {
            try
            {
                _context.AlarmRecords.Add(entity);
                await _context.SaveChangesAsync();
                return entity.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加报警记录失败");
                throw;
            }
        }

        public async Task<AlarmRecordEntity?> GetByIdAsync(string id)
        {
            try
            {
                if (int.TryParse(id, out int idInt))
                {
                    return await _context.AlarmRecords
                        .FirstOrDefaultAsync(a => a.Id == idInt);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报警记录失败: {Id}", id);
                throw;
            }
        }

        public async Task<List<AlarmRecordEntity>> GetByProjectIdAsync(string projectId)
        {
            try
            {
                return await _context.AlarmRecords
                    .Where(a => a.ProjectId == projectId)
                    .OrderByDescending(a => a.CreateTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取项目报警记录失败: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(AlarmRecordEntity entity)
        {
            try
            {
                _context.AlarmRecords.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新报警记录失败");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var entity = await _context.AlarmRecords.FindAsync(id);
                if (entity != null)
                {
                    _context.AlarmRecords.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除报警记录失败: {Id}", id);
                return false;
            }
        }
    }
}