-- 为设备表添加新字段
-- 添加独立的地理坐标字段和雷达特有信息字段

-- 添加经度字段
ALTER TABLE Devices ADD COLUMN Longitude REAL DEFAULT 0;

-- 添加纬度字段
ALTER TABLE Devices ADD COLUMN Latitude REAL DEFAULT 0;

-- 添加高度字段
ALTER TABLE Devices ADD COLUMN Elevation REAL DEFAULT 0;

-- 添加出厂ID字段
ALTER TABLE Devices ADD COLUMN FactoryId TEXT DEFAULT '';

-- 添加零点朝向字段
ALTER TABLE Devices ADD COLUMN Orientation REAL DEFAULT 0;

-- 检查表结构
PRAGMA table_info(Devices);

