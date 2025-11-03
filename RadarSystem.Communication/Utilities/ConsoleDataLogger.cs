using System;

namespace RadarSystem.Communication.Utilities
{
    /// <summary>
    /// 控制台数据记录器 - 统一的数据接收和保存输出格式
    /// </summary>
    public static class ConsoleDataLogger
    {
        /// <summary>
        /// 记录数据接收
        /// </summary>
        public static void LogDataReceived(
            string deviceType,
            int port,
            string factoryId,
            string command,
            int dataLength,
            string hexData,
            string? deviceId = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            Console.WriteLine("================================================================================");
            Console.WriteLine($"【{deviceType}数据接收】");
            Console.WriteLine($"  时间: {timestamp}");
            Console.WriteLine($"  端口: {port}");
            Console.WriteLine($"  唯一值(SlaveId/SlaveId): {factoryId}");
            Console.WriteLine($"  命令代码: {command}");
            Console.WriteLine($"  数据长度: {dataLength:N0} 字节 ({dataLength / 1024.0:F2} KB)");
            
            // 显示前200个字符的十六进制数据
            int displayLength = Math.Min(200, hexData.Length);
            Console.WriteLine($"  原始数据(HEX): {hexData.Substring(0, displayLength)}{(hexData.Length > 200 ? "..." : "")}");
            
            // 如果数据较小，显示完整数据
            if (dataLength <= 1000 && hexData.Length <= 2000)
            {
                Console.WriteLine($"  完整数据: {hexData}");
            }
            
            if (!string.IsNullOrEmpty(deviceId))
            {
                Console.WriteLine($"  设备映射: SlaveId({factoryId}) → DeviceId({deviceId})");
            }
            
            Console.WriteLine("================================================================================");
        }

        /// <summary>
        /// 记录文件保存
        /// </summary>
        public static void LogFileSaved(
            string deviceType,
            string deviceId,
            string factoryId,
            string dataType,
            string typeName,
            string filePath,
            long fileSize,
            bool success,
            string? errorMessage = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            Console.WriteLine("********************************************************************************");
            Console.WriteLine($"【{deviceType}文件保存】");
            Console.WriteLine($"  时间: {timestamp}");
            Console.WriteLine($"  设备ID: {deviceId}");
            Console.WriteLine($"  SlaveId: {factoryId}");
            Console.WriteLine($"  数据类型: {typeName} ({dataType})");
            Console.WriteLine($"  文件路径: {filePath}");
            Console.WriteLine($"  文件大小: {fileSize / 1024.0:F2} KB");
            
            if (success)
            {
                Console.WriteLine($"  状态: ✅ 保存成功");
            }
            else
            {
                Console.WriteLine($"  状态: ❌ 保存失败");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine($"  错误: {errorMessage}");
                }
            }
            
            Console.WriteLine("********************************************************************************");
        }

        /// <summary>
        /// 记录服务器启动
        /// </summary>
        public static void LogServerStarted(
            string deviceType,
            int port,
            string projectId,
            string dataPath,
            bool success,
            string? errorMessage = null)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            
            if (success)
            {
                Console.WriteLine($"║ ✅ {deviceType} 服务器启动成功！");
                Console.WriteLine($"║   监听端口: {port}");
                Console.WriteLine($"║   项目ID: {projectId}");
                Console.WriteLine($"║   数据路径: {dataPath}");
                Console.WriteLine($"║   等待设备连接...");
            }
            else
            {
                Console.WriteLine($"║ ❌ {deviceType} 服务器启动失败！");
                Console.WriteLine($"║   端口: {port}");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine($"║   错误: {errorMessage}");
                }
            }
            
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// 记录设备映射加载
        /// </summary>
        public static void LogDeviceMappingLoaded(
            string deviceType,
            int deviceCount,
            Dictionary<string, string>? mappings = null)
        {
            Console.WriteLine("────────────────────────────────────────────────────────────────────────────────");
            Console.WriteLine($"【{deviceType}设备映射】");
            Console.WriteLine($"  加载时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  设备数量: {deviceCount}");
            
            if (mappings != null && mappings.Count > 0)
            {
                Console.WriteLine($"  映射列表:");
                foreach (var mapping in mappings.Take(10))  // 只显示前10个
                {
                    Console.WriteLine($"    SlaveId({mapping.Key}) → DeviceId({mapping.Value})");
                }
                if (mappings.Count > 10)
                {
                    Console.WriteLine($"    ... 还有 {mappings.Count - 10} 个设备");
                }
            }
            
            Console.WriteLine("────────────────────────────────────────────────────────────────────────────────");
        }

        /// <summary>
        /// 记录心跳包
        /// </summary>
        public static void LogHeartbeat(
            string deviceType,
            string deviceId,
            string factoryId,
            string? ipAddress = null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 💓 {deviceType} 心跳 - SlaveId:{factoryId} DeviceId:{deviceId}{(ipAddress != null ? $" IP:{ipAddress}" : "")}");
            Console.ResetColor();
        }
    }
}

