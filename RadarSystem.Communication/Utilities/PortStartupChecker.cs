using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Utilities
{
    /// <summary>
    /// DotNetty端口启动检查器
    /// 验证指定端口是否成功启动监听
    /// </summary>
    public class PortStartupChecker
    {
        private readonly ILogger<PortStartupChecker> _logger;
        private readonly int _checkIntervalSeconds;
        private readonly int _maxRetries;
        private readonly int _timeoutSeconds;

        public PortStartupChecker(
            ILogger<PortStartupChecker> logger,
            int checkIntervalSeconds = 2,
            int maxRetries = 15,
            int timeoutSeconds = 30)
        {
            _logger = logger;
            _checkIntervalSeconds = checkIntervalSeconds;
            _maxRetries = maxRetries;
            _timeoutSeconds = timeoutSeconds;
        }

        /// <summary>
        /// 检查端口是否在监听
        /// </summary>
        public bool IsPortListening(int port)
        {
            try
            {
                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

                return tcpConnInfoArray.Any(endpoint => endpoint.Port == port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查端口 {Port} 时发生错误", port);
                return false;
            }
        }

        /// <summary>
        /// 等待端口开始监听（异步）
        /// </summary>
        public async Task<bool> WaitForPortListeningAsync(int port, string deviceName)
        {
            _logger.LogInformation("开始检查端口 {Port} ({DeviceName}) 的监听状态...", port, deviceName);
            Console.WriteLine($"⏳ 正在检查端口 {port} ({deviceName}) ...");

            int retries = 0;
            DateTime startTime = DateTime.Now;

            while (retries < _maxRetries)
            {
                if ((DateTime.Now - startTime).TotalSeconds > _timeoutSeconds)
                {
                    _logger.LogWarning("检查端口 {Port} ({DeviceName}) 超时", port, deviceName);
                    Console.WriteLine($"⚠️  端口 {port} ({deviceName}) 检查超时");
                    return false;
                }

                if (IsPortListening(port))
                {
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    _logger.LogInformation("✅ 端口 {Port} ({DeviceName}) 已成功监听，耗时 {Elapsed:F1}秒", 
                        port, deviceName, elapsed);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ 端口 {port} ({deviceName}) 监听成功！耗时 {elapsed:F1}秒");
                    Console.ResetColor();
                    
                    return true;
                }

                retries++;
                await Task.Delay(_checkIntervalSeconds * 1000);
                
                if (retries % 5 == 0)
                {
                    Console.WriteLine($"   检查中... ({retries}/{_maxRetries})");
                }
            }

            _logger.LogWarning("端口 {Port} ({DeviceName}) 在 {Retries} 次检查后仍未监听", 
                port, deviceName, retries);
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ 端口 {port} ({deviceName}) 启动失败！");
            Console.ResetColor();
            
            return false;
        }

        /// <summary>
        /// 批量检查多个端口
        /// </summary>
        public async Task<Dictionary<int, bool>> CheckMultiplePortsAsync(Dictionary<int, string> ports)
        {
            var results = new Dictionary<int, bool>();

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  DotNetty端口启动检查                                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            foreach (var port in ports.OrderBy(p => p.Key))
            {
                var isListening = await WaitForPortListeningAsync(port.Key, port.Value);
                results[port.Key] = isListening;
                await Task.Delay(500); // 短暂延迟
            }

            // 显示汇总
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  端口检查汇总                                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

            int successCount = results.Count(r => r.Value);
            int totalCount = results.Count;

            foreach (var result in results.OrderBy(r => r.Key))
            {
                var status = result.Value ? "✅ 监听中" : "❌ 未监听";
                var color = result.Value ? ConsoleColor.Green : ConsoleColor.Red;
                
                Console.ForegroundColor = color;
                Console.WriteLine($"  端口 {result.Key,5}: {status} - {ports[result.Key]}");
                Console.ResetColor();
            }

            Console.WriteLine($"\n  成功: {successCount}/{totalCount}");
            
            if (successCount == totalCount)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✅ 所有端口启动成功！");
                Console.ResetColor();
            }
            else if (successCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠️  部分端口启动成功 ({successCount}/{totalCount})");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ❌ 所有端口启动失败！");
                Console.ResetColor();
            }

            Console.WriteLine("════════════════════════════════════════════════════════════════\n");

            return results;
        }

        /// <summary>
        /// 检查重点端口（从配置读取）
        /// </summary>
        public async Task<Dictionary<int, bool>> CheckCriticalPortsAsync()
        {
            var criticalPorts = new Dictionary<int, string>
            {
                { 1030, "圆弧雷达 ArcRadar" },
                { 10305, "MIMO Lite雷达" },
                { 1060, "建筑物雷达" },
                { 11135, "建筑物2D雷达" },
                { 11125, "MIMO雷达" },
                { 11129, "MIMO通用" },
                { 11133, "交通雷达" },
                { 11127, "俯仰电机" },
                { 11114, "电机" },
                { 11111, "GPS设备" }
            };

            return await CheckMultiplePortsAsync(criticalPorts);
        }

        /// <summary>
        /// 显示所有监听端口
        /// </summary>
        public void ShowAllListeningPorts()
        {
            try
            {
                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();

                Console.WriteLine("\n【当前所有监听端口】");
                foreach (var listener in tcpListeners.OrderBy(l => l.Port))
                {
                    Console.WriteLine($"  TCP  {listener.Address}:{listener.Port}  LISTENING");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取监听端口列表失败");
            }
        }
    }
}

