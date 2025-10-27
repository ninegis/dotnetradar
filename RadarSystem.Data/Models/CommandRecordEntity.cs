using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 指令下发记录实体
    /// </summary>
    [Table("command_records")]
    public class CommandRecordEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("device_id")]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [Column("command_type")]
        [MaxLength(64)]
        public string CommandType { get; set; } = string.Empty; // 11(参数控制)等

        [Required]
        [Column("command_content")]
        public string CommandContent { get; set; } = string.Empty; // 指令内容JSON

        [Column("command_params_json")]
        public string? CommandParamsJson { get; set; } // 指令参数JSON

        [Column("operator")]
        [MaxLength(128)]
        public string? Operator { get; set; } // 操作人员

        [Column("status")]
        [MaxLength(32)]
        public string Status { get; set; } = "pending"; // pending/sent/success/failed

        [Column("send_time")]
        public DateTime? SendTime { get; set; }

        [Column("response_time")]
        public DateTime? ResponseTime { get; set; }

        [Column("response_content")]
        public string? ResponseContent { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("retry_count")]
        public int RetryCount { get; set; } = 0;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }

    /// <summary>
    /// 算法配置实体
    /// </summary>
    [Table("algorithm_configs")]
    public class AlgorithmConfigEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("project_id")]
        [MaxLength(64)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [Column("device_id")]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        // 算法参数（对应前端的algorithmParam）
        [Column("filter_type")]
        public int FilterType { get; set; } = 0; // 滤波类型

        [Column("alpha_filter")]
        public int AlphaFilter { get; set; } = 0; // Alpha滤波参数

        [Column("beta_filter")]
        public int BetaFilter { get; set; } = 0; // Beta滤波参数

        [Column("de_noise_thread")]
        public int DeNoiseThread { get; set; } = 0; // 去噪阈值

        [Column("sens_coef")]
        public int SensCoef { get; set; } = 0; // 灵敏度系数

        [Column("defo_image_dec")]
        public string DefoImageDec { get; set; } = "1"; // 形变图像抽取

        [Column("scat_image_dec")]
        public string ScatImageDec { get; set; } = "1"; // 散射图像抽取

        [Column("win_coheren")]
        public int WinCoheren { get; set; } = 0; // 窗口相干

        [Column("atm_pha_err_est_func_switch")]
        public string AtmPhaErrEstFuncSwitch { get; set; } = "0"; // 大气相位误差估计开关

        [Column("filter_width")]
        public int FilterWidth { get; set; } = 0; // 滤波宽度

        [Column("monitor_mode")]
        public string MonitorMode { get; set; } = "0"; // 监测模式

        [Column("ipv4")]
        [MaxLength(64)]
        public string? Ipv4 { get; set; } // IP地址

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }
}

