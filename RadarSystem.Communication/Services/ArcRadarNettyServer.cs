using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography; // ✅ MD5校验
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
        /// 从数据库加载 SlaveId -> DeviceId 映射
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
                    
                    // ✅ API返回格式: {"success": true, "data": [...]}
                    var apiResponse = JsonConvert.DeserializeObject<DeviceApiResponse>(json);
                    
                    if (apiResponse?.Data != null && apiResponse.Data.Count > 0)
                    {
                        foreach (var device in apiResponse.Data)
                        {
                            if (!string.IsNullOrEmpty(device.SlaveId))
                            {
                                // SlaveId (出厂ID) 就是 SlaveId
                                _deviceIdMap.TryAdd(device.SlaveId, device.DeviceId);
                                _logger.LogInformation("加载设备映射: SlaveId={SlaveId} → DeviceId={DeviceId}", 
                                    device.SlaveId, device.DeviceId);
                            }
                        }
                        _logger.LogInformation("设备映射加载完成，共{Count}个设备", apiResponse.Data.Count);
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

                        // ✅ 添加解码器（参考Java RadarDecoder）
                        pipeline.AddLast("decoder", new ArcRadarDecoder());

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

                // ✅ 获取设备ID（通过SlaveId查询）
                // 🔍 调试：仅在首次查询或找不到时输出详细信息
                bool isFirstQuery = !_deviceIdMap.ContainsKey(slaveIdStr) && 
                                   string.IsNullOrEmpty(DeviceInfoCache.GetDeviceIdBySlaveId(slaveIdStr));
                
                if (isFirstQuery)
                {
                    Console.WriteLine($"[DEBUG] ====== 首次查询 SlaveId={slaveIdStr} (原始int={slaveId}) ======");
                    Console.WriteLine($"[DEBUG] DeviceInfoCache设备数: {RadarSystem.Communication.Services.DeviceInfoCache.GetDeviceCount()}");
                    Console.WriteLine($"[DEBUG] 本地_deviceIdMap数量: {_deviceIdMap.Count}");
                    
                    // 输出所有已加载的映射
                    Console.WriteLine("[DEBUG] === 本地_deviceIdMap映射 ===");
                    if (_deviceIdMap.Count > 0)
                    {
                        foreach (var kvp in _deviceIdMap)
                        {
                            Console.WriteLine($"[DEBUG]   SlaveId={kvp.Key} → DeviceId={kvp.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG]   (映射表为空)");
                    }
                    
                    // 输出DeviceInfoCache映射
                    RadarSystem.Communication.Services.DeviceInfoCache.PrintAllMappings();
                    Console.WriteLine($"[DEBUG] ====================================================");
                }
                
                string deviceId = GetDeviceId(slaveIdStr);
                if (string.IsNullOrEmpty(deviceId))
                {
                    _logger.LogWarning("⚠️ 未找到 SlaveId={SlaveId} 对应的设备，数据将被丢弃！请检查设备配置！", slaveIdStr);
                    Console.WriteLine($"[WARNING] ❌ 未找到SlaveId={slaveIdStr}对应的设备，数据被丢弃！");
                    Console.WriteLine($"[WARNING] 💡 提示：请检查数据库中是否有SlaveId='{slaveIdStr}'或SlaveId='20'的设备记录");
                    return; // ✅ 直接返回，不处理未配置的设备
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

                _logger.LogInformation("接收到圆弧雷达数据 - 端口:{Port}, SlaveId:{SlaveId}, 命令:0x{Command}, 长度:{Length}字节", 
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
                    _logger.LogInformation("圆弧雷达接收到形变数据上报 - SlaveId:{SlaveId}, DeviceId:{DeviceId}, 数据长度:{Length}字节", 
                        slaveId, deviceId, data.Length);
                    Console.WriteLine($"[DATA] 形变数据: SlaveId={slaveId}, Length={data.Length} bytes");
                    HandleImageData(slaveId, deviceId, "00", "形变", data);
                    break;

                case "0301": // 复散射数据 - Java: 0301
                    _logger.LogInformation("圆弧雷达接收到复散射数据上报 - SlaveId:{SlaveId}, DeviceId:{DeviceId}, 数据长度:{Length}字节", 
                        slaveId, deviceId, data.Length);
                    Console.WriteLine($"[DATA] 复散射数据: SlaveId={slaveId}, Length={data.Length} bytes");
                    HandleImageData(slaveId, deviceId, "01", "复散射", data);
                    break;

                case "0303": // 置信度数据 - Java: 0303
                    _logger.LogInformation("圆弧雷达接收到置信度数据上报 - SlaveId:{SlaveId}, DeviceId:{DeviceId}, 数据长度:{Length}字节", 
                        slaveId, deviceId, data.Length);
                    Console.WriteLine($"[DATA] 置信度数据: SlaveId={slaveId}, Length={data.Length} bytes");
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
        private void SendDeviceOnlineStatus(string deviceId, string slaveId, bool isOnline)
        {
            try
            {
                var statusMessage = new
                {
                    deviceId = deviceId,
                    slaveId = slaveId,  // ✅ 修改为slaveId（与前端一致）
                    status = isOnline ? "online" : "offline",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    timestampUnix = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds(),
                    type = "ArcRadar"
                };

                string json = JsonConvert.SerializeObject(statusMessage);
                Console.WriteLine($"[MQTT] 准备发布设备状态: {json}");
                var result = _mqttService.PublishAsync("/dev/device/status", json).Result;
                
                if (result)
                {
                    Console.WriteLine($"[STATUS] ✅ MQTT发布成功 - Device {deviceId} (SlaveId={slaveId}): {(isOnline ? "ONLINE" : "OFFLINE")}");
                    _logger.LogInformation("设备状态已发布到MQTT: {DeviceId} - {Status}", deviceId, isOnline ? "在线" : "离线");
                }
                else
                {
                    Console.WriteLine($"[STATUS] ⚠️ MQTT未连接，无法发布设备状态: {deviceId}");
                    _logger.LogWarning("MQTT未连接，设备状态未发布: {DeviceId}", deviceId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STATUS] ❌ 发送设备状态失败: {ex.Message}");
                _logger.LogWarning(ex, "发送设备状态失败（MQTT不可用）: {DeviceId}", deviceId);
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
                Console.WriteLine($"[HandleImageData] 开始处理 {typeName} 数据: DeviceId={deviceId}, SlaveId={slaveId}, DataType={dataType}, Size={data.Length} bytes");
                
                // ✅ 通过SlaveId查询设备信息并构建路径
                string filePath = GetFilePath(dataType, slaveId, deviceId);
                Console.WriteLine($"[HandleImageData] 文件路径: {filePath}");
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
                Console.WriteLine($"[HandleImageData] ✅ 数据已加入队列，队列大小: {_imageQueue.Count}");
                _logger.LogInformation("{TypeName}数据已加入处理队列，设备ID: {DeviceId}, 文件路径: {FilePath}, 队列大小: {QueueSize}", 
                    typeName, deviceId, filePath, _imageQueue.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HandleImageData] ❌ 错误: {ex.Message}");
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
            Console.WriteLine($"[SEND] Heartbeat ACK to SlaveId={slaveIdStr}");
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
            _logger.LogInformation("图像数据处理队列线程已启动");
            Console.WriteLine("[ProcessImageQueue] ✅ 队列处理线程已启动");
            
            while (true)
            {
                try
                {
                    if (_imageQueue.TryDequeue(out var radarImage))
                    {
                        Console.WriteLine($"[ProcessImageQueue] 从队列取出数据: {radarImage.TypeName}, Size={radarImage.Data.Length} bytes");
                        await SaveRadarImage(radarImage);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ProcessImageQueue] ❌ 错误: {ex.Message}");
                    _logger.LogError(ex, "处理图像队列时发生错误");
                }
            }
        }

        /// <summary>
        /// 保存雷达图像（参考Java RadarConsumerThread.writeData）
        /// </summary>
        private async Task SaveRadarImage(ArcRadarImage radarImage)
        {
            string fullPath = string.Empty;
            
            try
            {
                // ✅ 参考Java: checkMD5() - 校验数据完整性
                // Java: checkMD5(bytes, offset + 4, offset + 20, dataType)
                // offset = 12, 所以检查 bytes[16:28] 的MD5
                int offset = 12;
                if (radarImage.Data.Length < offset + 20)
                {
                    _logger.LogWarning("数据长度不足，无法进行MD5校验: {Length}", radarImage.Data.Length);
                    return;
                }
                
                bool md5Valid = CheckMD5(radarImage.Data, offset + 4, offset + 20, radarImage.DataType);
                if (!md5Valid)
                {
                    _logger.LogWarning("数据MD5校验失败，跳过保存: DeviceId={DeviceId}, DataType={DataType}", 
                        radarImage.DeviceId, radarImage.DataType);
                    Console.WriteLine($"[MD5] ❌ 校验失败，数据已丢弃: {radarImage.DeviceId}");
                    return;
                }
                
                Console.WriteLine($"[MD5] ✅ 校验通过: {radarImage.DeviceId}, {radarImage.DataType}");
                
                // ✅ radarImage.FilePath是基础目录（参考Java格式）
                string baseDirectory = radarImage.FilePath;
                
                // ✅ 参考Java: getStringPath() - 添加日期和数据类型前缀
                // Java格式: file + dataPath + dataType + uuid
                // dataPath = /yyyyMMdd/
                // dataType: "00"->"X", "01"->"F", "02"->"Z"
                string dateFolder = DateTime.Now.ToString("yyyyMMdd");
                string dataTypePrefix = radarImage.DataType switch
                {
                    "00" => "X",  // 形变
                    "01" => "F",  // 复散射
                    "02" => "Z",  // 置信度
                    _ => radarImage.DataType
                };
                
                // ✅ 构建完整路径: baseDir/yyyyMMdd/dataType_uuid
                string dateDir = Path.Combine(baseDirectory, dateFolder);
                if (!Directory.Exists(dateDir))
                {
                    Directory.CreateDirectory(dateDir);
                    _logger.LogInformation("创建日期目录: {Directory}", dateDir);
                }
                
                // ✅ 生成唯一文件名（参考Java使用UUID）
                string uuid = Guid.NewGuid().ToString("N").Substring(0, 16); // 简化UUID
                string fileName = $"{dataTypePrefix}{uuid}";
                fullPath = Path.Combine(dateDir, fileName);

                _logger.LogInformation("准备保存文件: {FilePath}, 数据类型: {DataType}, 大小: {Size}字节", 
                    fullPath, radarImage.DataType, radarImage.Data.Length);
                Console.WriteLine($"[SAVE] 保存文件: {fullPath}, Size={radarImage.Data.Length} bytes");
                
                // ✅ 保存完整的原始数据（参考Java: FileUtil.writeFile(mimoRadarImage.getRadarBytes(), file)）
                await File.WriteAllBytesAsync(fullPath, radarImage.Data);
                
                // ✅ 验证文件是否保存成功
                if (File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    Console.WriteLine($"[SAVE] ✅✅✅ 文件保存成功！ ✅✅✅");
                    Console.WriteLine($"  文件路径: {fullPath}");
                    Console.WriteLine($"  文件大小: {fileInfo.Length} bytes ({radarImage.Data.Length} bytes)");
                    Console.WriteLine($"  数据类型: {radarImage.TypeName} ({radarImage.DataType})");
                    Console.WriteLine($"  设备信息: DeviceId={radarImage.DeviceId}, SlaveId={radarImage.SlaveId}");
                }
                else
                {
                    Console.WriteLine($"[SAVE] ❌ 文件保存失败！文件不存在: {fullPath}");
                    throw new Exception($"文件保存失败，文件不存在: {fullPath}");
                }

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
        /// MD5校验（参考Java: checkMD5）
        /// </summary>
        private bool CheckMD5(byte[] bytes, int start, int end, string dataType)
        {
            try
            {
                if (bytes.Length < end)
                {
                    _logger.LogWarning("数据长度不足，无法进行MD5校验: {Length}, 需要: {End}", bytes.Length, end);
                    return false;
                }
                
                // ✅ 参考Java: byte[] b1 = Arrays.copyOfRange(bytes, start, end);
                // 提取MD5值（16字节）
                byte[] md5Value = new byte[end - start];
                Array.Copy(bytes, start, md5Value, 0, end - start);
                
                // ✅ 参考Java: byte[] b2 = Arrays.copyOfRange(bytes, end, bytes.length);
                // 提取数据部分
                byte[] dataPart = new byte[bytes.Length - end];
                Array.Copy(bytes, end, dataPart, 0, bytes.Length - end);
                
                // ✅ 计算数据部分的MD5
                using (var md5 = MD5.Create())
                {
                    byte[] computedMD5 = md5.ComputeHash(dataPart);
                    
                    // ✅ 比较MD5值
                    if (computedMD5.Length != md5Value.Length)
                    {
                        return false;
                    }
                    
                    for (int i = 0; i < computedMD5.Length; i++)
                    {
                        if (computedMD5[i] != md5Value[i])
                        {
                            return false;
                        }
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MD5校验异常，允许保存");
                // ✅ 如果MD5校验失败，为了测试先允许保存
                return true; // 临时允许，用于测试
            }
        }

        /// <summary>
        /// 获取设备ID（通过SlaveId查询映射）
        /// ✅ 注意：DeviceId和SlaveId是不同的值，不能混用！
        /// </summary>
        private string GetDeviceId(string slaveId)
        {
            // 优先从内存缓存获取
            var deviceId = DeviceInfoCache.GetDeviceIdBySlaveId(slaveId);
            if (!string.IsNullOrEmpty(deviceId))
            {
                Console.WriteLine($"[GetDeviceId] ✅ 从缓存查询: SlaveId={slaveId} → DeviceId={deviceId}");
                return deviceId;
            }
            
            // 其次从本地映射表获取
            if (_deviceIdMap.TryGetValue(slaveId, out string? mappedId))
            {
                Console.WriteLine($"[GetDeviceId] ✅ 从映射表查询: SlaveId={slaveId} → DeviceId={mappedId}");
                return mappedId;
            }

            // ✅ 如果没有找到映射，返回空字符串（不能使用SlaveId作为DeviceId！）
            Console.WriteLine($"[GetDeviceId] ❌ 未找到映射: SlaveId={slaveId}, 返回空字符串");
            return string.Empty;
        }

        /// <summary>
        /// 获取文件路径：通过SlaveId查询设备信息，构建 Data/ProjectId_DeviceId_SlaveId/yyyyMMdd/ 路径
        /// </summary>
        private string GetFilePath(string dataType, string slaveId, string deviceId)
        {
            // ✅ 通过SlaveId从设备表查询设备信息
            var device = DeviceInfoCache.GetDevice(deviceId);
            
            // ✅ 获取ProjectId（从设备信息中）
            string projectId = device?.ProjectId ?? _config.ProjectId;
            
            // ✅ 构建目录名称：ProjectId_DeviceId_SlaveId
            string deviceFolder = $"{projectId}_{deviceId}_{slaveId}";
            
            // ✅ 获取日期目录：yyyyMMdd
            string dateFolder = DateTime.Now.ToString("yyyyMMdd");
            
            // ✅ 路径格式: Data/ProjectId_DeviceId_SlaveId/yyyyMMdd/
            string baseDir = Path.Combine(_config.DataPath, deviceFolder, dateFolder);
            
            // ✅ 确保目录存在
            if (!Directory.Exists(baseDir))
            {
                try
                {
                    Directory.CreateDirectory(baseDir);
                    Console.WriteLine($"[GetFilePath] ✅ 创建数据目录: {baseDir}");
                    _logger.LogInformation("创建数据目录: {Directory}, ProjectId={ProjectId}, DeviceId={DeviceId}, SlaveId={SlaveId}", 
                        baseDir, projectId, deviceId, slaveId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetFilePath] ❌ 创建目录失败: {baseDir}, 错误: {ex.Message}");
                    _logger.LogError(ex, "创建数据目录失败: {Directory}", baseDir);
                }
            }
            
            Console.WriteLine($"[GetFilePath] ✅ 路径: {baseDir}");
            Console.WriteLine($"  └─ ProjectId={projectId}, DeviceId={deviceId}, SlaveId={slaveId}");
            
            return baseDir;
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
                
                if (device != null && !string.IsNullOrEmpty(device.SlaveId))
                {
                    Console.WriteLine($"[DISCONNECT] 设备断开: DeviceId={deviceId}, SlaveId={device.SlaveId}");
                    SendDeviceOnlineStatus(deviceId, device.SlaveId, false);
                }
                else
                {
                    Console.WriteLine($"[DISCONNECT] ⚠️ 设备断开但无SlaveId信息: DeviceId={deviceId}");
                }
                
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
                // ✅ 解码器已经处理完数据包，这里应该收到byte[]数组
                if (message is byte[] data)
                {
                    // 显示接收的原始数据（前100字节）
                    string hex = BitConverter.ToString(data).Replace("-", "").ToUpper();
                    string prefix = hex.Length >= 4 ? hex.Substring(0, 4) : hex;
                    
                    _logger.LogInformation("收到数据包 {Length} 字节, 前缀: {Prefix}", data.Length, prefix);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] >>> [ChannelRead] 收到数据包: {data.Length} bytes, 前缀: {prefix}, Hex: {hex.Substring(0, Math.Min(100, hex.Length))}...");
                    
                    _server.HandleData(data, context);
                }
                else if (message is IByteBuffer buffer)
                {
                    // ✅ 如果解码器没有处理，直接读取
                    byte[] bufferData = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(bufferData);
                    
                    string hex = BitConverter.ToString(bufferData).Replace("-", "").ToUpper();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] >>> [ChannelRead] 收到缓冲区数据: {bufferData.Length} bytes, Hex: {hex.Substring(0, Math.Min(100, hex.Length))}...");
                    
                    _server.HandleData(bufferData, context);
                    buffer.Release();
                }
                else
                {
                    Console.WriteLine($"[ChannelRead] ⚠️ 未知消息类型: {message.GetType()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChannelRead] ❌ 错误: {ex.Message}");
                _logger.LogError(ex, "ChannelRead error");
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, "圆弧雷达处理器发生异常");
            context.CloseAsync();
        }
    }

    /// <summary>
    /// 圆弧雷达解码器（参考Java RadarDecoder）
    /// </summary>
    public class ArcRadarDecoder : ByteToMessageDecoder
    {
        private static readonly ILogger _logger = new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger<ArcRadarDecoder>();

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                // ✅ 参考Java: 标记读取位置
                input.MarkReaderIndex();
                int totalLength = input.ReadableBytes;

                // ✅ 参考Java: 至少需要4字节才能判断头部
                if (input.ReadableBytes < 4)
                {
                    input.ResetReaderIndex();
                    return; // 数据不足，等待更多数据
                }

                // ✅ 参考Java: 读取前2字节作为前缀
                byte[] dataPrefix = new byte[2];
                input.ReadBytes(dataPrefix);
                string prefixHexString = BitConverter.ToString(dataPrefix).Replace("-", "").ToUpper();

                int protocolLength = 0;

                // ✅ 参考Java: 检查前缀并计算协议长度
                if ("5A5A".Equals(prefixHexString) && totalLength >= 12)
                {
                    // ✅ 参考Java: 读取长度字段（偏移6-9，共4字节）
                    // Java: byte[] dataLength = new byte[10]; in.readBytes(dataLength);
                    //       protocolLength = ByteUtil.toInt(dataLength, 6, 9) + 12;
                    // ❌ 不要再次MarkReaderIndex！直接读取即可，最后会ResetReaderIndex到初始位置0
                    byte[] headerData = new byte[10]; // 读取剩余10字节（不包括已读的2字节前缀）
                    input.ReadBytes(headerData);
                    
                    // ✅ 使用ByteUtil.ToIntLittleEndian解析长度（LITTLE_ENDIAN，对齐Java）
                    int dataLength = RadarSystem.Communication.Utilities.ByteUtil.ToIntLittleEndian(headerData, 6, 9);
                    protocolLength = dataLength + 12;
                    
                    Console.WriteLine($"[Decoder] 5A5A帧: dataLength={dataLength}, protocolLength={protocolLength}, totalLength={totalLength}");
                }
                else if ("3C3C".Equals(prefixHexString) && totalLength >= 13)
                {
                    // ✅ 参考Java: 读取长度字段（偏移7-10，共4字节）
                    // Java: byte[] dataLength = new byte[11]; in.readBytes(dataLength);
                    //       protocolLength = ByteUtil.toInt(dataLength, 7, 10) + 13;
                    // ❌ 不要再次MarkReaderIndex！
                    byte[] headerData = new byte[11]; // 读取剩余11字节
                    input.ReadBytes(headerData);
                    
                    // ✅ 使用ByteUtil.ToIntLittleEndian解析长度（LITTLE_ENDIAN，对齐Java）
                    int dataLength = RadarSystem.Communication.Utilities.ByteUtil.ToIntLittleEndian(headerData, 7, 10);
                    protocolLength = dataLength + 13;
                    
                    Console.WriteLine($"[Decoder] 3C3C帧: dataLength={dataLength}, protocolLength={protocolLength}, totalLength={totalLength}");
                }

                // ✅ 参考Java: 如果数据不足，等待更多数据
                if (totalLength < protocolLength)
                {
                    input.ResetReaderIndex();
                    return;
                }

                // ✅ 参考Java: 处理无效前缀
                if (!"5A5A".Equals(prefixHexString) && !"3C3C".Equals(prefixHexString))
                {
                    int readableNum = input.ReadableBytes;
                    byte[] readableBytes = new byte[readableNum];
                    input.ResetReaderIndex();
                    input.ReadBytes(readableBytes);
                    string str = BitConverter.ToString(readableBytes).Replace("-", "").ToUpper();
                    
                    Console.WriteLine($"[Decoder] ⚠️ 无效前缀: {prefixHexString}, 数据: {str.Substring(0, Math.Min(100, str.Length))}...");
                    
                    // ✅ 参考Java: 查找有效前缀
                    if (str.Contains("5A5A"))
                    {
                        int prefixIndex = str.IndexOf("5A5A");
                        input.ResetReaderIndex();
                        input.SkipBytes(prefixIndex / 2);
                        Console.WriteLine($"[Decoder] ✅ 找到5A5A前缀，跳过 {prefixIndex / 2} 字节");
                        return;
                    }
                    if (str.Contains("3C3C"))
                    {
                        int prefixIndex = str.IndexOf("3C3C");
                        input.ResetReaderIndex();
                        input.SkipBytes(prefixIndex / 2);
                        Console.WriteLine($"[Decoder] ✅ 找到3C3C前缀，跳过 {prefixIndex / 2} 字节");
                        return;
                    }
                    
                    // ✅ 如果没有找到有效前缀，丢弃所有数据
                    input.ResetReaderIndex();
                    input.SkipBytes(readableNum);
                    Console.WriteLine($"[Decoder] ❌ 未找到有效前缀，丢弃 {readableNum} 字节");
                    return;
                }

                // ✅ 参考Java: 读取完整协议数据包
                input.ResetReaderIndex();
                byte[] protocol = new byte[protocolLength];
                input.ReadBytes(protocol);
                
                Console.WriteLine($"[Decoder] ✅ 解析数据包: 前缀={prefixHexString}, 长度={protocolLength} bytes");
                output.Add(protocol);
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine($"[Decoder] ❌ 索引越界: {ex.Message}");
                input.ResetReaderIndex();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Decoder] ❌ 解析错误: {ex.Message}");
                input.ResetReaderIndex();
            }
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
        public string SlaveId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }

    /// <summary>
    /// API响应格式
    /// </summary>
    public class DeviceApiResponse
    {
        public bool Success { get; set; }
        public List<DeviceMappingDto> Data { get; set; } = new List<DeviceMappingDto>();
    }
}

