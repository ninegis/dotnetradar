using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 高程图颜色配置实体
    /// </summary>
    [Table("terrain_color_configs")]
    public class TerrainColorConfigEntity
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
        /// 配色方案类型 (0:线性, 1:分类)
        /// </summary>
        [Column("color_scheme_type")]
        public int ColorSchemeType { get; set; } = 0;

        /// <summary>
        /// 最小高程值（米）
        /// </summary>
        [Column("min_elevation")]
        public double MinElevation { get; set; } = 0;

        /// <summary>
        /// 最大高程值（米）
        /// </summary>
        [Column("max_elevation")]
        public double MaxElevation { get; set; } = 1000;

        /// <summary>
        /// HSL色相起始值（0-360）
        /// </summary>
        [Column("hsl_h_start")]
        public int HslHStart { get; set; } = 120;  // 绿色（低海拔）

        /// <summary>
        /// HSL色相结束值（0-360）
        /// </summary>
        [Column("hsl_h_end")]
        public int HslHEnd { get; set; } = 0;  // 红色（高海拔）

        /// <summary>
        /// 饱和度（0-1）
        /// </summary>
        [Column("hsl_s")]
        public double HslS { get; set; } = 1.0;

        /// <summary>
        /// 亮度（0-1）
        /// </summary>
        [Column("hsl_l")]
        public double HslL { get; set; } = 0.5;

        /// <summary>
        /// 分类数量（分类配色时使用）
        /// </summary>
        [Column("class_count")]
        public int ClassCount { get; set; } = 5;

        /// <summary>
        /// 是否自适应范围
        /// </summary>
        [Column("auto_adapt_range")]
        public bool AutoAdaptRange { get; set; } = true;

        /// <summary>
        /// 自适应缓冲比例（0-1）
        /// </summary>
        [Column("adapt_buffer_ratio")]
        public double AdaptBufferRatio { get; set; } = 0.1;

        /// <summary>
        /// 自定义颜色区间（JSON格式，分类配色时使用）
        /// 格式: [{"min":0,"max":200,"color":"#00FF00","label":"平原"}]
        /// </summary>
        [Column("custom_ranges")]
        public string? CustomRanges { get; set; }

        /// <summary>
        /// 是否启用（0:否, 1:是）
        /// </summary>
        [Column("enable")]
        public bool Enable { get; set; } = false;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }
}

