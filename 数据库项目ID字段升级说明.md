# 数据库表ProjectId字段全面升级说明

## 📅 更新日期
2025-10-24

## 🎯 升级目标
确保系统中所有相关表都包含ProjectId字段，使所有操作都基于当前选中的项目进行，实现多项目隔离。

---

## ✅ 已完成的工作

### 1. 数据库实体模型更新

#### 1.1 创建新表实体（CommandRecordEntity.cs）
- **指令下发记录表** (`command_records`)
  - 字段：id, project_id, device_id, command_type, command_content, command_params_json, operator, status, send_time, response_time, response_content, error_message, retry_count, create_time, update_time
  - 用途：记录所有下发给雷达设备的指令历史
  
- **算法配置表** (`algorithm_configs`)
  - 字段：id, project_id, device_id, filter_type, alpha_filter, beta_filter, de_noise_thread, sens_coef, defo_image_dec, scat_image_dec, win_coheren, atm_pha_err_est_func_switch, filter_width, monitor_mode, ipv4, config_json, create_time, update_time
  - 用途：存储每个设备的算法配置参数（对应前端的algorithmParam）

#### 1.2 已有表添加ProjectId字段
| 表名 | 文件 | ProjectId类型 | 说明 |
|------|------|--------------|------|
| users | UserEntity.cs | 可空TEXT | 用户所属项目（NULL表示管理员） |
| AlarmHandleRecords | RadarDataEntity.cs | 必填TEXT | 报警处理记录所属项目 |
| layers | SystemEntities.cs | 可空TEXT | 图层所属项目（NULL表示全局图层） |
| tilt_motor_configs | ConfigEntities.cs | 必填TEXT | 俯仰电机配置所属项目 |

#### 1.3 已有ProjectId的表（确认无需修改）
- Projects（主表）
- Devices
- AlarmRecords
- alarm_rules
- alarm_contacts
- sms_configs
- color_settings
- geo_marks
- panel_configs
- image_marks
- image_analysis_configs
- project_configurations
- image_diff_analysis_configs
- hidden_area_analysis_configs
- radar_param_configs
- radar_images
- image_generation_tasks

---

### 2. DbContext更新（RadarDbContext.cs）

#### 2.1 新增DbSet
```csharp
public DbSet<CommandRecordEntity> CommandRecords { get; set; }
public DbSet<AlgorithmConfigEntity> AlgorithmConfigs { get; set; }
```

#### 2.2 配置外键关系
- `CommandRecordEntity`: 与Projects、Devices建立外键（Restrict删除）
- `AlgorithmConfigEntity`: 与Projects、Devices建立外键（Restrict删除），每个设备一个算法配置（唯一约束）
- `TiltMotorConfigEntity`: 添加与Projects的外键关系
- `LayerEntity`: 添加与Projects的可空外键关系（项目删除时设为NULL）

#### 2.3 索引优化
- `command_records`: 
  - `idx_command_records_project_id`
  - `idx_command_records_device_id`
  - `idx_command_records_status`（复合索引：status + create_time）
  
- `algorithm_configs`:
  - `idx_algorithm_configs_project_id`
  - `idx_algorithm_configs_project_device`（唯一复合索引：project_id + device_id）

---

### 3. 数据库迁移逻辑（Program.cs）

#### 3.1 为现有表添加字段
```sql
-- Devices表
ALTER TABLE Devices ADD COLUMN Longitude REAL DEFAULT 0;
ALTER TABLE Devices ADD COLUMN Latitude REAL DEFAULT 0;
ALTER TABLE Devices ADD COLUMN Elevation REAL DEFAULT 0;
ALTER TABLE Devices ADD COLUMN FactoryId TEXT DEFAULT '';
ALTER TABLE Devices ADD COLUMN Orientation REAL DEFAULT 0;

-- users表
ALTER TABLE users ADD COLUMN ProjectId TEXT DEFAULT NULL;

-- AlarmHandleRecords表
ALTER TABLE AlarmHandleRecords ADD COLUMN ProjectId TEXT DEFAULT '';

-- layers表
ALTER TABLE layers ADD COLUMN project_id TEXT DEFAULT NULL;

-- tilt_motor_configs表
ALTER TABLE tilt_motor_configs ADD COLUMN project_id TEXT DEFAULT '';
```

