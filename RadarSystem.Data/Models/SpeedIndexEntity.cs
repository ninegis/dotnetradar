using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 速度指标配置实体（存储图像差分分析的时间单位配置）
    /// </summary>
    [Table("speed_indices")]
    public class SpeedIndexEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// 时间单位（多个时间单位用逗号分隔，如："02,03,04"）
        /// 02:30分钟, 03:1小时, 04:1天, 07:3天, 05:1周, 06:1月
        /// </summary>
        [Column("time_units")]
        [MaxLength(128)]
        public string TimeUnits { get; set; } = "04"; // 默认1天

        /// <summary>
        /// 是否启用30分钟
        /// </summary>
        [Column("enable_30min")]
        public bool Enable30Min { get; set; } = false;

        /// <summary>
        /// 是否启用1小时
        /// </summary>
        [Column("enable_1hour")]
        public bool Enable1Hour { get; set; } = false;

        /// <summary>
        /// 是否启用1天
        /// </summary>
        [Column("enable_1day")]
        public bool Enable1Day { get; set; } = true;

        /// <summary>
        /// 是否启用3天
        /// </summary>
        [Column("enable_3day")]
        public bool Enable3Day { get; set; } = false;

        /// <summary>
        /// 是否启用1周
        /// </summary>
        [Column("enable_1week")]
        public bool Enable1Week { get; set; } = false;

        /// <summary>
        /// 是否启用1月
        /// </summary>
        [Column("enable_1month")]
        public bool Enable1Month { get; set; } = false;

        /// <summary>
        /// 是否自动生成速度图像
        /// </summary>
        [Column("auto_gen_speed_image")]
        public bool AutoGenSpeedImage { get; set; } = false;

        /// <summary>
        /// 速度图像生成间隔（分钟）
        /// </summary>
        [Column("speed_image_interval")]
        public int SpeedImageInterval { get; set; } = 60;

        /// <summary>
        /// 是否自动生成加速度图像
        /// </summary>
        [Column("auto_gen_acceleration_image")]
        public bool AutoGenAccelerationImage { get; set; } = false;

        /// <summary>
        /// 加速度图像生成间隔（分钟）
        /// </summary>
        [Column("acceleration_image_interval")]
        public int AccelerationImageInterval { get; set; } = 120;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }
}


