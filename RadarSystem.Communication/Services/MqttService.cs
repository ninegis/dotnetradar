using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace RadarSystem.Communication.Services
{
    /// <summary>
    /// MQTT通信服务
    /// </summary>
    public class MqttService : IDisposable
    {
        private readonly ILogger<MqttService> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        private bool _isConnected = false;

        public MqttService(ILogger<MqttService> logger, MqttConfiguration config)
        {
            _logger = logger;
            _mqttClient = new MqttFactory().CreateMqttClient();
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(config.BrokerHost, config.BrokerPort)
                .WithClientId(config.ClientId)
                .WithCredentials(config.Username, config.Password)
                .WithCleanSession()
                .Build();

            // 注册事件
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        /// <summary>
        /// 连接到MQTT代理
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                // ✅ 如果已连接，直接返回true
                if (_isConnected && _mqttClient.IsConnected)
                {
                    _logger.LogInformation("MQTT已连接，跳过重复连接");
                    return true;
                }
                
                _logger.LogInformation("正在连接到MQTT代理: {Host}:{Port}", _mqttOptions.ChannelOptions, _mqttOptions.ChannelOptions);
                
                var result = await _mqttClient.ConnectAsync(_mqttOptions);
                _isConnected = result.ResultCode == MqttClientConnectResultCode.Success;
                
                if (_isConnected)
                {
                    _logger.LogInformation("MQTT连接成功");
                }
                else
                {
                    _logger.LogError("MQTT连接失败: {ResultCode}", result.ResultCode);
                }
                
                return _isConnected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接MQTT代理时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 断开MQTT连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_isConnected)
                {
                    await _mqttClient.DisconnectAsync();
                    _isConnected = false;
                    _logger.LogInformation("MQTT连接已断开");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开MQTT连接时发生错误");
            }
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        public async Task<bool> SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            try
            {
            if (!_isConnected)
            {
                // MQTT不可用时静默返回
                return false;
            }

                var subscribeOptions = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(qos)
                    .Build();

                var result = await _mqttClient.SubscribeAsync(subscribeOptions);
                
                var items = result.Items.ToList();
                if (items.Count > 0 && (items[0].ResultCode == MqttClientSubscribeResultCode.GrantedQoS0 ||
                                        items[0].ResultCode == MqttClientSubscribeResultCode.GrantedQoS1 ||
                                        items[0].ResultCode == MqttClientSubscribeResultCode.GrantedQoS2))
                {
                    _logger.LogInformation("成功订阅主题: {Topic}", topic);
                    return true;
                }
                else
                {
                    _logger.LogError("订阅主题失败: {Topic}", topic);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订阅主题时发生错误: {Topic}", topic);
                return false;
            }
        }

        /// <summary>
        /// 取消订阅主题
        /// </summary>
        public async Task<bool> UnsubscribeAsync(string topic)
        {
            try
            {
            if (!_isConnected)
            {
                // MQTT不可用时静默返回
                return false;
            }

                await _mqttClient.UnsubscribeAsync(topic);
                _logger.LogInformation("成功取消订阅主题: {Topic}", topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订阅主题时发生错误: {Topic}", topic);
                return false;
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        public async Task<bool> PublishAsync(string topic, object message, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce, bool retain = false)
        {
            try
            {
                if (!_isConnected)
                {
                    _logger.LogWarning("MQTT未连接，无法发布消息到主题: {Topic}", topic);
                    return false;
                }

                string jsonMessage;
                if (message is string strMessage)
                {
                    jsonMessage = strMessage;
                }
                else
                {
                    jsonMessage = JsonConvert.SerializeObject(message);
                }

                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(jsonMessage)
                    .WithQualityOfServiceLevel(qos)
                    .WithRetainFlag(retain)
                    .Build();

                var result = await _mqttClient.PublishAsync(mqttMessage);
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("成功发布消息到主题: {Topic}", topic);
                    return true;
                }
                else
                {
                    _logger.LogError("发布消息失败: {Topic}", topic);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布消息时发生错误: {Topic}", topic);
                return false;
            }
        }

        /// <summary>
        /// 发布字节数组消息
        /// </summary>
        public async Task<bool> PublishAsync(string topic, byte[] payload, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce, bool retain = false)
        {
            try
            {
                if (!_isConnected)
                {
                    _logger.LogWarning("MQTT未连接，无法发布消息到主题: {Topic}", topic);
                    return false;
                }

                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(qos)
                    .WithRetainFlag(retain)
                    .Build();

                var result = await _mqttClient.PublishAsync(mqttMessage);
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("成功发布字节消息到主题: {Topic}", topic);
                    return true;
                }
                else
                {
                    _logger.LogError("发布字节消息失败: {Topic}", topic);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布字节消息时发生错误: {Topic}", topic);
                return false;
            }
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation("MQTT客户端已连接");
            await Task.CompletedTask;
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("MQTT客户端已断开连接: {Reason}", e.Reason);
            _isConnected = false;
            await Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = e.ApplicationMessage.PayloadSegment.ToArray();
                var message = Encoding.UTF8.GetString(payload);

                _logger.LogDebug("收到MQTT消息 - 主题: {Topic}, 内容: {Message}", topic, message);

                // 处理接收到的消息
                await HandleReceivedMessageAsync(topic, message, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理MQTT消息时发生错误");
            }
        }

        private async Task HandleReceivedMessageAsync(string topic, string message, byte[] payload)
        {
            // 根据主题处理不同类型的消息
            if (topic.StartsWith("/radar/data/"))
            {
                await HandleRadarDataMessageAsync(topic, message, payload);
            }
            else if (topic.StartsWith("/radar/alarm/"))
            {
                await HandleAlarmMessageAsync(topic, message);
            }
            else if (topic.StartsWith("/radar/command/"))
            {
                await HandleCommandMessageAsync(topic, message);
            }
        }

        private async Task HandleRadarDataMessageAsync(string topic, string message, byte[] payload)
        {
            _logger.LogInformation("处理雷达数据消息: {Topic}", topic);
            // 实现雷达数据处理逻辑
            await Task.CompletedTask;
        }

        private async Task HandleAlarmMessageAsync(string topic, string message)
        {
            _logger.LogInformation("处理报警消息: {Topic}", topic);
            // 实现报警处理逻辑
            await Task.CompletedTask;
        }

        private async Task HandleCommandMessageAsync(string topic, string message)
        {
            _logger.LogInformation("处理命令消息: {Topic}", topic);
            // 实现命令处理逻辑
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
    }

    /// <summary>
    /// MQTT配置
    /// </summary>
    public class MqttConfiguration
    {
        public string BrokerHost { get; set; } = "localhost";
        public int BrokerPort { get; set; } = 1883;
        public string ClientId { get; set; } = "RadarSystem";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int KeepAliveInterval { get; set; } = 60;
        public int ReconnectDelay { get; set; } = 5000;
    }
}
