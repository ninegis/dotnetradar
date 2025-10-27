using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 雷达图像实体
    /// </summary>
    [Table("radar_images")]
    public class RadarImageEntity
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

        [Required]
        [Column("file_name")]
        [MaxLength(512)]
        public string FileName { get; set; } = string.Empty;

        [Column("file_path")]
        [MaxLength(1024)]
        public string? FilePath { get; set; }

        [Column("file_url")]
        [MaxLength(1024)]
        public string? FileUrl { get; set; }

        [Column("file_size")]
        public long FileSize { get; set; }

        [Column("image_type")]
        [MaxLength(32)]
        public string ImageType { get; set; } = string.Empty; // terrain/defo/scat/speed

        [Column("duration")]
        public int Duration { get; set; }

        [Column("sequence")]
        public int Sequence { get; set; }

        [Column("time_unit")]
        [MaxLength(16)]
        public string? TimeUnit { get; set; }

        [Column("status")]
        [MaxLength(32)]
        public string Status { get; set; } = "pending"; // pending/processing/completed/failed

        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("capture_time")]
        public DateTime CaptureTime { get; set; } = DateTime.Now;

        [Column("metadata_json")]
        public string? MetadataJson { get; set; } // 图像元数据JSON

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }

    /// <summary>
    /// 图像生成任务实体
    /// </summary>
    [Table("image_generation_tasks")]
    public class ImageGenerationTaskEntity
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

        [Column("task_type")]
        [MaxLength(32)]
        public string TaskType { get; set; } = "generate"; // generate/restore

        [Column("parameters_json")]
        public string? ParametersJson { get; set; }

        [Column("status")]
        [MaxLength(32)]
        public string Status { get; set; } = "pending"; // pending/running/completed/failed

        [Column("progress")]
        public int Progress { get; set; } = 0; // 0-100

        [Column("result_image_id")]
        [MaxLength(64)]
        public string? ResultImageId { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }
}

