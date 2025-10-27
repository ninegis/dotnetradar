using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 项目服务实现
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            IProjectRepository projectRepository,
            IDeviceRepository deviceRepository,
            ILogger<ProjectService> logger)
        {
            _projectRepository = projectRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int ProjectId)> CreateProjectAsync(CreateProjectRequest request)
        {
            try
            {
                // 验证项目ID是否已存在
                if (await _projectRepository.ExistsAsync(request.ProjectId))
                {
                    return (false, $"项目ID '{request.ProjectId}' 已存在", 0);
                }

                // 验证必填字段
                if (string.IsNullOrWhiteSpace(request.ProjectId))
                {
                    return (false, "项目ID不能为空", 0);
                }

                if (string.IsNullOrWhiteSpace(request.ProjectName))
                {
                    return (false, "项目名称不能为空", 0);
                }

                // 创建存储目录
                if (!string.IsNullOrWhiteSpace(request.StoragePath))
                {
                    try
                    {
                        if (!Directory.Exists(request.StoragePath))
                        {
                            Directory.CreateDirectory(request.StoragePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "创建存储目录失败: {StoragePath}", request.StoragePath);
                    }
                }

                var project = new Project
                {
                    ProjectId = request.ProjectId,
                    ProjectName = request.ProjectName,
                    Description = request.Description,
                    Location = request.Location,
                    Status = "Active",
                    CreatedBy = request.CreatedBy,
                    StoragePath = request.StoragePath,
                    ContactPerson = request.ContactPerson,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail,
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,
                    Elevation = request.Elevation,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                var projectId = await _projectRepository.CreateAsync(project);

                _logger.LogInformation("项目创建成功: {ProjectId} - {ProjectName}", request.ProjectId, request.ProjectName);
                return (true, "项目创建成功", projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建项目失败: {ProjectId}", request.ProjectId);
                return (false, $"创建项目失败: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)> UpdateProjectAsync(Project project)
        {
            try
            {
                // 验证项目是否存在
                var existingProject = await _projectRepository.GetByProjectIdAsync(project.ProjectId);
                if (existingProject == null)
                {
                    return (false, $"项目 '{project.ProjectId}' 不存在");
                }

                // 验证必填字段
                if (string.IsNullOrWhiteSpace(project.ProjectName))
                {
                    return (false, "项目名称不能为空");
                }

                var success = await _projectRepository.UpdateAsync(project);

                if (success)
                {
                    _logger.LogInformation("项目更新成功: {ProjectId}", project.ProjectId);
                    return (true, "项目更新成功");
                }
                else
                {
                    return (false, "项目更新失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新项目失败: {ProjectId}", project.ProjectId);
                return (false, $"更新项目失败: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteProjectAsync(string projectId)
        {
            try
            {
                // 检查项目下是否有设备
                var devices = await _deviceRepository.GetByProjectIdAsync(projectId);
                if (devices.Count > 0)
                {
                    return (false, $"项目下还有 {devices.Count} 个设备，请先删除设备");
                }

                var success = await _projectRepository.DeleteAsync(projectId);

                if (success)
                {
                    _logger.LogInformation("项目删除成功: {ProjectId}", projectId);
                    return (true, "项目删除成功");
                }
                else
                {
                    return (false, "项目不存在或已被删除");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除项目失败: {ProjectId}", projectId);
                return (false, $"删除项目失败: {ex.Message}");
            }
        }

        public async Task<Project?> GetProjectAsync(string projectId)
        {
            try
            {
                var project = await _projectRepository.GetByProjectIdAsync(projectId);
                
                if (project != null)
                {
                    // 加载项目下的设备
                    project.Devices = await _deviceRepository.GetByProjectIdAsync(projectId);
                }

                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取项目失败: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<(List<Project> Projects, int TotalCount)> QueryProjectsAsync(ProjectQueryRequest request)
        {
            try
            {
                var projects = await _projectRepository.QueryAsync(request);
                var totalCount = await _projectRepository.CountAsync(request);

                // 为每个项目加载设备数量
                foreach (var project in projects)
                {
                    var devices = await _deviceRepository.GetByProjectIdAsync(project.ProjectId);
                    project.Devices = devices;
                }

                return (projects, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询项目列表失败");
                throw;
            }
        }

        public async Task<List<Project>> GetActiveProjectsAsync()
        {
            try
            {
                return await _projectRepository.GetActiveProjectsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活动项目列表失败");
                throw;
            }
        }

        public async Task<bool> IsProjectIdAvailableAsync(string projectId)
        {
            try
            {
                return !await _projectRepository.ExistsAsync(projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证项目ID失败: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<string> AddProjectAsync(object request)
        {
            _logger.LogInformation("添加项目 (新接口)");
            // TODO: 实现添加项目逻辑，对应前端API调用
            await Task.CompletedTask;
            return Guid.NewGuid().ToString();
        }

        public async Task RemoveProjectAsync(string projectId)
        {
            _logger.LogInformation("删除项目: {ProjectId} (新接口)", projectId);
            // TODO: 实现删除项目逻辑，对应前端API调用
            await _projectRepository.DeleteAsync(projectId);
        }

        public async Task SetProjectViewAsync(object request)
        {
            _logger.LogInformation("设置项目视角");
            // TODO: 实现设置项目视角逻辑
            await Task.CompletedTask;
        }

        public async Task UpdateImageAnalysisConfigAsync(object request)
        {
            _logger.LogInformation("更新图像分析配置");
            // TODO: 实现更新图像分析配置逻辑
            await Task.CompletedTask;
        }

        public async Task UpdateProjectInfoAsync(object request)
        {
            _logger.LogInformation("更新项目信息 (新接口)");
            // TODO: 实现更新项目信息逻辑
            await Task.CompletedTask;
        }
    }
}

