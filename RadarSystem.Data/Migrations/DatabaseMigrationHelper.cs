using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RadarSystem.Data.Context;
using System;
using System.Threading.Tasks;

namespace RadarSystem.Data.Migrations
{
    /// <summary>
    /// 数据库迁移帮助类
    /// </summary>
    public class DatabaseMigrationHelper
    {
        private readonly RadarDbContext _context;
        private readonly ILogger<DatabaseMigrationHelper> _logger;

        public DatabaseMigrationHelper(RadarDbContext context, ILogger<DatabaseMigrationHelper> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 确保数据库已创建并更新到最新版本
        /// </summary>
        public async Task<bool> EnsureDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("开始检查数据库...");

                // 确保数据库存在
                bool created = await _context.Database.EnsureCreatedAsync();
                
                if (created)
                {
                    _logger.LogInformation("✅ 数据库已创建");
                }
                else
                {
                    _logger.LogInformation("✅ 数据库已存在");
                }

                // 检查新表是否存在
                await CheckAndCreateNewTablesAsync();

                _logger.LogInformation("✅ 数据库检查完成");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 数据库初始化失败");
                return false;
            }
        }

        /// <summary>
        /// 检查并创建新表
        /// </summary>
        private async Task CheckAndCreateNewTablesAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                // 检查新表
                var tables = new[]
                {
                    "geo_marks",
                    "alarm_rules",
                    "color_settings",
                    "panel_configs",
                    "image_marks",
                    "image_analysis_configs"
                };

                foreach (var tableName in tables)
                {
                    var checkTableSql = $@"
                        SELECT name FROM sqlite_master 
                        WHERE type='table' AND name='{tableName}'";

                    using var command = connection.CreateCommand();
                    command.CommandText = checkTableSql;
                    var result = await command.ExecuteScalarAsync();

                    if (result == null)
                    {
                        _logger.LogInformation($"创建新表: {tableName}");
                        await CreateTableAsync(tableName);
                    }
                    else
                    {
                        _logger.LogInformation($"表已存在: {tableName}");
                    }
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查新表时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 创建表
        /// </summary>
        private async Task CreateTableAsync(string tableName)
        {
            string createSql = tableName switch
            {
                "geo_marks" => @"
                    CREATE TABLE geo_marks (
                        id TEXT PRIMARY KEY NOT NULL,
                        project_id TEXT NOT NULL,
                        name TEXT NOT NULL,
                        type TEXT,
                        coordinates_json TEXT,
                        description TEXT,
                        color TEXT,
                        icon TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        is_deleted INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (project_id) REFERENCES Projects(Id)
                    );
                    CREATE INDEX idx_geo_marks_project ON geo_marks(project_id);
                    CREATE INDEX idx_geo_marks_project_deleted ON geo_marks(project_id, is_deleted);
                    CREATE INDEX idx_geo_marks_name ON geo_marks(name);",

                "alarm_rules" => @"
                    CREATE TABLE alarm_rules (
                        id TEXT PRIMARY KEY NOT NULL,
                        project_id TEXT NOT NULL,
                        rule_name TEXT NOT NULL,
                        rule_description TEXT,
                        alarm_content TEXT,
                        alarm_rule TEXT DEFAULT '>',
                        alarm_level INTEGER DEFAULT 1,
                        enable INTEGER NOT NULL DEFAULT 1,
                        alarm_threshold REAL,
                        devices_json TEXT,
                        geo_mark_array_json TEXT,
                        data_source TEXT,
                        target_type TEXT,
                        mode TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        is_deleted INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (project_id) REFERENCES Projects(Id)
                    );
                    CREATE INDEX idx_alarm_rules_project ON alarm_rules(project_id);
                    CREATE INDEX idx_alarm_rules_project_enable ON alarm_rules(project_id, enable, is_deleted);
                    CREATE INDEX idx_alarm_rules_name ON alarm_rules(rule_name);",

                "color_settings" => @"
                    CREATE TABLE color_settings (
                        id TEXT PRIMARY KEY NOT NULL,
                        project_id TEXT NOT NULL,
                        setting_type TEXT NOT NULL,
                        type INTEGER,
                        min_value REAL,
                        max_value REAL,
                        hsl_h_start INTEGER,
                        hsl_h_end INTEGER,
                        hsl_direction INTEGER,
                        filter_enable INTEGER,
                        filter_min REAL,
                        filter_max REAL,
                        filter_alpha REAL,
                        hsl_s REAL DEFAULT 1.0,
                        hsl_l REAL DEFAULT 0.5,
                        value_array_json TEXT,
                        color_array_json TEXT,
                        auto_mode INTEGER DEFAULT 0,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(Id)
                    );
                    CREATE INDEX idx_color_settings_project ON color_settings(project_id);
                    CREATE UNIQUE INDEX idx_color_settings_project_type ON color_settings(project_id, setting_type);",

                "panel_configs" => @"
                    CREATE TABLE panel_configs (
                        id TEXT PRIMARY KEY NOT NULL,
                        project_id TEXT NOT NULL,
                        panel_type TEXT NOT NULL,
                        config_json TEXT NOT NULL DEFAULT '{}',
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(Id)
                    );
                    CREATE INDEX idx_panel_configs_project ON panel_configs(project_id);
                    CREATE UNIQUE INDEX idx_panel_configs_project_type ON panel_configs(project_id, panel_type);",

                "image_marks" => @"
                    CREATE TABLE image_marks (
                        id TEXT PRIMARY KEY NOT NULL,
                        project_id TEXT NOT NULL,
                        image_id TEXT,
                        name TEXT NOT NULL,
                        mark_type TEXT,
                        coordinates_json TEXT,
                        description TEXT,
                        color TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        is_deleted INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (project_id) REFERENCES Projects(Id)
                    );
                    CREATE INDEX idx_image_marks_project ON image_marks(project_id);
                    CREATE INDEX idx_image_marks_project_deleted ON image_marks(project_id, is_deleted);
                    CREATE INDEX idx_image_marks_image ON image_marks(image_id);",

                "image_analysis_configs" => @"
                    CREATE TABLE image_analysis_configs (
                        id TEXT PRIMARY KEY NOT NULL,
                        project_id TEXT NOT NULL,
                        standard_image_side_pixel INTEGER DEFAULT 16384,
                        compress_image_side_pixel INTEGER DEFAULT 1024,
                        matrix_tile_rng_num INTEGER DEFAULT 1203,
                        matrix_tile_ang_num INTEGER DEFAULT 61,
                        gen_defo INTEGER DEFAULT 0,
                        gen_scat INTEGER DEFAULT 1,
                        gen_speed INTEGER DEFAULT 0,
                        gen_acceleration INTEGER DEFAULT 0,
                        config_json TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(Id)
                    );
                    CREATE UNIQUE INDEX idx_image_analysis_configs_project ON image_analysis_configs(project_id);",

                _ => throw new ArgumentException($"Unknown table: {tableName}")
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(createSql);
                _logger.LogInformation($"✅ 表 {tableName} 创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 创建表 {tableName} 失败");
                throw;
            }
        }

        /// <summary>
        /// 获取数据库信息
        /// </summary>
        public async Task<string> GetDatabaseInfoAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                var sql = @"
                    SELECT name FROM sqlite_master 
                    WHERE type='table' 
                    ORDER BY name";

                using var command = connection.CreateCommand();
                command.CommandText = sql;

                var tables = new System.Collections.Generic.List<string>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }

                await connection.CloseAsync();

                return $"数据库包含 {tables.Count} 个表:\n" + string.Join(", ", tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据库信息失败");
                return "获取数据库信息失败";
            }
        }
    }
}

