using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Services;

// 简单的测试程序来验证Netty服务启动
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var loggerFactory = LoggerFactory.Create(builder => 
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<ArcRadarNettyServer>();
var mqttLogger = loggerFactory.CreateLogger<MqttService>();

// 读取配置
var arcRadarSection = configuration.GetSection("Netty:ArcRadar");
var enable = arcRadarSection.GetValue<bool>("Enable");
var port = arcRadarSection.GetValue<int>("Port");

Console.WriteLine("================================================================================");
Console.WriteLine($"配置检查:");
Console.WriteLine($"  Enable: {enable}");
Console.WriteLine($"  Port: {port}");
Console.WriteLine($"  ProjectId: {arcRadarSection.GetValue<string>("ProjectId")}");
Console.WriteLine($"  DataPath: {arcRadarSection.GetValue<string>("DataPath")}");
Console.WriteLine($"  ApiPort: {arcRadarSection.GetValue<string>("ApiPort")}");
Console.WriteLine("================================================================================");

if (!enable)
{
    Console.WriteLine("❌ 圆弧雷达未启用！");
    return;
}

// 创建MQTT配置
var mqttConfig = new MqttConfiguration
{
    BrokerHost = "localhost",
    BrokerPort = 1883,
    ClientId = "TestClient"
};

var mqttService = new MqttService(mqttLogger, mqttConfig);

// 尝试启动MQTT
Console.WriteLine("\n正在启动MQTT...");
try
{
    await mqttService.ConnectAsync();
    Console.WriteLine("✅ MQTT连接成功");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ MQTT连接失败: {ex.Message}");
    Console.WriteLine("继续启动Netty服务器...");
}

// 创建ArcRadar配置
var arcConfig = new ArcRadarConfiguration
{
    Port = port,
    Enable = enable,
    ProjectId = arcRadarSection.GetValue<string>("ProjectId") ?? "PROJECT001",
    DataPath = arcRadarSection.GetValue<string>("DataPath") ?? "../../",
    ApiPort = arcRadarSection.GetValue<string>("ApiPort") ?? "8099"
};

Console.WriteLine("\n正在创建圆弧雷达服务器...");
var arcServer = new ArcRadarNettyServer(logger, arcConfig, mqttService);

Console.WriteLine("正在启动圆弧雷达服务器...");
await arcServer.StartAsync();

Console.WriteLine("\n✅ 圆弧雷达服务器启动成功！");
Console.WriteLine($"监听端口: {port}");

Console.WriteLine("\n按任意键停止服务器...");
Console.ReadKey();

await arcServer.StopAsync();

