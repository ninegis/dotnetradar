using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 项目服务接口
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// 创建项目
        /// </summary>
        Task<(bool Success, string Message, int ProjectId)> CreateProjectAsync(CreateProjectRequest request);

        /// <summary>
        /// 更新项目
        /// </summary>
        Task<(bool Success, string Message)> UpdateProjectAsync(Project project);

        /// <summary>
        /// 删除项目
        /// </summary>
        Task<(bool Success, string Message)> DeleteProjectAsync(string projectId);

        /// <summary>
        /// 获取项目详情
        /// </summary>
        Task<Project?> GetProjectAsync(string projectId);

        /// <summary>
        /// 查询项目列表
        /// </summary>
        Task<(List<Project> Projects, int TotalCount)> QueryProjectsAsync(ProjectQueryRequest request);

        /// <summary>
        /// 获取所有活动项目
        /// </summary>
        Task<List<Project>> GetActiveProjectsAsync();

        /// <summary>
        /// 验证项目ID是否可用
        /// </summary>
        Task<bool> IsProjectIdAvailableAsync(string projectId);

        // 新增接口，对应前端API调用
        Task<string> AddProjectAsync(object request);
        Task RemoveProjectAsync(string projectId);
        Task SetProjectViewAsync(object request);
        Task UpdateImageAnalysisConfigAsync(object request);
        Task UpdateProjectInfoAsync(object request);
    }
}

