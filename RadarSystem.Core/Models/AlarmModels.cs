using System;
using System.Collections.Generic;

namespace RadarSystem.Core.Models
{
    /// <summary>
    /// 报警规则模型
    /// </summary>
    public class AlarmRule
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string? RuleDescription { get; set; }
        public string? AlarmContent { get; set; }
        public string RuleOperator { get; set; } = ">"; // >、<、>=、<=、=
        public int AlarmLevel { get; set; } = 1; // 1-4
        public bool Enable { get; set; } = true;
        public double AlarmThreshold { get; set; }
        public string? DevicesJson { get; set; }
        public string? GeoMarkArrayJson { get; set; }
        public string? DataSource { get; set; }
        public string? TargetType { get; set; }
        public string? Mode { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 报警记录模型
    /// </summary>
    public class AlarmRecord
    {
        public string HandleId { get; set; } = string.Empty;
        public string RuleId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool AlarmStatus { get; set; }
        public AlarmLevel AlarmLevel { get; set; }
        public string AlarmContent { get; set; } = string.Empty;
        public string HandleStatus { get; set; } = "00";
        public string ScanStatus { get; set; } = "unscanned";
    }

    /// <summary>
    /// 报警处理记录模型
    /// </summary>
    public class AlarmHandleRecord
    {
        public string HandleId { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string Video { get; set; } = string.Empty;
        public string HandleDescription { get; set; } = string.Empty;
        public DateTime HandleTime { get; set; }
        public string Handler { get; set; } = string.Empty;
    }

    /// <summary>
    /// 报警级别枚举
    /// </summary>
    public enum AlarmLevel
    {
        Blue = 1,    // 蓝色
        Yellow = 2,  // 黄色
        Orange = 3,  // 橙色
        Red = 4      // 红色
    }

    /// <summary>
    /// 报警查询请求模型
    /// </summary>
    public class AlarmQueryRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string[] RuleIds { get; set; } = Array.Empty<string>();
        public string[] Status { get; set; } = Array.Empty<string>();
        public string[] Type { get; set; } = Array.Empty<string>();
        public string StartDateTime { get; set; } = string.Empty;
        public string EndDateTime { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageRowSize { get; set; } = 20;
        public string Timezone { get; set; } = "Asia/Shanghai";
        public int Count { get; set; } = 1;
    }

    /// <summary>
    /// 报警数据插入请求模型
    /// </summary>
    public class AlarmDataInsertRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string RuleId { get; set; } = string.Empty;
        public bool AlarmStatus { get; set; }
        public string CurrentTime { get; set; } = string.Empty;
        public string AlarmLevel { get; set; } = string.Empty;
        public string AlarmContent { get; set; } = string.Empty;
        public string HandleId { get; set; } = string.Empty;
        public string HandleStatus { get; set; } = string.Empty;
    }
}
