using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 色条配置实体（位移/散射）
    /// </summary>
    [Table("colorbar_configs")]
    public class ColorBarConfigEntity
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
        /// 模式：defo(位移) / scat(散射)
        /// </summary>
        [Required]
        [Column("mode")]
        [MaxLength(16)]
        public string Mode { get; set; } = "defo";

        /// <summary>
        /// 最小值
        /// </summary>
        [Column("min_value")]
        public double MinValue { get; set; } = -100;

        /// <summary>
        /// 最大值
        /// </summary>
        [Column("max_value")]
        public double MaxValue { get; set; } = 100;

        /// <summary>
        /// HSL色相起始值（0-240）
        /// </summary>
        [Column("hsl_h_start")]
        public int HslHStart { get; set; } = 0;

        /// <summary>
        /// HSL色相结束值（0-360）
        /// </summary>
        [Column("hsl_h_end")]
        public int HslHEnd { get; set; } = 240;

        /// <summary>
        /// HSL色相渐变方向 (0:正向/顺时针, 1:反向/逆时针)
        /// </summary>
        [Column("hsl_direction")]
        public int HslDirection { get; set; } = 0;

        /// <summary>
        /// HSL饱和度 (0-1, 0=灰色, 1=鲜艳)
        /// </summary>
        [Column("hsl_s")]
        public double HslS { get; set; } = 1.0;

        /// <summary>
        /// HSL亮度 (0-1, 0=黑色, 0.5=标准, 1=白色)
        /// </summary>
        [Column("hsl_l")]
        public double HslL { get; set; } = 0.5;

        /// <summary>
        /// 透明度（0-1）
        /// </summary>
        [Column("filter_alpha")]
        public double FilterAlpha { get; set; } = 0.8;

        /// <summary>
        /// 过滤最小值（-1000-1000）
        /// </summary>
        [Column("filter_min")]
        public double FilterMin { get; set; } = -1000;

        /// <summary>
        /// 过滤最大值（-1000-1000）
        /// </summary>
        [Column("filter_max")]
        public double FilterMax { get; set; } = 1000;

        /// <summary>
        /// 是否启用透明通道（0:否, 1:是）
        /// </summary>
        [Column("filter_enable")]
        public int FilterEnable { get; set; } = 0;

        /// <summary>
        /// 配色方案类型 (0:线性, 1:分类)
        /// </summary>
        [Column("color_scheme_type")]
        public int ColorSchemeType { get; set; } = 0;

        /// <summary>
        /// 分类数量（分类配色时使用）
        /// </summary>
        [Column("class_count")]
        public int ClassCount { get; set; } = 5;

        /// <summary>
        /// 是否自适应范围（根据实际数据自动调整min/max）
        /// </summary>
        [Column("auto_adapt_range")]
        public bool AutoAdaptRange { get; set; } = false;

        /// <summary>
        /// 自适应缓冲比例（0-1，如0.1表示上下各扩展10%）
        /// </summary>
        [Column("adapt_buffer_ratio")]
        public double AdaptBufferRatio { get; set; } = 0.1;

        /// <summary>
        /// 自定义颜色区间（JSON格式，分类配色时使用）
        /// 格式: [{"min":-100,"max":-50,"color":"#0000FF","label":"蓝色"}]
        /// </summary>
        [Column("custom_ranges")]
        public string? CustomRanges { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
    }
}



