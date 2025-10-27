using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 地理标记服务实现
    /// </summary>
    public class GeoService : IGeoService
    {
        private readonly IGeoMarkRepository _geoMarkRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<GeoService> _logger;

        public GeoService(
            IGeoMarkRepository geoMarkRepository,
            IProjectRepository projectRepository,
            ILogger<GeoService> logger)
        {
            _geoMarkRepository = geoMarkRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<GeoMark> CreateGeoMarkAsync(CreateGeoMarkRequest request)
        {
            try
            {
                // 验证请求
                if (!await ValidateGeoMarkAsync(request))
                {
                    throw new ArgumentException("地理标记验证失败");
                }

                // 检查项目是否存在
                var project = await _projectRepository.GetByProjectIdAsync(request.ProjectId);
                if (project == null)
                {
                    throw new ArgumentException($"项目不存在: {request.ProjectId}");
                }

                // 创建地理标记
                var geoMark = new GeoMark
                {
                    Id = $"GEOMARK_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                    ProjectId = request.ProjectId,
                    Name = request.Name,
                    Type = request.Type,
                    CoordinatesJson = request.CoordinatesJson,
                    Description = request.Description,
                    Color = request.Color ?? "#FF0000",
                    Icon = request.Icon,
                    CreateTime = DateTime.Now,
                    IsDeleted = false
                };

                var result = await _geoMarkRepository.CreateAsync(geoMark);
                _logger.LogInformation($"创建地理标记成功: {result.Name} (ID: {result.Id})");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建地理标记失败: {request.Name}");
                throw;
            }
        }

        public async Task<GeoMark> UpdateGeoMarkAsync(string id, UpdateGeoMarkRequest request)
        {
            try
            {
                var existing = await _geoMarkRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    throw new ArgumentException($"地理标记不存在: {id}");
                }

                // 更新字段
                if (!string.IsNullOrEmpty(request.Name))
                    existing.Name = request.Name;
                
                if (!string.IsNullOrEmpty(request.Type))
                    existing.Type = request.Type;
                
                if (request.CoordinatesJson != null)
                    existing.CoordinatesJson = request.CoordinatesJson;
                
                if (request.Description != null)
                    existing.Description = request.Description;
                
                if (request.Color != null)
                    existing.Color = request.Color;
                
                if (request.Icon != null)
                    existing.Icon = request.Icon;

                existing.UpdateTime = DateTime.Now;

                var result = await _geoMarkRepository.UpdateAsync(existing);
                _logger.LogInformation($"更新地理标记成功: {result.Name} (ID: {result.Id})");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新地理标记失败: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteGeoMarkAsync(string id, bool hardDelete = false)
        {
            try
            {
                var result = await _geoMarkRepository.DeleteAsync(id, hardDelete);
                
                if (result)
                {
                    _logger.LogInformation($"删除地理标记成功: ID={id}, HardDelete={hardDelete}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除地理标记失败: {id}");
                throw;
            }
        }

        public async Task<GeoMark?> GetGeoMarkAsync(string id)
        {
            return await _geoMarkRepository.GetByIdAsync(id);
        }

        public async Task<List<GeoMark>> GetProjectGeoMarksAsync(string projectId, bool includeDeleted = false)
        {
            return await _geoMarkRepository.GetByProjectIdAsync(projectId, includeDeleted);
        }

        public async Task<List<GeoMark>> SearchGeoMarksAsync(string projectId, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetProjectGeoMarksAsync(projectId);
            }
            
            return await _geoMarkRepository.SearchAsync(projectId, searchText);
        }

        public async Task<int> GetGeoMarkCountAsync(string projectId)
        {
            return await _geoMarkRepository.GetCountByProjectIdAsync(projectId);
        }

        public async Task<bool> ValidateGeoMarkAsync(CreateGeoMarkRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProjectId))
            {
                _logger.LogWarning("项目ID不能为空");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("地理标记名称不能为空");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                _logger.LogWarning("地理标记类型不能为空");
                return false;
            }

            var validTypes = new[] { "Point", "Line", "Polygon", "Circle", "Rectangle" };
            if (Array.IndexOf(validTypes, request.Type) == -1)
            {
                _logger.LogWarning($"无效的地理标记类型: {request.Type}");
                return false;
            }

            return await Task.FromResult(true);
        }
    }
}

