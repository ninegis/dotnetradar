using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Utilities;
using System;
using System.Collections.Generic;

namespace RadarSystem.Communication.Decoders
{
    /// <summary>
    /// 通用雷达协议解码器 - 适用于 MIMO Lite、MIMO、建筑物雷达等
    /// 协议格式：
    /// - 5A5A 开头的命令帧：5A5A + SlaveID(4字节) + Command(2字节) + Length(4字节) + Data
    /// - 3C3C 开头的响应帧：3C3C + SlaveID(4字节) + Command(2字节) + Status(1字节) + Length(4字节) + Data
    /// </summary>
    public class CommonRadarDecoder : ByteToMessageDecoder
    {
        private readonly ILogger _logger;
        private readonly string _deviceType;

        public CommonRadarDecoder(ILogger logger, string deviceType)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deviceType = deviceType;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                input.MarkReaderIndex();
                int totalLength = input.ReadableBytes;

                // 至少需要 4 个字节来读取前缀
                if (input.ReadableBytes < 4)
                {
                    input.ResetReaderIndex();
                    return;
                }

                // 读取 2 字节前缀
                byte[] dataPrefix = new byte[2];
                input.ReadBytes(dataPrefix);
                string prefixHexString = ByteUtil.Bytes2Str(dataPrefix).ToUpper();

                int protocolLength = 0;

                // 5A5A 命令帧：前缀(2) + SlaveID(4) + Command(2) + Length(4) = 12 字节头
                if ("5A5A".Equals(prefixHexString) && totalLength >= 12)
                {
                    byte[] headerData = new byte[10]; // 剩余的 10 字节
                    input.ReadBytes(headerData);
                    // Length 在偏移 6-9 位置（4 字节，大端序）
                    int dataLength = ByteUtil.ToInt(headerData, 6, 9);
                    protocolLength = dataLength + 12; // 总长度 = 数据长度 + 头部长度
                }
                // 3C3C 响应帧：前缀(2) + SlaveID(4) + Command(2) + Status(1) + Length(4) = 13 字节头
                else if ("3C3C".Equals(prefixHexString) && totalLength >= 13)
                {
                    byte[] headerData = new byte[11]; // 剩余的 11 字节
                    input.ReadBytes(headerData);
                    // Length 在偏移 7-10 位置（4 字节，大端序）
                    int dataLength = ByteUtil.ToInt(headerData, 7, 10);
                    protocolLength = dataLength + 13; // 总长度 = 数据长度 + 头部长度
                }

                // 数据不完整，等待更多数据
                if (totalLength < protocolLength)
                {
                    input.ResetReaderIndex();
                    return;
                }

                // 前缀不正确，尝试查找正确的前缀
                if (!"5A5A".Equals(prefixHexString) && !"3C3C".Equals(prefixHexString))
                {
                    int readableNum = input.ReadableBytes;
                    byte[] readableBytes = new byte[readableNum];
                    input.ReadBytes(readableBytes);
                    string hexStr = ByteUtil.Bytes2Str(readableBytes).ToUpper();

                    _logger.LogError($"[{_deviceType}] 数据帧错误，接收到的数据异常，数据帧信息为: {hexStr}");

                    // 检查是否包含有效前缀
                    if (!hexStr.Contains("5A5A") && !hexStr.Contains("3C3C"))
                    {
                        _logger.LogError($"[{_deviceType}] 数据帧错误，接收到的数据异常，不包含指定的帧头，丢弃该帧数据");
                        return;
                    }

                    // 尝试定位到正确的前缀位置
                    if (hexStr.Contains("5A5A"))
                    {
                        int prefixIndex = hexStr.IndexOf("5A5A");
                        input.ResetReaderIndex();
                        byte[] discardBytes = new byte[prefixIndex / 2 + dataPrefix.Length];
                        input.ReadBytes(discardBytes);
                        _logger.LogWarning($"[{_deviceType}] 数据帧错误，丢弃的帧为: {ByteUtil.Bytes2Str(discardBytes).ToUpper()}");
                        _logger.LogWarning($"[{_deviceType}] 数据帧错误，接收到的数据异常，在数据中找到指定的帧头: 5A5A，丢弃之前的帧数据");
                        return;
                    }

                    if (hexStr.Contains("3C3C"))
                    {
                        int prefixIndex = hexStr.IndexOf("3C3C");
                        input.ResetReaderIndex();
                        byte[] discardBytes = new byte[prefixIndex / 2 + dataPrefix.Length];
                        input.ReadBytes(discardBytes);
                        _logger.LogWarning($"[{_deviceType}] 数据帧错误，丢弃的帧为: {ByteUtil.Bytes2Str(discardBytes).ToUpper()}");
                        _logger.LogWarning($"[{_deviceType}] 数据帧错误，接收到的数据异常，在数据中找到指定的帧头: 3C3C，丢弃之前的帧数据");
                        return;
                    }
                }

                // 读取完整的协议数据
                input.ResetReaderIndex();
                byte[] protocolData = new byte[protocolLength];
                input.ReadBytes(protocolData);

                // 输出解码后的数据
                output.Add(protocolData);

                _logger.LogDebug($"[{_deviceType}] 成功解码数据帧，长度: {protocolLength} 字节");
            }
            catch (IndexOutOfRangeException ex)
            {
                _logger.LogError(ex, $"[{_deviceType}] 可读空间超出可写空间长度");
                input.ResetReaderIndex();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_deviceType}] 解析数据出现异常");
                input.ResetReaderIndex();
            }
        }
    }
}

