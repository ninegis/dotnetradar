using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 告警联系人实体
    /// </summary>
    [Table("alarm_contacts")]
    public class AlarmContactEntity
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
        [Column("name")]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [Column("email")]
        [MaxLength(256)]
        public string? Email { get; set; }

        [Column("phone")]
        [MaxLength(32)]
        public string? Phone { get; set; }

        [Column("alarm_level")]
        public int AlarmLevel { get; set; } = 1; // 1-4

        [Column("enable")]
        public bool Enable { get; set; } = true;

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
    /// 短信配置实体（对应前端smsNotifyConfig）
    /// </summary>
    [Table("sms_configs")]
    public class SmsConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用短信通知
        /// </summary>
        [Column("enable")]
        public bool Enable { get; set; } = false;

        /// <summary>
        /// 短信推送通道（00:无模板, 01:阿里云）
        /// </summary>
        [Column("notify_channel")]
        [MaxLength(16)]
        public string NotifyChannel { get; set; } = "00";

        /// <summary>
        /// 访问密钥ID（AccessKey ID）
        /// </summary>
        [Column("access_key_id")]
        [MaxLength(256)]
        public string? AccessKeyId { get; set; }

        /// <summary>
        /// 访问密钥（AccessKey Secret）
        /// </summary>
        [Column("access_key_secret")]
        [MaxLength(256)]
        public string? AccessKeySecret { get; set; }

        /// <summary>
        /// 短信签名
        /// </summary>
        [Column("sign_name")]
        [MaxLength(128)]
        public string? SignName { get; set; }

        /// <summary>
        /// 模板代码
        /// </summary>
        [Column("template_code")]
        [MaxLength(128)]
        public string? TemplateCode { get; set; }

        /// <summary>
        /// 短信服务商（保留字段）
        /// </summary>
        [Column("provider")]
        [MaxLength(64)]
        public string? Provider { get; set; }

        /// <summary>
        /// API密钥（保留字段）
        /// </summary>
        [Column("api_key")]
        [MaxLength(256)]
        public string? ApiKey { get; set; }

        /// <summary>
        /// API密钥Secret（保留字段）
        /// </summary>
        [Column("api_secret")]
        [MaxLength(256)]
        public string? ApiSecret { get; set; }

        /// <summary>
        /// 模板内容（保留字段）
        /// </summary>
        [Column("template_content")]
        public string? TemplateContent { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }
}
