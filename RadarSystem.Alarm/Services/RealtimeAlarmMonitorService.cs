using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;
using CoreAlarmLevel = RadarSystem.Core.Models.AlarmLevel;

namespace RadarSystem.Alarm.Services
{
    /// <summary>
    /// 实时报警监控服务
    /// </summary>
    public class RealtimeAlarmMonitorService : BackgroundService
    {
        private readonly IAlarmService _alarmService;
        private readonly ILogger<RealtimeAlarmMonitorService> _logger;
        private readonly Dictionary<string, AlarmState> _alarmStates = new();
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10); // 每10秒检查一次
        
        public RealtimeAlarmMonitorService(
            IAlarmService alarmService,
            ILogger<RealtimeAlarmMonitorService> logger)
        {
            _alarmService = alarmService ?? throw new ArgumentNullException(nameof(alarmService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("实时报警监控服务已启动");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorAlarmsAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // 正常停止
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "报警监控过程中发生错误");
                }
            }
            
            _logger.LogInformation("实时报警监控服务已停止");
        }
        
        private async Task MonitorAlarmsAsync()
        {
            // 检查所有待监控的点
            var monitoringPoints = await GetMonitoringPointsAsync();
            
            foreach (var point in monitoringPoints)
            {
                await CheckPointAsync(point);
            }
            
            // 检查报警恢复
            await CheckAlarmRecoveryAsync();
        }
        
        private async Task<List<MonitoringPoint>> GetMonitoringPointsAsync()
        {
            // TODO: 从配置或数据库获取监控点
            // 这里返回示例数据
            return await Task.FromResult(new List<MonitoringPoint>());
        }
        
        private async Task CheckPointAsync(MonitoringPoint point)
        {
            try
            {
                // 获取最新数据
                var latestValue = await GetLatestValueAsync(point);
                
                if (latestValue == null)
                {
                    return;
                }
                
                // 检查是否超过阈值
                var threshold = point.Threshold;
                bool isAlarming = Math.Abs(latestValue.Value) >= threshold;
                
                // 获取或创建报警状态
                if (!_alarmStates.TryGetValue(point.Id, out var state))
                {
                    state = new AlarmState { PointId = point.Id };
                    _alarmStates[point.Id] = state;
                }
                
                if (isAlarming && !state.IsActive)
                {
                    // 新报警触发
                    await TriggerAlarmAsync(point, latestValue.Value, threshold);
                    state.IsActive = true;
                    state.TriggerTime = DateTime.Now;
                    state.LastAlarmValue = latestValue.Value;
                }
                else if (isAlarming && state.IsActive)
                {
                    // 持续报警，更新状态
                    state.LastAlarmValue = latestValue.Value;
                    state.AlarmCount++;
                    
                    // 如果报警等级升级，发送通知
                    var newLevel = DetermineAlarmLevel(latestValue.Value, threshold);
                    if (newLevel > state.Level)
                    {
                        await EscalateAlarmAsync(point, latestValue.Value, newLevel);
                        state.Level = newLevel;
                    }
                }
                else if (!isAlarming && state.IsActive)
                {
                    // 报警恢复
                    await RecoverAlarmAsync(point, state);
                    state.IsActive = false;
                    state.RecoveryTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查监控点失败: {PointId}", point.Id);
            }
        }
        
        private async Task<MonitoringValue?> GetLatestValueAsync(MonitoringPoint point)
        {
            // TODO: 实现获取最新值的逻辑
            return await Task.FromResult<MonitoringValue?>(null);
        }
        
        private async Task TriggerAlarmAsync(MonitoringPoint point, double value, double threshold)
        {
            var level = DetermineAlarmLevel(value, threshold);
            
            var alarm = new AlarmRecord
            {
                HandleId = Guid.NewGuid().ToString(),
                RuleId = point.Id,
                ProjectId = "PROJECT001", // TODO: 从配置获取
                AlarmLevel = level,
                AlarmContent = $"{point.Name}超过阈值: 当前值={value:F2}, 阈值={threshold:F2}",
                Timestamp = DateTime.Now,
                AlarmStatus = true,
                HandleStatus = "未处理",
                ScanStatus = "未扫描"
            };
            
            await _alarmService.CreateAlarmRecordAsync(alarm);
            
            _logger.LogWarning("报警触发: {PointName}, 值={Value}, 阈值={Threshold}, 等级={Level}",
                point.Name, value, threshold, level);
        }
        
        private async Task EscalateAlarmAsync(MonitoringPoint point, double value, CoreAlarmLevel newLevel)
        {
            _logger.LogWarning("报警等级升级: {PointName}, 新等级={Level}, 值={Value}",
                point.Name, newLevel, value);
            
            // 可以在这里发送更高级别的通知
            await Task.CompletedTask;
        }
        
        private async Task RecoverAlarmAsync(MonitoringPoint point, AlarmState state)
        {
            var duration = DateTime.Now - state.TriggerTime;
            
            _logger.LogInformation("报警恢复: {PointName}, 持续时间={Duration}, 报警次数={Count}",
                point.Name, duration, state.AlarmCount);
            
            // TODO: 更新报警记录状态为已恢复
            await Task.CompletedTask;
        }
        
        private async Task CheckAlarmRecoveryAsync()
        {
            // 检查长时间未恢复的报警
            var now = DateTime.Now;
            var staleAlarms = _alarmStates.Values
                .Where(s => s.IsActive && (now - s.TriggerTime) > TimeSpan.FromHours(24))
                .ToList();
            
            foreach (var alarm in staleAlarms)
            {
                _logger.LogWarning("长时间未恢复的报警: PointId={PointId}, 持续时间={Duration}",
                    alarm.PointId, now - alarm.TriggerTime);
            }
            
            await Task.CompletedTask;
        }
        
        private CoreAlarmLevel DetermineAlarmLevel(double value, double threshold)
        {
            double ratio = Math.Abs(value) / threshold;
            
            if (ratio >= 2.0)
            {
                return CoreAlarmLevel.Red; // Critical
            }
            else if (ratio >= 1.5)
            {
                return CoreAlarmLevel.Orange; // High
            }
            else if (ratio >= 1.0)
            {
                return CoreAlarmLevel.Yellow; // Medium
            }
            else
            {
                return CoreAlarmLevel.Blue; // Low
            }
        }
    }
    
    #region 数据模型
    
    public class MonitoringPoint
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Threshold { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
    
    public class MonitoringValue
    {
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class AlarmState
    {
        public string PointId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime TriggerTime { get; set; }
        public DateTime RecoveryTime { get; set; }
        public double LastAlarmValue { get; set; }
        public CoreAlarmLevel Level { get; set; } = CoreAlarmLevel.Blue;
        public int AlarmCount { get; set; }
    }
    
    // AlarmLevel 枚举已在 RadarSystem.Core.Models.AlarmModels 中定义
    // 使用 Blue(1), Yellow(2), Orange(3), Red(4) 映射到 Low, Medium, High, Critical
    
    #endregion
}

