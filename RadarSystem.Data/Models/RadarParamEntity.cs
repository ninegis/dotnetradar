using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 雷达基础参数实体（拆分为独立字段，不使用JSON）
    /// </summary>
    [Table("radar_params")]
    public class RadarParamEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("device_id")]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        // ========== 雷达基础参数 ==========
        
        /// <summary>
        /// 图像起始角度（度）
        /// </summary>
        [Column("img_angle_start")]
        public double ImgAngleStart { get; set; } = 0;

        /// <summary>
        /// 图像结束角度（度）
        /// </summary>
        [Column("img_angle_end")]
        public double ImgAngleEnd { get; set; } = 360;

        /// <summary>
        /// 最小距离（米）
        /// </summary>
        [Column("rng_min")]
        public double RngMin { get; set; } = 0;

        /// <summary>
        /// 最大距离（米）
        /// </summary>
        [Column("rng_max")]
        public double RngMax { get; set; } = 1000;

        /// <summary>
        /// 频段
        /// </summary>
        [Column("freq_band")]
        [MaxLength(32)]
        public string FreqBand { get; set; } = "0";

        /// <summary>
        /// 天线波束半角（度）
        /// </summary>
        [Column("ante_beam_half")]
        public double AnteBeamHalf { get; set; } = 60;

        /// <summary>
        /// 数据版本
        /// </summary>
        [Column("data_version")]
        [MaxLength(32)]
        public string DataVersion { get; set; } = "0";

        /// <summary>
        /// 模型选择（MIMO Lite专用）
        /// </summary>
        [Column("model_select")]
        [MaxLength(32)]
        public string? ModelSelect { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }
}

