using System;

namespace RadarSystem.Data.TDengine
{
    /// <summary>
    /// TDengine 配置
    /// </summary>
    public class TDengineConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6030;
        public string Database { get; set; } = "radar_db";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "taosdata";
        public int ConnectionPoolSize { get; set; } = 10;
        public bool AutoCreateDatabase { get; set; } = true;
        public bool AutoCreateTables { get; set; } = true;
        public int DataRetentionDays { get; set; } = 90; // 数据保留天数

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public string GetConnectionString()
        {
            return $"host={Host};port={Port};username={Username};password={Password};database={Database}";
        }

        /// <summary>
        /// 获取不带数据库的连接字符串（用于创建数据库）
        /// </summary>
        public string GetConnectionStringWithoutDb()
        {
            return $"host={Host};port={Port};username={Username};password={Password}";
        }
    }
}

