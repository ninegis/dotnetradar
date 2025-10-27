using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 图层实体
    /// </summary>
    [Table("layers")]
    public class LayerEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Column("oid")]
        [MaxLength(64)]
        public string Oid { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [Column("type")]
        [MaxLength(64)]
        public string? Type { get; set; }

        [Column("url")]
        [MaxLength(1024)]
        public string? Url { get; set; }

        [Column("project_id")]
        [MaxLength(64)]
        public string? ProjectId { get; set; } // 图层所属项目（可为空，表示全局图层）

        [Column("user_id")]
        [MaxLength(64)]
        public string? UserId { get; set; }

        [Column("post_id")]
        [MaxLength(64)]
        public string? PostId { get; set; }

        [Column("division_id")]
        [MaxLength(64)]
        public string? DivisionId { get; set; }

        [Column("org_id")]
        [MaxLength(64)]
        public string? OrgId { get; set; }

        [Column("tree_id")]
        [MaxLength(64)]
        public string? TreeId { get; set; }

        [Column("enable")]
        public bool Enable { get; set; } = true;

        [Column("show")]
        public bool Show { get; set; } = true;

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }

    /// <summary>
    /// 系统日志实体
    /// </summary>
    [Table("system_logs")]
    public class SystemLogEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("log_type")]
        [MaxLength(32)]
        public string LogType { get; set; } = "operation"; // operation/error/warning/info

        [Column("operate_content")]
        public string? OperateContent { get; set; }

        [Column("operate_username")]
        [MaxLength(128)]
        public string? OperateUsername { get; set; }

        [Column("project_code")]
        [MaxLength(64)]
        public string? ProjectCode { get; set; }

        [Column("project_name")]
        [MaxLength(256)]
        public string? ProjectName { get; set; }

        [Column("ip_address")]
        [MaxLength(64)]
        public string? IpAddress { get; set; }

        [Column("address_info")]
        [MaxLength(256)]
        public string? AddressInfo { get; set; }

        [Column("user_agent")]
        [MaxLength(512)]
        public string? UserAgent { get; set; }

        [Column("request_url")]
        [MaxLength(1024)]
        public string? RequestUrl { get; set; }

        [Column("request_method")]
        [MaxLength(16)]
        public string? RequestMethod { get; set; }

        [Column("request_params")]
        public string? RequestParams { get; set; }

        [Column("response_time")]
        public int? ResponseTime { get; set; } // 毫秒

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 系统配置实体
    /// </summary>
    [Table("system_configs")]
    public class SystemConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("config_key")]
        [MaxLength(128)]
        public string ConfigKey { get; set; } = string.Empty;

        [Column("config_value")]
        public string? ConfigValue { get; set; }

        [Column("config_type")]
        [MaxLength(32)]
        public string ConfigType { get; set; } = "string"; // string/number/boolean/json

        [Column("description")]
        [MaxLength(512)]
        public string? Description { get; set; }

        [Column("category")]
        [MaxLength(64)]
        public string? Category { get; set; } // disk/network/alarm/radar

        [Column("is_public")]
        public bool IsPublic { get; set; } = false; // 是否公开可见

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 磁盘存储配置实体
    /// </summary>
    [Table("disk_storage_configs")]
    public class DiskStorageConfigEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("disc_space_percentage")]
        public double DiscSpacePercentage { get; set; } = 80.0;

        [Column("delete_file")]
        public bool DeleteFile { get; set; } = false;

        [Column("total_space")]
        public long TotalSpace { get; set; }

        [Column("used_space")]
        public long UsedSpace { get; set; }

        [Column("available_space")]
        public long AvailableSpace { get; set; }

        [Column("warning_threshold")]
        public int WarningThreshold { get; set; } = 80;

        [Column("error_threshold")]
        public int ErrorThreshold { get; set; } = 90;

        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 雷达参数配置实体
    /// </summary>
    [Table("radar_param_configs")]
    public class RadarParamConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("device_id")]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        [Column("param_type")]
        [MaxLength(32)]
        public string ParamType { get; set; } = "base"; // base/algo/speed/colorbar/hiddenarea

        [Column("parameters_json")]
        public string ParametersJson { get; set; } = "{}";

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }
}

