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
    /// 算法配置实体（新32字段版本）
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

        // ========== 算法参数（32个字段） ==========
        
        // 1. 监测模式 (枚举型 Z/B/S)
        [Column("mon_mode")]
        [MaxLength(16)]
        public string MonMode { get; set; } = "Z";

        // 2. 相位滤波类型选择控制变量 (枚举型 0/1)
        [Column("pha_flt_type_ctrl")]
        public int PhaFltTypeCtrl { get; set; } = 0;

        // 3. 滤波半窗长 (整数，默认1)
        [Column("flt_half_win_len")]
        public int FltHalfWinLen { get; set; } = 1;

        // 4. 大气滤波使能 (浮点，默认0.0)
        [Column("atm_flt_en")]
        public double AtmFltEn { get; set; } = 0.0;

        // 5. 均值加权 (浮点，默认0.0)
        [Column("mean_wgt")]
        public double MeanWgt { get; set; } = 0.0;

        // 6. 压缩形变阈值 (整数，默认1)
        [Column("cmp_def_thr")]
        public int CmpDefThr { get; set; } = 1;

        // 7. 压缩倍数 (整数，默认1)
        [Column("cmp_mult")]
        public int CmpMult { get; set; } = 1;

        // 8. 幅度检测门限 (浮点，默认0.0)
        [Column("amp_det_thr")]
        public double AmpDetThr { get; set; } = 0.0;

        // 9. 大气滤波参数 A (浮点，默认0.0)
        [Column("atm_flt_para_a")]
        public double AtmFltParaA { get; set; } = 0.0;

        // 10. 大气滤波参数 B (浮点，默认0.0)
        [Column("atm_flt_para_b")]
        public double AtmFltParaB { get; set; } = 0.0;

        // 11. 第二阶段大气校正门限 (atmConstThread_2nd) (浮点，默认0.0)
        [Column("atm_corr_thr_2nd_1")]
        public double AtmCorrThr2nd_1 { get; set; } = 0.0;

        // 12. 二次大气补偿更新周期 (浮点，默认0.0)
        [Column("atm_comp_upd_per")]
        public double AtmCompUpdPer { get; set; } = 0.0;

        // 13. 第二阶段大气校正门限 (atmMode) (浮点，默认0.0)
        [Column("atm_corr_thr_2nd_2")]
        public double AtmCorrThr2nd_2 { get; set; } = 0.0;

        // 14. 形变图像抽帧 (枚举型)
        [Column("def_img_decim")]
        [MaxLength(16)]
        public string DefImgDecim { get; set; } = "1";

        // 15. 复数图图像抽帧 (枚举型)
        [Column("cplx_img_decim")]
        [MaxLength(16)]
        public string CplxImgDecim { get; set; } = "1";

        // 16. 大气校正算法 (枚举型)
        [Column("atm_corr_alg")]
        [MaxLength(16)]
        public string AtmCorrAlg { get; set; } = "0";

        // 17. 大气相位误差估计距离 (atmPhaEstAngle) (浮点，默认0.0)
        [Column("atm_pha_err_est_dist_1")]
        public double AtmPhaErrEstDist_1 { get; set; } = 0.0;

        // 18. 大气相位误差估计距离 (atmPhaEstRng) (浮点，默认0.0)
        [Column("atm_pha_err_est_dist_2")]
        public double AtmPhaErrEstDist_2 { get; set; } = 0.0;

        // 19. 标准差加权 (浮点，默认0.0)
        [Column("std_dev_wgt")]
        public double StdDevWgt { get; set; } = 0.0;

        // 20. 短时形变量积参数 (浮点，默认0.0)
        [Column("short_def_acc_para")]
        public double ShortDefAccPara { get; set; } = 0.0;

        // 21. 去噪门限 (整数，默认1)
        [Column("denoise_thr")]
        public int DenoiseThr { get; set; } = 1;

        // 22. 是否噪声均衡 (浮点，默认0.0)
        [Column("is_noise_eq")]
        public double IsNoiseEq { get; set; } = 0.0;

        // 23. 噪声均衡类型 (浮点，默认0.0)
        [Column("noise_eq_type")]
        public double NoiseEqType { get; set; } = 0.0;

        // 24. 幅度离差选择门限初值 (浮点，默认0.1)
        [Column("amp_dev_sel_thr_init")]
        public double AmpDevSelThrInit { get; set; } = 0.1;

        // 25. 相干系数阈值初值 (浮点，默认0.01)
        [Column("coh_coe_thr_init")]
        public double CohCoeThrInit { get; set; } = 0.01;

        // 26. 相关系数有效 PS 点 (浮点，默认0.0)
        [Column("corr_coeff_eff_ps_pts")]
        public double CorrCoeffEffPSPts { get; set; } = 0.0;

        // 27. 有效 PS 点 (浮点，默认0.0)
        [Column("eff_ps_pts")]
        public double EffPSPts { get; set; } = 0.0;

        // 28. 干涉相位残差阈值 (浮点，默认0.0)
        [Column("ifg_pha_res_thr")]
        public double IfgPhaResThr { get; set; } = 0.0;

        // 29. 奇异点门限 (浮点，默认0.0)
        [Column("sing_pnt_thr")]
        public double SingPntThr { get; set; } = 0.0;

        // 30. PS 点灵敏度 (整数，默认1)
        [Column("ps_pnt_sens")]
        public int PSPntSens { get; set; } = 1;

        // 31. PS 门限调节系数 (浮点，默认0.0)
        [Column("ps_thr_adj_coeff")]
        public double PSThrAdjCoeff { get; set; } = 0.0;

        // 32. 相干半窗长 (整数，默认1)
        [Column("coh_half_win_len")]
        public int CohHalfWinLen { get; set; } = 1;

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ProjectEntity? Project { get; set; }
        public virtual DeviceEntity? Device { get; set; }
    }
}

