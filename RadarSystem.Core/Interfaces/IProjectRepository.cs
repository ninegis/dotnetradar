using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 项目数据仓储接口
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// 创建项目
        /// </summary>
        Task<int> CreateAsync(Project project);

        /// <summary>
        /// 更新项目
        /// </summary>
        Task<bool> UpdateAsync(Project project);

        /// <summary>
        /// 删除项目（软删除）
        /// </summary>
        Task<bool> DeleteAsync(string projectId);

        /// <summary>
        /// 根据ID获取项目
        /// </summary>
        Task<Project?> GetByIdAsync(int id);

        /// <summary>
        /// 根据项目ID获取项目
        /// </summary>
        Task<Project?> GetByProjectIdAsync(string projectId);

        /// <summary>
        /// 查询项目列表
        /// </summary>
        Task<List<Project>> QueryAsync(ProjectQueryRequest request);

        /// <summary>
        /// 获取项目总数
        /// </summary>
        Task<int> CountAsync(ProjectQueryRequest request);

        /// <summary>
        /// 获取所有活动项目
        /// </summary>
        Task<List<Project>> GetActiveProjectsAsync();

        /// <summary>
        /// 检查项目ID是否存在
        /// </summary>
        Task<bool> ExistsAsync(string projectId);
    }
}