#### 3.2 创建新表
```sql
-- 指令下发记录表
CREATE TABLE command_records (
    id TEXT PRIMARY KEY,
    project_id TEXT NOT NULL,
    device_id TEXT NOT NULL,
    command_type TEXT NOT NULL,
    command_content TEXT NOT NULL,
    command_params_json TEXT,
    operator TEXT,
    status TEXT DEFAULT 'pending',
    send_time TEXT,
    response_time TEXT,
    response_content TEXT,
    error_message TEXT,
    retry_count INTEGER DEFAULT 0,
    create_time TEXT NOT NULL,
    update_time TEXT,
    FOREIGN KEY (project_id) REFERENCES Projects(ProjectId),
    FOREIGN KEY (device_id) REFERENCES Devices(DeviceId)
);

-- 算法配置表
CREATE TABLE algorithm_configs (
    id TEXT PRIMARY KEY,
    project_id TEXT NOT NULL,
    device_id TEXT NOT NULL,
    filter_type INTEGER DEFAULT 0,
    alpha_filter INTEGER DEFAULT 0,
    beta_filter INTEGER DEFAULT 0,
    de_noise_thread INTEGER DEFAULT 0,
    sens_coef INTEGER DEFAULT 0,
    defo_image_dec TEXT DEFAULT '1',
    scat_image_dec TEXT DEFAULT '1',
    win_coheren INTEGER DEFAULT 0,
    atm_pha_err_est_func_switch TEXT DEFAULT '0',
    filter_width INTEGER DEFAULT 0,
    monitor_mode TEXT DEFAULT '0',
    ipv4 TEXT,
    config_json TEXT,
    create_time TEXT NOT NULL,
    update_time TEXT,
    FOREIGN KEY (project_id) REFERENCES Projects(ProjectId),
    FOREIGN KEY (device_id) REFERENCES Devices(DeviceId),
    UNIQUE (project_id, device_id)
);
```

---

## 🔄 前端需要的调整

### 1. 确保所有API调用携带当前项目ID

所有工具栏下的操作（告警规则、指令下发、数据管理、算法配置等）都应使用：
```javascript
// 获取当前选中的项目ID
const currentProjectId = store.projectInfo.projectSelected;

// 在API调用时传递
ApiRadar.someMethod(currentProjectId, ...otherParams);
```

### 2. 主要涉及的前端组件

1. **告警规则管理** (`AlarmRule.vue`)
   - 添加、修改、删除告警规则时使用`currentProjectId`

2. **告警联系人管理** (`AlarmContactList.vue`)
   - 添加、修改、删除联系人时使用`currentProjectId`

3. **指令下发** (`CommandIcon.vue`)
   - 下发指令时使用`currentProjectId`

4. **算法参数** (`AlgorithmParams.vue`)
   - 更新算法参数时使用`currentProjectId`

5. **雷达参数** (`RadarParams.vue`)
   - 已更新为使用当前项目和设备

6. **数据管理** (`DataGenerate.vue`, `DataRestore.vue`)
   - 数据生成和恢复时使用`currentProjectId`

7. **色条配置** (`ColorConfig.vue`)
   - 更新色条时使用`currentProjectId`

8. **危险区域配置** (`DangerConfig.vue`)
   - 更新危险区域时使用`currentProjectId`

---

## 📊 数据库表统计

| 类别 | 表数 | 说明 |
|------|------|------|
| 核心业务表 | 6 | Projects, Devices, RadarData, AlarmRecords, AlarmHandleRecords, users |
| 配置表 | 11 | alarm_rules, alarm_contacts, sms_configs, color_settings, geo_marks, image_analysis_configs, image_marks, panel_configs, project_configurations, image_diff_analysis_configs, hidden_area_analysis_configs |
| 图像数据表 | 4 | radar_images, image_generation_tasks, system_logs, layers |
| 系统管理表 | 6 | system_configs, disk_storage_configs, radar_param_configs, tilt_motor_configs, **command_records**(新), **algorithm_configs**(新) |
| **总计** | **27** | 包含2个新增表 |

