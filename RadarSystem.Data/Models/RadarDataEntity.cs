using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 雷达数据实体
    /// </summary>
    [Table("RadarData")]
    public class RadarDataEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [StringLength(10)]
        public string DataType { get; set; } = string.Empty;

        public int Sequence { get; set; }

        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        public int Duration { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public int TaskId { get; set; }

        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        public float RangeResolution { get; set; }

        public float AngleResolution { get; set; }

        public float RangeMin { get; set; }

        public float AngleMin { get; set; }

        public int RangeNumber { get; set; }

        public int AngleNumber { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// 报警记录实体
    /// </summary>
    [Table("AlarmRecords")]
    public class AlarmRecordEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string HandleId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RuleId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        public bool AlarmStatus { get; set; }

        public int AlarmLevel { get; set; }

        [StringLength(500)]
        public string AlarmContent { get; set; } = string.Empty;

        [StringLength(20)]
        public string HandleStatus { get; set; } = "00";

        [StringLength(20)]
        public string ScanStatus { get; set; } = "unscanned";

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// 报警处理记录实体
    /// </summary>
    [Table("AlarmHandleRecords")]
    public class AlarmHandleRecordEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string HandleId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProjectId { get; set; } = string.Empty;

        [StringLength(200)]
        public string Photo { get; set; } = string.Empty;

        [StringLength(200)]
        public string Video { get; set; } = string.Empty;

        [StringLength(1000)]
        public string HandleDescription { get; set; } = string.Empty;

        public DateTime HandleTime { get; set; }

        [StringLength(50)]
        public string Handler { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// 项目信息实体
    /// </summary>
    [Table("Projects")]
    public class ProjectEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        // 联系人信息
        [StringLength(50)]
        public string ContactPerson { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        // 地理位置信息
        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public double Elevation { get; set; }

        // 时间信息
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// 设备信息实体
    /// </summary>
    [Table("Devices")]
    public class DeviceEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [StringLength(50)]
        public string DeviceType { get; set; } = string.Empty;

        public int DeviceTypeCode { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Offline";

        // ✅ 地理位置信息（独立字段）
        public double Longitude { get; set; }
        
        public double Latitude { get; set; }
        
        public double Elevation { get; set; }

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(100)]
        public string IpAddress { get; set; } = string.Empty;

        public int Port { get; set; }

        [StringLength(200)]
        public string MqttTopic { get; set; } = string.Empty;

        // ✅ 雷达特有信息
        [StringLength(50)]
        public string FactoryId { get; set; } = string.Empty;  // 出厂ID
        
        public double Orientation { get; set; }  // 零点朝向（度）

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime LastUpdateTime { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
