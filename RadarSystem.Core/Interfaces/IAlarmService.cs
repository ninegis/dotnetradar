using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 报警服务接口
    /// </summary>
    public interface IAlarmService
    {
        /// <summary>
        /// 创建报警记录
        /// </summary>
        /// <param name="alarmRecord">报警记录</param>
        /// <returns>是否创建成功</returns>
        Task<bool> CreateAlarmRecordAsync(AlarmRecord alarmRecord);

        /// <summary>
        /// 查询报警记录
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>报警记录列表</returns>
        Task<List<AlarmRecord>> QueryAlarmRecordsAsync(AlarmQueryRequest request);

        /// <summary>
        /// 统计报警数量
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="ruleIds">规则ID数组</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>各级别报警数量</returns>
        Task<Dictionary<AlarmLevel, int>> GetAlarmCountByLevelAsync(string projectId, string[] ruleIds, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 更新报警处理状态
        /// </summary>
        /// <param name="handleId">处理ID</param>
        /// <param name="handleStatus">处理状态</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateAlarmHandleStatusAsync(string handleId, string handleStatus);

        /// <summary>
        /// 查询未扫描的报警规则
        /// </summary>
        /// <param name="request">扫描请求</param>
        /// <returns>未扫描的报警记录</returns>
        Task<List<AlarmRecord>> QueryUnscannedAlarmRulesAsync(AlarmQueryRequest request);

        /// <summary>
        /// 批量更新扫描状态
        /// </summary>
        /// <param name="handleIds">处理ID数组</param>
        /// <param name="scanStatus">扫描状态</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateScanStatusAsync(string[] handleIds, string scanStatus);
    }
}
