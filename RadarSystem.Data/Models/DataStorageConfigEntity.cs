using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 数据存储配置实体
    /// </summary>
    [Table("data_storage_configs")]
    public class DataStorageConfigEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用自动清理
        /// </summary>
        [Column("auto_cleanup_enable")]
        public bool AutoCleanupEnable { get; set; } = false;

        /// <summary>
        /// 磁盘空间阈值百分比（0-100）
        /// </summary>
        [Column("disk_threshold_percent")]
        public int DiskThresholdPercent { get; set; } = 80;

        /// <summary>
        /// 数据保留天数
        /// </summary>
        [Column("data_retention_days")]
        public int DataRetentionDays { get; set; } = 90;

        /// <summary>
        /// 是否删除原始数据
        /// </summary>
        [Column("delete_raw_data")]
        public bool DeleteRawData { get; set; } = false;

        /// <summary>
        /// 是否删除图像数据
        /// </summary>
        [Column("delete_image_data")]
        public bool DeleteImageData { get; set; } = false;

        /// <summary>
        /// 是否删除分析结果
        /// </summary>
        [Column("delete_analysis_data")]
        public bool DeleteAnalysisData { get; set; } = false;

        /// <summary>
        /// 图像压缩质量（1-100）
        /// </summary>
        [Column("image_quality")]
        public int ImageQuality { get; set; } = 85;

        /// <summary>
        /// 是否启用图像压缩
        /// </summary>
        [Column("image_compression_enable")]
        public bool ImageCompressionEnable { get; set; } = true;

        /// <summary>
        /// 数据存储路径
        /// </summary>
        [Column("storage_path")]
        [MaxLength(512)]
        public string StoragePath { get; set; } = "./Data";

        /// <summary>
        /// 备份存储路径
        /// </summary>
        [Column("backup_path")]
        [MaxLength(512)]
        public string? BackupPath { get; set; }

        /// <summary>
        /// 是否启用自动备份
        /// </summary>
        [Column("auto_backup_enable")]
        public bool AutoBackupEnable { get; set; } = false;

        /// <summary>
        /// 备份间隔（天）
        /// </summary>
        [Column("backup_interval_days")]
        public int BackupIntervalDays { get; set; } = 7;

        /// <summary>
        /// 最大备份数量
        /// </summary>
        [Column("max_backup_count")]
        public int MaxBackupCount { get; set; } = 5;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }
}

