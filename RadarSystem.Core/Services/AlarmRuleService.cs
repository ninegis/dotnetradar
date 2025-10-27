using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Data.Models;
using RadarSystem.Data.Repositories;
using System.Text.Json;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 报警规则服务 - 简化版本
    /// </summary>
    public class AlarmRuleService : IAlarmRuleService
    {
        private readonly AlarmRuleRepository _alarmRuleRepository;
        private readonly ILogger<AlarmRuleService> _logger;

        public AlarmRuleService(
            AlarmRuleRepository alarmRuleRepository,
            ILogger<AlarmRuleService> logger)
        {
            _alarmRuleRepository = alarmRuleRepository;
            _logger = logger;
        }

        public async Task<List<object>> GetAlarmRulesAsync(string projectId)
        {
            try
            {
                _logger.LogInformation("获取告警规则列表: {ProjectId}", projectId);
                
                var entities = await _alarmRuleRepository.GetByProjectIdAsync(projectId);
                
                var result = entities.Select(e => new
                {
                    id = e.Id,
                    projectId = e.ProjectId,
                    ruleName = e.RuleName,
                    alarmContent = e.AlarmContent,
                    enable = e.Enable,
                    devices = e.Devices,
                    geoMarkArray = e.GeoMarkArray,
                    dataSource = e.DataSource,
                    targetFlag = e.TargetFlag,
                    createTime = e.CreateTime
                } as object).ToList();

                _logger.LogInformation("获取告警规则列表成功: {Count}条", result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取告警规则列表失败: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<string> AddAlarmRuleAsync(object request)
        {
            try
            {
                _logger.LogInformation("添加告警规则");
                
                var json = JsonSerializer.Serialize(request);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                if (data == null)
                    throw new ArgumentException("请求参数无效");

                var projectId = data.GetValueOrDefault("ProjectId")?.ToString() ?? string.Empty;
                var ruleName = data.GetValueOrDefault("RuleName")?.ToString() ?? $"规则_{DateTime.Now:yyyyMMddHHmmss}";

                if (string.IsNullOrEmpty(projectId))
                    throw new ArgumentException("项目ID不能为空");

                var entity = new AlarmRuleEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    ProjectId = projectId,
                    RuleName = ruleName,
                    AlarmContent = data.GetValueOrDefault("AlarmContent")?.ToString(),
                    Enable = data.GetValueOrDefault("Enable")?.ToString() != "false",
                    Devices = data.GetValueOrDefault("Devices")?.ToString(),
                    GeoMarkArray = data.GetValueOrDefault("GeoMarkArray")?.ToString(),
                    DataSource = data.GetValueOrDefault("DataSource")?.ToString() ?? "10",
                    TargetFlag = data.GetValueOrDefault("TargetFlag")?.ToString() == "true",
                    CreateTime = DateTime.Now
                };

                var id = await _alarmRuleRepository.AddAsync(entity);
                
                _logger.LogInformation("告警规则添加成功: {Id}, 名称: {Name}", id, ruleName);
                
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加告警规则失败");
                throw;
            }
        }

        public async Task UpdateAlarmRuleAsync(object request)
        {
            try
            {
                _logger.LogInformation("更新告警规则");
                
                var json = JsonSerializer.Serialize(request);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                if (data == null || !data.ContainsKey("Id"))
                    throw new ArgumentException("请求参数无效");

                var id = data["Id"]?.ToString() ?? string.Empty;
                var existing = await _alarmRuleRepository.GetByIdAsync(id);
                
                if (existing == null)
                    throw new ArgumentException($"告警规则不存在: {id}");

                // 更新字段
                if (data.ContainsKey("RuleName"))
                    existing.RuleName = data["RuleName"]?.ToString() ?? existing.RuleName;
                if (data.ContainsKey("AlarmContent"))
                    existing.AlarmContent = data["AlarmContent"]?.ToString();
                if (data.ContainsKey("Enable"))
                    existing.Enable = data["Enable"]?.ToString() != "false";
                if (data.ContainsKey("Devices"))
                    existing.Devices = data["Devices"]?.ToString();
                if (data.ContainsKey("GeoMarkArray"))
                    existing.GeoMarkArray = data["GeoMarkArray"]?.ToString();

                existing.UpdateTime = DateTime.Now;

                await _alarmRuleRepository.UpdateAsync(existing);
                
                _logger.LogInformation("告警规则更新成功: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新告警规则失败");
                throw;
            }
        }

        public async Task RemoveAlarmRuleAsync(string id, string projectId)
        {
            try
            {
                _logger.LogInformation("删除告警规则: {Id}, 项目: {ProjectId}", id, projectId);
                
                var existing = await _alarmRuleRepository.GetByIdAsync(id);
                if (existing == null)
                    throw new ArgumentException($"告警规则不存在: {id}");

                if (existing.ProjectId != projectId)
                    throw new ArgumentException("项目ID不匹配");

                await _alarmRuleRepository.DeleteAsync(id);
                
                _logger.LogInformation("告警规则删除成功: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除告警规则失败: {Id}", id);
                throw;
            }
        }
    }
}