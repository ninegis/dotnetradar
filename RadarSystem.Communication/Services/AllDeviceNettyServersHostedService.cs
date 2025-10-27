using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 设备Netty服务器后台服务
    /// 负责启动所有配置启用的设备服务器
    /// </summary>
    public class AllDeviceNettyServersHostedService : IHostedService
    {
        private readonly ILogger<AllDeviceNettyServersHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly MqttService _mqttService;
        private readonly List<DeviceNettyServerBase> _servers = new();

        public AllDeviceNettyServersHostedService(
            ILogger<AllDeviceNettyServersHostedService> logger,
            IConfiguration configuration,
            MqttService mqttService)
        {
            _logger = logger;
            _configuration = configuration;
            _mqttService = mqttService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("====================================");
            _logger.LogInformation("  边坡雷达设备通信系统启动中...");
            _logger.LogInformation("====================================");

            try
            {
                // 初始化MQTT服务
                _logger.LogInformation("正在启动MQTT服务...");
                await _mqttService.ConnectAsync();
                _logger.LogInformation("✅ MQTT服务启动成功");

                // 启动所有配置启用的设备服务器
                await StartDeviceServersAsync(cancellationToken);

                _logger.LogInformation("====================================");
                _logger.LogInformation("  设备通信系统启动完成");
                _logger.LogInformation("  活跃服务器数量: {Count}", _servers.Count);
                _logger.LogInformation("====================================");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设备通信系统启动失败");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止设备通信系统...");

            // 停止所有设备服务器
            var stopTasks = _servers.Select(server => Task.Run(async () =>
            {
                try
                {
                    await server.StopAsync();
                    _logger.LogInformation("已停止设备服务器: {DeviceType}", server.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "停止设备服务器失败: {DeviceType}", server.GetType().Name);
                }
            }, cancellationToken));

            await Task.WhenAll(stopTasks);

            // 停止MQTT服务
            try
            {
                await _mqttService.DisconnectAsync();
                _logger.LogInformation("✅ MQTT服务已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止MQTT服务失败");
            }

            _logger.LogInformation("设备通信系统已完全停止");
        }

        private async Task StartDeviceServersAsync(CancellationToken cancellationToken)
        {
            var nettySection = _configuration.GetSection("Netty");
            if (nettySection == null)
            {
                _logger.LogWarning("未找到Netty配置节");
                return;
            }

            // 启动MIMO系列雷达
            await TryStartServer<MimoRadarNettyServer>("MimoRadar", cancellationToken);
            await TryStartServer<MimoLiteRadarNettyServer>("MimoLiteRadar", cancellationToken);
            await TryStartServer<MimoNettyServer>("Mimo", cancellationToken);

            // 启动建筑物雷达
            await TryStartServer<BuildingRadarNettyServer>("BuildingRadar", cancellationToken);
            await TryStartServer<Building2DRadarNettyServer>("Building2DRadar", cancellationToken);

            // 启动传感器设备
            await TryStartServer<GpsNettyServer>("Gps", cancellationToken);
            await TryStartServer<GpsV1NettyServer>("GpsV1", cancellationToken);
            await TryStartServer<QxzNettyServer>("Qxz", cancellationToken);
            await TryStartServer<LaserNettyServer>("Laser", cancellationToken);

            // 启动控制设备
            await TryStartServer<MotorNettyServer>("Motor", cancellationToken);
            await TryStartServer<BMotorNettyServer>("BMotor", cancellationToken);
            await TryStartServer<MotorPitchNettyServer>("MotorPitch", cancellationToken);

            // 启动告警设备
            await TryStartServer<AlarmNettyServer>("Alarm", cancellationToken);
            await TryStartServer<AlarmDeviceNettyServer>("AlarmDevice", cancellationToken);

            // 启动其他设备
            await TryStartServer<BwNettyServer>("Bw", cancellationToken);
            await TryStartServer<BwV1NettyServer>("BwV1", cancellationToken);
            await TryStartServer<CmNettyServer>("Cm", cancellationToken);
            await TryStartServer<OrientationNettyServer>("Orientation", cancellationToken);

            _logger.LogInformation("注意: ArcRadar圆弧雷达服务器需单独配置启动（不同的类层次结构）");
        }

        private async Task TryStartServer<T>(string configKey, CancellationToken cancellationToken) where T : DeviceNettyServerBase
        {
            try
            {
                var config = _configuration.GetSection($"Netty:{configKey}");
                var enabled = config.GetValue<bool>("Enable", false);

                if (!enabled)
                {
                    _logger.LogDebug("设备服务器 {ConfigKey} 未启用，跳过", configKey);
                    return;
                }

                var port = config.GetValue<int>("Port", 0);
                if (port == 0)
                {
                    _logger.LogWarning("设备服务器 {ConfigKey} 端口配置无效", configKey);
                    return;
                }

                var deviceConfig = new DeviceNettyConfiguration
                {
                    Port = port,
                    ProjectId = config.GetValue<string>("ProjectId") ?? "PROJECT001",
                    DataPath = config.GetValue<string>("DataPath") ?? "../../",
                    ApiPort = config.GetValue<string>("ApiPort") ?? "80"
                };

                // 创建服务器实例
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var serverLogger = loggerFactory.CreateLogger<T>();
                var server = (T)Activator.CreateInstance(typeof(T), serverLogger, deviceConfig, _mqttService)!;

                // 启动服务器
                await server.StartAsync();
                
                _servers.Add(server);
                _logger.LogInformation("✅ 设备服务器启动成功: {ConfigKey} - 端口: {Port}", configKey, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动设备服务器失败: {ConfigKey}", configKey);
            }
        }
    }
}

