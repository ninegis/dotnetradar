using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Repositories
{
    /// <summary>
    /// 雷达数据仓库实现
    /// </summary>
    public class RadarDataRepository : IRadarDataRepository
    {
        private readonly RadarDbContext _context;
        private readonly ILogger<RadarDataRepository> _logger;

        public RadarDataRepository(RadarDbContext context, ILogger<RadarDataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddRadarDataAsync(RadarDataEntity radarData)
        {
            try
            {
                radarData.CreateTime = DateTime.Now;
                radarData.UpdateTime = DateTime.Now;
                
                _context.RadarData.Add(radarData);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加雷达数据时发生错误");
                return false;
            }
        }

        public async Task<RadarDataEntity?> GetRadarDataByIdAsync(int id)
        {
            try
            {
                return await _context.RadarData
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据ID获取雷达数据时发生错误");
                return null;
            }
        }

        public async Task<List<RadarDataEntity>> GetRadarDataByDeviceAndTimeAsync(string deviceId, DateTime startTime, DateTime endTime)
        {
            try
            {
                return await _context.RadarData
                    .Where(x => x.DeviceId == deviceId 
                               && x.Timestamp >= startTime 
                               && x.Timestamp <= endTime 
                               && !x.IsDeleted)
                    .OrderByDescending(x => x.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据设备ID和时间范围获取雷达数据时发生错误");
                return new List<RadarDataEntity>();
            }
        }

        public async Task<List<RadarDataEntity>> GetRadarDataByProjectAsync(string projectId, int pageIndex, int pageSize)
        {
            try
            {
                return await _context.RadarData
                    .Where(x => x.ProjectId == projectId && !x.IsDeleted)
                    .OrderByDescending(x => x.Timestamp)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据项目ID获取雷达数据时发生错误");
                return new List<RadarDataEntity>();
            }
        }

        public async Task<bool> UpdateRadarDataAsync(RadarDataEntity radarData)
        {
            try
            {
                radarData.UpdateTime = DateTime.Now;
                _context.RadarData.Update(radarData);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新雷达数据时发生错误");
                return false;
            }
        }

        public async Task<bool> DeleteRadarDataAsync(int id)
        {
            try
            {
                var radarData = await _context.RadarData.FindAsync(id);
                if (radarData != null)
                {
                    radarData.IsDeleted = true;
                    radarData.UpdateTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除雷达数据时发生错误");
                return false;
            }
        }

        public async Task<int> GetRadarDataCountAsync(string? projectId = null, string? deviceId = null)
        {
            try
            {
                var query = _context.RadarData.Where(x => !x.IsDeleted);
                
                if (!string.IsNullOrEmpty(projectId))
                {
                    query = query.Where(x => x.ProjectId == projectId);
                }
                
                if (!string.IsNullOrEmpty(deviceId))
                {
                    query = query.Where(x => x.DeviceId == deviceId);
                }
                
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取雷达数据数量时发生错误");
                return 0;
            }
        }

        public async Task<RadarDataEntity?> GetRadarDataByFileNameAsync(string fileName)
        {
            try
            {
                return await _context.RadarData
                    .Where(x => x.FileName == fileName && !x.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据文件名获取雷达数据时发生错误");
                return null;
            }
        }
    }
}