### ProjectId字段统计
| 状态 | 表数 | 说明 |
|------|------|------|
| 已有ProjectId | 19 | 无需修改 |
| 新增ProjectId | 4 | users, AlarmHandleRecords, layers, tilt_motor_configs |
| 新建表含ProjectId | 2 | command_records, algorithm_configs |
| 无需ProjectId | 2 | system_configs, disk_storage_configs（全局配置） |
| **总计** | **27** | |

---

## 🚀 下一步工作

### 1. 后端（可选，根据需要实现）
- [ ] 创建CommandRecordRepository和Service
- [ ] 创建AlgorithmConfigRepository和Service
- [ ] 创建CommandController API接口
- [ ] 创建AlgorithmConfigController API接口

### 2. 前端（必需）
- [x] 确保所有工具栏操作使用`store.projectInfo.projectSelected`
- [ ] 更新所有API调用，传递当前项目ID
- [ ] 测试多项目切换场景
- [ ] 测试数据隔离是否正确

### 3. 测试
- [ ] 启动后端，验证数据库迁移是否成功
- [ ] 测试新表的CRUD操作
- [ ] 测试项目切换时数据是否正确过滤
- [ ] 测试外键约束是否生效

---

## 💡 重要提示

1. **数据迁移是幂等的**：可以多次运行，不会重复添加字段或表
2. **外键约束**：删除项目时，关联数据会根据配置：
   - `Restrict`: 阻止删除（如果有关联数据）
   - `Cascade`: 级联删除
   - `SetNull`: 设为NULL
3. **项目隔离**：所有查询都应该增加`WHERE project_id = ?`条件
4. **默认项目**：管理员用户的ProjectId可以为NULL，表示可以访问所有项目

---

## 📝 数据库Schema更新总结

### 新增字段清单
| 表名 | 字段名 | 类型 | 默认值 | 说明 |
|------|--------|------|--------|------|
| Devices | Longitude | REAL | 0 | 经度 |
| Devices | Latitude | REAL | 0 | 纬度 |
| Devices | Elevation | REAL | 0 | 高程 |
| Devices | FactoryId | TEXT | '' | 出厂ID |
| Devices | Orientation | REAL | 0 | 零点朝向 |
| users | ProjectId | TEXT | NULL | 所属项目 |
| AlarmHandleRecords | ProjectId | TEXT | '' | 所属项目 |
| layers | project_id | TEXT | NULL | 所属项目 |
| tilt_motor_configs | project_id | TEXT | '' | 所属项目 |

### 新增表清单
1. **command_records** - 18个字段
2. **algorithm_configs** - 17个字段

---

## 🔧 编译和运行

### 编译后端
```bash
cd RadarSystem.WebAPI
dotnet build --configuration Release
```

### 运行后端
```bash
dotnet run --configuration Release
```

### 检查数据库迁移日志
启动后端时，查看日志输出，应该看到：
```
[INF] 数据库初始化完成
[INF] 已添加字段: Devices.Longitude
[INF] 已添加字段: users.ProjectId
...
[INF] 已创建表: command_records
[INF] 已创建表: algorithm_configs
[INF] 数据库迁移完成
```

---

## ✅ 升级完成检查清单

- [x] 创建CommandRecordEntity和AlgorithmConfigEntity
- [x] 为users、AlarmHandleRecords、layers、tilt_motor_configs添加ProjectId
- [x] 更新RadarDbContext添加新表和外键
- [x] 更新Program.cs添加数据库迁移逻辑
- [x] 生成升级说明文档
- [ ] 编译测试后端
- [ ] 更新前端API调用
- [ ] 功能测试

---

**文档生成时间**: 2025-10-24  
**版本**: v1.0

