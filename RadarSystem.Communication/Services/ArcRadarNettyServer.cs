using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RadarSystem.Core.Models;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// 圆弧雷达 Netty 服务器（对应 Java DAG 模块）
    /// 默认端口：1030
    /// </summary>
    public class ArcRadarNettyServer : IDisposable
    {
        private readonly ILogger<ArcRadarNettyServer> _logger;
        private readonly ArcRadarConfiguration _config;
        private readonly MqttService _mqttService;
        private IEventLoopGroup? _bossGroup;
        private IEventLoopGroup? _workerGroup;
        private IChannel? _boundChannel;
        private bool _isRunning = false;

        // 设备通道映射
        private readonly ConcurrentDictionary<string, IChannelHandlerContext> _deviceChannelMap;
        
        // 设备ID映射（slaveId -> deviceId）
        private readonly ConcurrentDictionary<string, string> _deviceIdMap;
        
        // 心跳时间记录
        private readonly ConcurrentDictionary<string, long> _heartbeatTimeMap;
        
        // 雷达图像队列
        private readonly ConcurrentQueue<ArcRadarImage> _imageQueue;

        public event EventHandler<ArcRadarDataReceivedEventArgs>? DataReceived;
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

        public ArcRadarNettyServer(
            ILogger<ArcRadarNettyServer> logger, 
            ArcRadarConfiguration config,
            MqttService mqttService)
        {
            _logger = logger;
            _config = config;
            _mqttService = mqttService;
            _deviceChannelMap = new ConcurrentDictionary<string, IChannelHandlerContext>();
            _deviceIdMap = new ConcurrentDictionary<string, string>();
            _heartbeatTimeMap = new ConcurrentDictionary<string, long>();
            _imageQueue = new ConcurrentQueue<ArcRadarImage>();

            // 启动图像处理线程
            Task.Run(() => ProcessImageQueue());
            
            // ✅ 启动设备映射加载线程
            Task.Run(() => LoadDeviceMappingAsync());
        }

        /// <summary>
        /// 从数据库加载 FactoryId -> DeviceId 映射
        /// </summary>
        private async Task LoadDeviceMappingAsync()
        {
            try
            {
                _logger.LogInformation("正在从API加载设备映射...");
                
                // 调用API获取设备列表
                string apiUrl = $"http://localhost:{_config.ApiPort}/api/Device";
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var devices = JsonConvert.DeserializeObject<List<DeviceMappingDto>>(json);
                    
                    if (devices != null)
                    {
                        foreach (var device in devices)
                        {
                            if (!string.IsNullOrEmpty(device.FactoryId))
                            {
                                // FactoryId (出厂ID) 就是 SlaveId
                                _deviceIdMap.TryAdd(device.FactoryId, device.DeviceId);
                                _logger.LogInformation("加载设备映射: FactoryId={FactoryId} → DeviceId={DeviceId}", 
                                    device.FactoryId, device.DeviceId);
                            }
                        }
                        _logger.LogInformation("设备映射加载完成，共{Count}个设备", devices.Count);
                    }
                }
                else
                {
                    _logger.LogWarning("无法从API获取设备列表，HTTP状态码: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "加载设备映射失败，将使用 SlaveId 作为 DeviceId");
            }
        }

        /// <summary>
        /// 启动圆弧雷达 Netty 服务器
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                _logger.LogWarning("圆弧雷达 Netty 服务器已经在运行");
                return;
            }

            if (!_config.Enable)
            {
                _logger.LogInformation("圆弧雷达 Netty 服务器已禁用");
                return;
            }

            try
            {
                _logger.LogInformation("正在启动圆弧雷达 Netty 服务器，端口: {Port}", _config.Port);

                // 创建事件循环组
                _bossGroup = new MultithreadEventLoopGroup(1);
                _workerGroup = new MultithreadEventLoopGroup();

                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(_bossGroup, _workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 128)
                    .Handler(new LoggingHandler("ARC-RADAR-SRV"))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // 添加日志处理器
                        pipeline.AddLast("logger", new LoggingHandler("ARC-RADAR-CONN"));

                        // ✅ 不使用解码器，直接处理原始字节
                        // pipeline.AddLast("decoder", new ArcRadarDecoder());

                        // 添加业务处理器
                        pipeline.AddLast("handler", new ArcRadarServerHandler(this, _logger));
                    }));

                // 绑定端口并启动服务器
                _boundChannel = await bootstrap.BindAsync(_config.Port);
                _isRunning = true;

                _logger.LogInformation("圆弧雷达 Netty 服务器启动成功，监听端口: {Port}", _config.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动圆弧雷达 Netty 服务器失败");
                throw;
            }
        }

        /// <summary>
        /// 停止圆弧雷达 Netty 服务器
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                _logger.LogInformation("正在停止圆弧雷达 Netty 服务器");

                if (_boundChannel != null)
                {
                    await _boundChannel.CloseAsync();
                }

                if (_workerGroup != null)
                {
                    await _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                }

                if (_bossGroup != null)
                {
                    await _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                }

                _isRunning = false;
                _logger.LogInformation("圆弧雷达 Netty 服务器已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止圆弧雷达 Netty 服务器时发生错误");
            }
        }

        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        internal void HandleData(byte[] data, IChannelHandlerContext context)
        {
            try
            {
                string hexString = BitConverter.ToString(data).Replace("-", "").ToUpper();
                
                if (hexString.Length < 16)
                {
                    _logger.LogWarning("接收到的数据长度不足: {Length}", hexString.Length);
                    return;
                }

                // 解析命令
                string command = hexString.Substring(12, 4);
                
                // ✅ Java参考: ByteUtil.stringToInt(hexString.substring(4, 12))  
                // 使用LITTLE_ENDIAN字节序（与Java一致）
                string slaveIdHex = hexString.Substring(4, 8);
                byte[] slaveIdBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    slaveIdBytes[i] = Convert.ToByte(slaveIdHex.Substring(i * 2, 2), 16);
                }
                int slaveId = BitConverter.ToInt32(slaveIdBytes, 0); // LITTLE_ENDIAN
                string slaveIdStr = slaveId.ToString();

                // 获取设备ID
                string deviceId = GetDeviceId(slaveIdStr);
                if (string.IsNullOrEmpty(deviceId))
                {
                    _logger.LogWarning("未找到 FactoryId/SlaveId {SlaveId} 对应的设备ID，使用FactoryId作为DeviceId", slaveIdStr);
                    deviceId = slaveIdStr; // 使用 FactoryId 作为 DeviceId
                }

                // ✅ 使用统一的控制台输出
                RadarSystem.Communication.Utilities.ConsoleDataLogger.LogDataReceived(
                    "圆弧雷达",
                    _config.Port,
                    slaveIdStr,
                    $"0x{command}",
                    data.Length,
                    hexString,
                    deviceId);

                _logger.LogInformation("接收到圆弧雷达数据 - 端口:{Port}, FactoryId:{FactoryId}, 命令:0x{Command}, 长度:{Length}字节", 
                    _config.Port, slaveIdStr, command, data.Length);

                // 保存设备通道
                _deviceChannelMap.TryAdd(deviceId, context);

                // 获取客户端地址
                var remoteAddress = context.Channel.RemoteAddress as IPEndPoint;

                // 处理不同的命令
                if (hexString.StartsWith("5A5A"))
                {
                    HandleUpstreamCommand(command, slaveIdStr, deviceId, data, context, remoteAddress);
                }
                else if (hexString.StartsWith("3C3C"))
                {
                    HandleDownstreamResponse(command, slaveIdStr, deviceId, data, context, remoteAddress);
                }
                else
                {
                    _logger.LogWarning("收到未知命令头: {Header}", hexString.Substring(0, 4));
                }

                // 触发数据接收事件
                DataReceived?.Invoke(this, new ArcRadarDataReceivedEventArgs(deviceId, slaveIdStr, command, data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理圆弧雷达数据时发生错误");
            }
        }

        /// <summary>
        /// 处理上行命令（雷达发送到服务器）
        /// </summary>
        private void HandleUpstreamCommand(
            string command, 
            string slaveId, 
            string deviceId, 
            byte[] data, 
            IChannelHandlerContext context,
            IPEndPoint? remoteAddress)
        {
            switch (command)
            {
                case "0000": // 心跳命令
                    _logger.LogInformation("圆弧雷达接收到心跳命令，设备: {DeviceId}", deviceId);
                    HandleHeartbeat(slaveId, deviceId, data, context, remoteAddress);
                    break;

                case "1000": // 时间同步命令
                    _logger.LogInformation("圆弧雷达接收到时间同步命令，设备: {DeviceId}", deviceId);
                    HandleTimeSync(slaveId, context);
                    break;

                case "0302": // 形变数据 - Java: 0302
                    _logger.LogInformation("圆弧雷达接收到形变数据上报 - FactoryId:{FactoryId}, DeviceId:{DeviceId}, 数据长度:{Length}字节", 
                        slaveId, deviceId, data.Length);
                    Console.WriteLine($"[DATA] 形变数据: FactoryId={slaveId}, Length={data.Length} bytes");
                    HandleImageData(slaveId, deviceId, "00", "形变", data);
                    break;

                case "0301": // 复散射数据 - Java: 0301
                    _logger.LogInformation("圆弧雷达接收到复散射数据上报 - FactoryId:{FactoryId}, DeviceId:{DeviceId}, 数据长度:{Length}字节", 
                        slaveId, deviceId, data.Length);
                    Console.WriteLine($"[DATA] 复散射数据: FactoryId={slaveId}, Length={data.Length} bytes");
                    HandleImageData(slaveId, deviceId, "01", "复散射", data);
                    break;

                case "0303": // 置信度数据 - Java: 0303
                    _logger.LogInformation("圆弧雷达接收到置信度数据上报 - FactoryId:{FactoryId}, DeviceId:{DeviceId}, 数据长度:{Length}字节", 
                        slaveId, deviceId, data.Length);
                    Console.WriteLine($"[DATA] 置信度数据: FactoryId={slaveId}, Length={data.Length} bytes");
                    HandleImageData(slaveId, deviceId, "02", "置信度", data);
                    break;

                default:
                    _logger.LogWarning("收到未知上行命令: {Command}", command);
                    break;
            }
        }

        /// <summary>
        /// 处理下行响应（服务器命令的响应）
        /// </summary>
        private void HandleDownstreamResponse(
            string command, 
            string slaveId, 
            string deviceId, 
            byte[] data, 
            IChannelHandlerContext context,
            IPEndPoint? remoteAddress)
        {
            string stateHex = BitConverter.ToString(data).Replace("-", "").Substring(16, 2);
            int state = Convert.ToInt32(stateHex, 16);

            _logger.LogInformation("接收到雷达返回信息，命令: {Command}, 状态: {State}", command, state);

            // 根据命令类型处理响应
            switch (command)
            {
                case "0100": // 获取场景参数响应
                case "0103": // 获取电机参数响应
                    HandleSceneParamResponse(deviceId, data, state, remoteAddress);
                    break;

                case "0101": // 获取算法参数响应
                    HandleAlgorithmParamResponse(deviceId, data, state, remoteAddress);
                    break;

                default:
                    SendCommandResponse(command, deviceId, state);
                    break;
            }
        }

        /// <summary>
        /// 处理心跳
        /// </summary>
        private void HandleHeartbeat(
            string slaveId, 
            string deviceId, 
            byte[] data, 
            IChannelHandlerContext context,
            IPEndPoint? remoteAddress)
        {
            // 解析雷达信息
            var radarInfo = ParseRadarInfo(slaveId, deviceId, data);

            // ✅ 发送设备上线状态
            SendDeviceOnlineStatus(deviceId, slaveId, true);

            // 发送心跳到 MQTT
            SendHeartbeatToMqtt(deviceId);

            // 发送设备信息到 MQTT
            SendDeviceInfoToMqtt(radarInfo, remoteAddress);

            // 响应心跳
            SendHeartbeatResponse(slaveId, context);
        }
        
        /// <summary>
        /// 发送设备在线状态（MQTT）
        /// </summary>
        private void SendDeviceOnlineStatus(string deviceId, string factoryId, bool isOnline)
        {
            try
            {
                var statusMessage = new
                {
                    deviceId = deviceId,
                    factoryId = factoryId,
                    status = isOnline ? "online" : "offline",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    type = "ArcRadar"
                };

                string json = JsonConvert.SerializeObject(statusMessage);
                _mqttService.PublishAsync("/dev/device/status", json).Wait();
                
                Console.WriteLine($"[STATUS] Device {deviceId} (FactoryId={factoryId}): {(isOnline ? "ONLINE" : "OFFLINE")}");
                _logger.LogInformation("设备状态更新: {DeviceId} - {Status}", deviceId, isOnline ? "在线" : "离线");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "发送设备状态失败（MQTT不可用）");
            }
        }

        /// <summary>
        /// 处理时间同步（参考Java: timeSync）
        /// </summary>
        private void HandleTimeSync(string slaveIdStr, IChannelHandlerContext context)
        {
            _logger.LogInformation("处理时间同步命令，时间: {Time}", DateTime.Now);

            // Java: getSlaveIdHexString - 转为反向hex
            int slaveIdInt = int.Parse(slaveIdStr);
            string slaveIdHex = GetSlaveIdHexString(slaveIdInt);
            
            // Java: String.valueOf(new Date().getTime())
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string timeStr = timestamp.ToString();
            
            // 转为十六进制字符串（Java: ByteUtil.string2HexString）
            string timeHex = "";
            foreach (char c in timeStr)
            {
                timeHex += ((int)c).ToString("X2");
            }
            
            int timeHexLength = timeHex.Length / 2;
            string lengthHex = timeHexLength.ToString("X").PadRight(8, '0');
            
            // Java: "3C3C" + slaveIdHexString + "100000" + byteLength + timeHexString
            string commandHex = $"3C3C{slaveIdHex}1000{lengthHex}{timeHex}";

            _logger.LogInformation("时间同步响应: {Command}", commandHex);
            Console.WriteLine($"[SEND] TimeSync: {commandHex}");
            SendCommand(context, commandHex);
        }
        
        /// <summary>
        /// 获取SlaveId十六进制字符串（反向填充，参考Java: getSlaveIdHexString）
        /// </summary>
        private string GetSlaveIdHexString(int slaveId)
        {
            // Java: ByteUtil.intToHexString(slaveId, 1)
            string hexString = slaveId.ToString("X");
            
            // Java: ByteUtil.fillReverse(hexString, 8, '0')
            return hexString.PadRight(8, '0');
        }

        /// <summary>
        /// 处理图像数据
        /// </summary>
        private void HandleImageData(string slaveId, string deviceId, string dataType, string typeName, byte[] data)
        {
            try
            {
                string filePath = GetFilePath(dataType, _config.ProjectId, deviceId);
                _logger.LogInformation("{TypeName}数据存储地址: {FilePath}", typeName, filePath);

                var radarImage = new ArcRadarImage
                {
                    SlaveId = slaveId,
                    DeviceId = deviceId,
                    DataType = dataType,
                    TypeName = typeName,
                    FilePath = filePath,
                    Data = data,
                    ReceiveTime = DateTime.Now
                };

                _imageQueue.Enqueue(radarImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理{TypeName}图像数据时发生错误", typeName);
            }
        }

        /// <summary>
        /// 处理场景参数响应
        /// </summary>
        private void HandleSceneParamResponse(string deviceId, byte[] data, int state, IPEndPoint? remoteAddress)
        {
            if (state == 0)
            {
                _logger.LogInformation("返回设备 {DeviceId} 的圆弧雷达参数", deviceId);
                
                // 这里可以解析参数并发送到 MQTT
                // TODO: 实现参数解析逻辑
            }
            else
            {
                _logger.LogError("返回圆弧雷达参数命令错误，状态: {State}", state);
            }
        }

        /// <summary>
        /// 处理算法参数响应
        /// </summary>
        private void HandleAlgorithmParamResponse(string deviceId, byte[] data, int state, IPEndPoint? remoteAddress)
        {
            if (state == 0)
            {
                _logger.LogInformation("返回设备 {DeviceId} 的圆弧雷达算法参数", deviceId);
                
                // 这里可以解析算法参数并发送到 MQTT
                // TODO: 实现算法参数解析逻辑
            }
            else
            {
                _logger.LogError("返回圆弧雷达算法参数命令错误，状态: {State}", state);
            }
        }

        /// <summary>
        /// 发送命令响应到 MQTT
        /// </summary>
        private void SendCommandResponse(string command, string deviceId, int state)
        {
            string action = GetActionFromCommand(command);
            
            if (action != "-1")
            {
                var response = new
                {
                    time = DateTime.Now.ToString("yyyyMMddHHmmss.fff"),
                    deviceId = deviceId,
                    action = action,
                    result = state.ToString()
                };

                string json = JsonConvert.SerializeObject(response);
                _mqttService.PublishAsync("/dev/radar/defo/command/response", json).Wait();
            }
        }

        /// <summary>
        /// 发送心跳到 MQTT（参考Java: sendHeartbeat）
        /// </summary>
        private void SendHeartbeatToMqtt(string deviceId)
        {
            try
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                
                if (_heartbeatTimeMap.TryGetValue(deviceId, out long lastTime))
                {
                    if ((currentTime - lastTime) / 1000 < 30)
                    {
                        return; // 30秒内不重复发送
                    }
                }

                var heartbeat = new
                {
                    deviceId = deviceId,
                    heartBeatClock = "60",
                    time = DateTime.Now.ToString("yyyyMMddHHmmss")
                };

                string json = JsonConvert.SerializeObject(heartbeat);
                _mqttService.PublishAsync("/dev/heartbeat", json).Wait();
                
                Console.WriteLine($"[MQTT] Heartbeat sent: {deviceId}");
                _heartbeatTimeMap[deviceId] = currentTime;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "发送MQTT心跳失败（MQTT不可用）");
            }
        }

        /// <summary>
        /// 发送设备信息到 MQTT
        /// </summary>
        private void SendDeviceInfoToMqtt(ArcRadarInfo radarInfo, IPEndPoint? remoteAddress)
        {
            var deviceInfo = new
            {
                time = DateTime.Now.ToString("yyyyMMddHHmmss.fff"),
                deviceId = radarInfo.DeviceId,
                slaveId = radarInfo.SlaveId,
                workModel = radarInfo.WorkModel,
                ipv4 = remoteAddress?.Address.ToString() ?? "",
                port = remoteAddress?.Port.ToString() ?? "",
                router = (string?)null,
                algorithmVersion = radarInfo.AlgorithmVersion,
                temperature = radarInfo.Temperature,
                communicationStatus = radarInfo.CommunicationStatus,
                fpgaVersion = radarInfo.FpgaVersion,
                gpsLockStatus = radarInfo.GpsLockStatus,
                paraSetStatus = radarInfo.ParaSetStatus,
                powerStatus = radarInfo.PowerStatus,
                processorStatus = radarInfo.ProcessorStatus,
                productDate = radarInfo.ProductDate,
                radarVersion = radarInfo.RadarVersion,
                rfStatus = radarInfo.RfStatus,
                selfCheckStatus = radarInfo.SelfCheckStatus,
                laserStatus = radarInfo.LaserStatus,
                workState = radarInfo.WorkState,
                lowPowerState = radarInfo.LowPowerState
            };

            string json = JsonConvert.SerializeObject(deviceInfo);
            _mqttService.PublishAsync("/dev/radar/defo/info", json).Wait();
        }

        /// <summary>
        /// 发送心跳响应
        /// </summary>
        /// <summary>
        /// 发送心跳响应（参考Java: heartBeatResponse）
        /// </summary>
        private void SendHeartbeatResponse(string slaveIdStr, IChannelHandlerContext context)
        {
            int slaveIdInt = int.Parse(slaveIdStr);
            string slaveIdHex = GetSlaveIdHexString(slaveIdInt);
            
            // Java: "3C3C" + slaveIdHexString + "00000000000000"
            string commandHex = $"3C3C{slaveIdHex}00000000000000";
            
            _logger.LogInformation("发送心跳响应: {Command}", commandHex);
            Console.WriteLine($"[SEND] Heartbeat ACK to FactoryId={slaveIdStr}");
            SendCommand(context, commandHex);
        }

        /// <summary>
        /// 发送命令到雷达
        /// </summary>
        private void SendCommand(IChannelHandlerContext context, string hexCommand)
        {
            try
            {
                _logger.LogDebug("向雷达发送命令: {Command}", hexCommand);
                
                byte[] commandBytes = HexStringToBytes(hexCommand);
                IByteBuffer buffer = Unpooled.WrappedBuffer(commandBytes);
                context.WriteAndFlushAsync(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送命令失败");
            }
        }

        /// <summary>
        /// 处理图像队列
        /// </summary>
        private async Task ProcessImageQueue()
        {
            while (true)
            {
                try
                {
                    if (_imageQueue.TryDequeue(out var radarImage))
                    {
                        await SaveRadarImage(radarImage);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理图像队列时发生错误");
                }
            }
        }

        /// <summary>
        /// 保存雷达图像
        /// </summary>
        private async Task SaveRadarImage(ArcRadarImage radarImage)
        {
            string fullPath = string.Empty;
            
            try
            {
                // radarImage.FilePath已经是完整目录路径
                string directory = radarImage.FilePath;
                
                // 确保目录存在
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("创建目录: {Directory}", directory);
                }

                // 生成文件名
                string fileName = $"{radarImage.DataType}_{DateTime.Now:yyyyMMddHHmmss}.dat";
                fullPath = Path.Combine(directory, fileName);

                _logger.LogInformation("准备保存文件: {FilePath}", fullPath);
                
                // 保存文件
                await File.WriteAllBytesAsync(fullPath, radarImage.Data);

                // ✅ 使用统一的控制台输出
                RadarSystem.Communication.Utilities.ConsoleDataLogger.LogFileSaved(
                    "圆弧雷达",
                    radarImage.DeviceId,
                    radarImage.SlaveId,
                    radarImage.DataType,
                    radarImage.TypeName,
                    fullPath,
                    radarImage.Data.Length,
                    true);

                _logger.LogInformation("保存{TypeName}数据成功: {FilePath}, 大小: {Size}字节", 
                    radarImage.TypeName, fullPath, radarImage.Data.Length);
            }
            catch (Exception ex)
            {
                RadarSystem.Communication.Utilities.ConsoleDataLogger.LogFileSaved(
                    "圆弧雷达",
                    radarImage.DeviceId,
                    radarImage.SlaveId,
                    radarImage.DataType,
                    radarImage.TypeName,
                    string.IsNullOrEmpty(fullPath) ? radarImage.FilePath : fullPath,
                    radarImage.Data.Length,
                    false,
                    ex.Message);
                
                _logger.LogError(ex, "保存雷达图像失败");
            }
        }

        /// <summary>
        /// 获取设备ID（优先从缓存，其次从映射表）
        /// </summary>
        private string GetDeviceId(string slaveId)
        {
            // 优先从内存缓存获取
            var deviceId = DeviceInfoCache.GetDeviceIdByFactoryId(slaveId);
            if (!string.IsNullOrEmpty(deviceId))
            {
                return deviceId;
            }
            
            // 其次从本地映射表获取
            if (_deviceIdMap.TryGetValue(slaveId, out string? mappedId))
            {
                return mappedId;
            }

            // 最后返回slaveId本身
            return slaveId;
        }

        /// <summary>
        /// 获取文件路径：ProjectId/DeviceId_FactoryId/dataType/yyyyMMdd/HHmmss.dat
        /// </summary>
        private string GetFilePath(string dataType, string projectId, string deviceId)
        {
            // 从缓存获取设备信息
            var device = DeviceInfoCache.GetDevice(deviceId);
            
            // 构建目录：DeviceId_FactoryId
            string deviceFolder = device != null && !string.IsNullOrEmpty(device.FactoryId)
                ? $"{deviceId}_{device.FactoryId}"
                : deviceId;
            
            string dateFolder = DateTime.Now.ToString("yyyyMMdd");
            
            // 路径：basePath/ProjectId/DeviceId_FactoryId/dataType/yyyyMMdd
            return Path.Combine(_config.DataPath, "data", projectId, deviceFolder, dataType, dateFolder);
        }

        /// <summary>
        /// 解析雷达信息
        /// </summary>
        private ArcRadarInfo ParseRadarInfo(string slaveId, string deviceId, byte[] data)
        {
            // TODO: 实现完整的雷达信息解析逻辑
            return new ArcRadarInfo
            {
                SlaveId = slaveId,
                DeviceId = deviceId,
                WorkModel = 0,
                AlgorithmVersion = "",
                Temperature = 0,
                CommunicationStatus = 1,
                FpgaVersion = "",
                GpsLockStatus = 0,
                ParaSetStatus = 0,
                PowerStatus = 1,
                ProcessorStatus = 1,
                ProductDate = 0,
                RadarVersion = "",
                RfStatus = 1,
                SelfCheckStatus = "",
                LaserStatus = 0,
                WorkState = 0,
                LowPowerState = 0
            };
        }

        /// <summary>
        /// 从命令获取动作代码
        /// </summary>
        private string GetActionFromCommand(string command)
        {
            return command switch
            {
                "0300" => "02", // 开始工作
                "0400" => "03", // 停止工作
                "FF00" => "00",
                "0000" => "01", // 设置雷达参数
                "0001" => "06",
                "0A01" => "04",
                "0A02" => "05",
                "0500" => "07",
                "0600" => "08",
                "0A03" => "14",
                "0A04" => "15",
                "0B01" => "16",
                "0003" => "101",
                _ => "-1"
            };
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        private byte[] HexStringToBytes(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            
            return bytes;
        }

        /// <summary>
        /// 触发客户端连接事件
        /// </summary>
        internal void OnClientConnected(IChannelHandlerContext context)
        {
            ClientConnected?.Invoke(this, new ClientConnectedEventArgs(context));
        }

        /// <summary>
        /// 触发客户端断开事件
        /// </summary>
        internal void OnClientDisconnected(IChannelHandlerContext context)
        {
            // ✅ 查找断开的设备并更新状态
            var disconnectedDevice = _deviceChannelMap.FirstOrDefault(x => x.Value == context);
            if (!disconnectedDevice.Equals(default(KeyValuePair<string, IChannelHandlerContext>)))
            {
                string deviceId = disconnectedDevice.Key;
                var device = DeviceInfoCache.GetDevice(deviceId);
                string factoryId = device?.FactoryId ?? deviceId;
                
                SendDeviceOnlineStatus(deviceId, factoryId, false);
                _deviceChannelMap.TryRemove(deviceId, out _);
            }
            
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(context));
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }

    /// <summary>
    /// 圆弧雷达服务器处理器
    /// </summary>
    internal class ArcRadarServerHandler : ChannelHandlerAdapter
    {
        private readonly ArcRadarNettyServer _server;
        private readonly ILogger _logger;

        public ArcRadarServerHandler(ArcRadarNettyServer server, ILogger logger)
        {
            _server = server;
            _logger = logger;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogInformation("圆弧雷达客户端连接: {RemoteAddress}", context.Channel.RemoteAddress);
            _server.OnClientConnected(context);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation("圆弧雷达客户端断开: {RemoteAddress}", context.Channel.RemoteAddress);
            _server.OnClientDisconnected(context);
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    // 读取所有可用字节
                    byte[] data = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(data);
                    
                    // 显示接收的原始数据（前100字节）
                    string hex = BitConverter.ToString(data).Replace("-", "").ToUpper();
                    _logger.LogInformation("收到数据 {Length} 字节, 前缀: {Prefix}", data.Length, hex.Substring(0, Math.Min(32, hex.Length)));
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] >>> Received {data.Length} bytes, Hex: {hex.Substring(0, Math.Min(100, hex.Length))}...");
                    
                    _server.HandleData(data, context);
                }
                else if (message is byte[] data)
                {
                    _server.HandleData(data, context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChannelRead error");
            }
            finally
            {
                // 释放消息资源
                if (message is IByteBuffer buffer)
                {
                    buffer.Release();
                }
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "圆弧雷达处理器发生异常");
            context.CloseAsync();
        }
    }

    /// <summary>
    /// 圆弧雷达解码器
    /// </summary>
    public class ArcRadarDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (input.ReadableBytes < 4)
            {
                return; // 数据不足，等待更多数据
            }

            // 标记当前读取位置
            input.MarkReaderIndex();

            // 读取前4个字节作为头部
            byte[] header = new byte[4];
            input.ReadBytes(header);
            string headerHex = BitConverter.ToString(header).Replace("-", "");

            // 重置读取位置
            input.ResetReaderIndex();

            // 检查是否是有效的头部（5A5A 或 3C3C）
            if (headerHex != "5A5A" && headerHex != "3C3C")
            {
                // 跳过这个字节，继续查找
                input.ReadByte();
                return;
            }

            // 读取完整的数据包
            // 这里简化处理，实际应根据协议解析长度字段
            int availableBytes = input.ReadableBytes;
            byte[] data = new byte[availableBytes];
            input.ReadBytes(data);

            output.Add(data);
        }
    }

    /// <summary>
    /// 圆弧雷达配置
    /// </summary>
    public class ArcRadarConfiguration
    {
        public int Port { get; set; } = 1030;
        public bool Enable { get; set; } = true;
        public string ProjectId { get; set; } = "PROJECT001";
        public string DataPath { get; set; } = "../..";
        public string ApiPort { get; set; } = "80";
    }

    /// <summary>
    /// 圆弧雷达图像数据
    /// </summary>
    public class ArcRadarImage
    {
        public string SlaveId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public DateTime ReceiveTime { get; set; }
    }

    /// <summary>
    /// 圆弧雷达信息
    /// </summary>
    public class ArcRadarInfo
    {
        public string SlaveId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public int WorkModel { get; set; }
        public string AlgorithmVersion { get; set; } = string.Empty;
        public int Temperature { get; set; }
        public int CommunicationStatus { get; set; }
        public string FpgaVersion { get; set; } = string.Empty;
        public int GpsLockStatus { get; set; }
        public int ParaSetStatus { get; set; }
        public int PowerStatus { get; set; }
        public int ProcessorStatus { get; set; }
        public int ProductDate { get; set; }
        public string RadarVersion { get; set; } = string.Empty;
        public int RfStatus { get; set; }
        public string SelfCheckStatus { get; set; } = string.Empty;
        public int LaserStatus { get; set; }
        public int WorkState { get; set; }
        public int LowPowerState { get; set; }
    }

    /// <summary>
    /// 圆弧雷达数据接收事件参数
    /// </summary>
    public class ArcRadarDataReceivedEventArgs : EventArgs
    {
        public string DeviceId { get; }
        public string SlaveId { get; }
        public string Command { get; }
        public byte[] Data { get; }

        public ArcRadarDataReceivedEventArgs(string deviceId, string slaveId, string command, byte[] data)
        {
            DeviceId = deviceId;
            SlaveId = slaveId;
            Command = command;
            Data = data;
        }
    }

    /// <summary>
    /// 设备映射DTO（从API获取）
    /// </summary>
    public class DeviceMappingDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string FactoryId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }
}

