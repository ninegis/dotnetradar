using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RadarSystem.Core.Models;

namespace RadarSystem.Radar.Services
{
    /// <summary>
    /// 数据队列管理器 - 完整对应Java的QueueManager
    /// </summary>
    public class DataQueueManager : IDataQueueManager
    {
        private readonly Channel<ReceivedRadarData> _radarDataQueue;
        private readonly ILogger<DataQueueManager> _logger;
        private readonly int _capacity;

        public DataQueueManager(ILogger<DataQueueManager> logger, int capacity = 512)
        {
            _logger = logger;
            _capacity = capacity;

            // 创建有界Channel - 对应Java的ArrayBlockingQueue(512)
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };

            _radarDataQueue = Channel.CreateBounded<ReceivedRadarData>(options);
            _logger.LogInformation("数据队列管理器初始化完成，容量: {Capacity}", capacity);
        }

        /// <summary>
        /// 将数据放入队列 - 完整对应Java的putRadarDataQueue()方法
        /// </summary>
        public async Task PutRadarDataQueueAsync(ReceivedRadarData data, CancellationToken cancellationToken = default)
        {
            try
            {
                await _radarDataQueue.Writer.WriteAsync(data, cancellationToken);
                _logger.LogDebug("雷达数据已入队，设备ID: {DeviceId}", data.DeviceId);
            }
            catch (ChannelClosedException)
            {
                _logger.LogWarning("数据队列已关闭，无法入队");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "雷达数据入队失败");
                throw;
            }
        }

        /// <summary>
        /// 从队列中取出数据 - 完整对应Java的takeRadarDataQueue()方法
        /// </summary>
        public async Task<ReceivedRadarData> TakeRadarDataQueueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _radarDataQueue.Reader.ReadAsync(cancellationToken);
                _logger.LogDebug("从队列中取出雷达数据，设备ID: {DeviceId}", data.DeviceId);
                return data;
            }
            catch (ChannelClosedException)
            {
                _logger.LogWarning("数据队列已关闭，无法取出数据");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从队列中取出数据失败");
                throw;
            }
        }

        /// <summary>
        /// 尝试从队列中取出数据（非阻塞）
        /// </summary>
        public bool TryTakeRadarDataQueue(out ReceivedRadarData? data)
        {
            return _radarDataQueue.Reader.TryRead(out data);
        }

        /// <summary>
        /// 获取队列当前大小
        /// </summary>
        public int GetQueueSize()
        {
            return _radarDataQueue.Reader.Count;
        }

        /// <summary>
        /// 获取队列容量
        /// </summary>
        public int GetCapacity()
        {
            return _capacity;
        }

        /// <summary>
        /// 检查队列是否为空
        /// </summary>
        public bool IsEmpty()
        {
            return _radarDataQueue.Reader.Count == 0;
        }

        /// <summary>
        /// 检查队列是否已满
        /// </summary>
        public bool IsFull()
        {
            return _radarDataQueue.Reader.Count >= _capacity;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public async Task ClearQueueAsync()
        {
            while (_radarDataQueue.Reader.TryRead(out _))
            {
                // 清空所有数据
            }
            await Task.CompletedTask;
            _logger.LogInformation("队列已清空");
        }

        /// <summary>
        /// 获取队列的Channel（用于高级操作）
        /// </summary>
        public Channel<ReceivedRadarData> GetChannel()
        {
            return _radarDataQueue;
        }
    }

    /// <summary>
    /// 数据队列管理器接口
    /// </summary>
    public interface IDataQueueManager
    {
        Task PutRadarDataQueueAsync(ReceivedRadarData data, CancellationToken cancellationToken = default);
        Task<ReceivedRadarData> TakeRadarDataQueueAsync(CancellationToken cancellationToken = default);
        bool TryTakeRadarDataQueue(out ReceivedRadarData? data);
        int GetQueueSize();
        int GetCapacity();
        bool IsEmpty();
        bool IsFull();
        Task ClearQueueAsync();
        Channel<ReceivedRadarData> GetChannel();
    }
}
