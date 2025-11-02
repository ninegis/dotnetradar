-- 添加项目初始化场景字段
ALTER TABLE Projects ADD COLUMN SceneLongitude REAL DEFAULT 120.0;
ALTER TABLE Projects ADD COLUMN SceneLatitude REAL DEFAULT 30.0;
ALTER TABLE Projects ADD COLUMN SceneHeight REAL DEFAULT 500.0;
ALTER TABLE Projects ADD COLUMN SceneHeading REAL DEFAULT 0.0;
ALTER TABLE Projects ADD COLUMN ScenePitch REAL DEFAULT -45.0;
ALTER TABLE Projects ADD COLUMN SceneRoll REAL DEFAULT 0.0;

-- 更新现有项目的场景数据（示例）
UPDATE Projects 
SET SceneLongitude = 120.123456,
    SceneLatitude = 30.123456,
    SceneHeight = 1000.0,
    SceneHeading = 0.0,
    ScenePitch = -60.0,
    SceneRoll = 0.0
WHERE ProjectId = 'PROJECT001';

