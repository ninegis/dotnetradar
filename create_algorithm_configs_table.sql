对算法配置表是按以下字段进行创建的，请删除无法的字段 代码以及前端代码：
 
CREATE TABLE algorithm_configs (
    id TEXT PRIMARY KEY,
    project_id TEXT NOT NULL,
    device_id TEXT NOT NULL,
    -- 新32个算法参数字段
    mon_mode TEXT DEFAULT 'Z',
    pha_flt_type_ctrl INTEGER DEFAULT 0,
    flt_half_win_len INTEGER DEFAULT 1,
    atm_flt_en REAL DEFAULT 0.0,
    mean_wgt REAL DEFAULT 0.0,
    cmp_def_thr INTEGER DEFAULT 1,
    cmp_mult INTEGER DEFAULT 1,
    amp_det_thr REAL DEFAULT 0.0,
    atm_flt_para_a REAL DEFAULT 0.0,
    atm_flt_para_b REAL DEFAULT 0.0,
    atm_corr_thr_2nd_1 REAL DEFAULT 0.0,
    atm_comp_upd_per REAL DEFAULT 0.0,
    atm_corr_thr_2nd_2 REAL DEFAULT 0.0,
    def_img_decim TEXT DEFAULT '1',
    cplx_img_decim TEXT DEFAULT '1',
    atm_corr_alg TEXT DEFAULT '0',
    atm_pha_err_est_dist_1 REAL DEFAULT 0.0,
    atm_pha_err_est_dist_2 REAL DEFAULT 0.0,
    std_dev_wgt REAL DEFAULT 0.0,
    short_def_acc_para REAL DEFAULT 0.0,
    denoise_thr INTEGER DEFAULT 1,
    is_noise_eq REAL DEFAULT 0.0,
    noise_eq_type REAL DEFAULT 0.0,
    amp_dev_sel_thr_init REAL DEFAULT 0.1,
    coh_coe_thr_init REAL DEFAULT 0.01,
    corr_coeff_eff_ps_pts REAL DEFAULT 0.0,
    eff_ps_pts REAL DEFAULT 0.0,
    ifg_pha_res_thr REAL DEFAULT 0.0,
    sing_pnt_thr REAL DEFAULT 0.0,
    ps_pnt_sens INTEGER DEFAULT 1,
    ps_thr_adj_coeff REAL DEFAULT 0.0,
    coh_half_win_len INTEGER DEFAULT 1,
    create_time TEXT NOT NULL,
    update_time TEXT,
    FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT,
    FOREIGN KEY (device_id) REFERENCES Devices(DeviceId) ON DELETE RESTRICT,
    UNIQUE (project_id, device_id)
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_algorithm_configs_project_id ON algorithm_configs(project_id);
CREATE UNIQUE INDEX IF NOT EXISTS idx_algorithm_configs_project_device ON algorithm_configs(project_id, device_id);

