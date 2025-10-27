using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RadarSystem.Communication.Decoders
{
    /// <summary>
    /// NMEA 0183 协议解码器
    /// 用于解析 GPS/GNSS 设备的 NMEA 语句
    /// 协议格式: $GPGGA,时间,纬度,N/S,经度,E/W,定位质量,卫星数...*校验和\r\n
    /// </summary>
    public class NmeaDecoder : ByteToMessageDecoder
    {
        private readonly ILogger _logger;
        private readonly string _deviceType;
        private readonly int _maxLineLength;

        public NmeaDecoder(ILogger logger, string deviceType, int maxLineLength = 1024)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deviceType = deviceType;
            _maxLineLength = maxLineLength;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                // 循环读取所有可用的 NMEA 语句
                while (input.ReadableBytes > 0)
                {
                    input.MarkReaderIndex();

                    // 查找行结束符 \r\n
                    int lineEndIndex = FindLineEnd(input);
                    
                    if (lineEndIndex == -1)
                    {
                        // 没有找到完整的行，等待更多数据
                        input.ResetReaderIndex();
                        
                        // 防止缓冲区溢出
                        if (input.ReadableBytes > _maxLineLength)
                        {
                            _logger.LogWarning($"[{_deviceType}] NMEA 行超过最大长度 {_maxLineLength}，丢弃数据");
                            input.SkipBytes(input.ReadableBytes);
                        }
                        return;
                    }

                    // 计算行长度（不包括 \r\n）
                    int lineLength = lineEndIndex - input.ReaderIndex;
                    
                    if (lineLength > _maxLineLength)
                    {
                        _logger.LogWarning($"[{_deviceType}] NMEA 行长度 {lineLength} 超过最大长度 {_maxLineLength}，跳过");
                        input.SkipBytes(lineLength + 2); // +2 for \r\n
                        continue;
                    }

                    // 读取一行数据
                    byte[] lineBytes = new byte[lineLength];
                    input.ReadBytes(lineBytes);
                    
                    // 跳过 \r\n
                    input.SkipBytes(2);

                    // 转换为字符串
                    string line = Encoding.ASCII.GetString(lineBytes).Trim();

                    // 验证 NMEA 语句
                    if (IsValidNmeaSentence(line))
                    {
                        output.Add(line);
                        _logger.LogDebug($"[{_deviceType}] 解码 NMEA 语句: {line}");
                    }
                    else
                    {
                        _logger.LogWarning($"[{_deviceType}] 无效的 NMEA 语句: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_deviceType}] 解析 NMEA 数据时发生异常");
                input.ResetReaderIndex();
            }
        }

        /// <summary>
        /// 查找行结束符 \r\n 的位置
        /// </summary>
        private int FindLineEnd(IByteBuffer buffer)
        {
            int readerIndex = buffer.ReaderIndex;
            int writerIndex = buffer.WriterIndex;

            for (int i = readerIndex; i < writerIndex - 1; i++)
            {
                if (buffer.GetByte(i) == '\r' && buffer.GetByte(i + 1) == '\n')
                {
                    return i;
                }
            }

            return -1; // 未找到
        }

        /// <summary>
        /// 验证 NMEA 语句是否有效
        /// </summary>
        private bool IsValidNmeaSentence(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return false;

            // NMEA 语句必须以 $ 开头
            if (!sentence.StartsWith("$"))
                return false;

            // NMEA 语句必须包含 * 和校验和
            int asteriskIndex = sentence.IndexOf('*');
            if (asteriskIndex == -1 || asteriskIndex >= sentence.Length - 2)
            {
                _logger.LogDebug($"[{_deviceType}] NMEA 语句缺少校验和: {sentence}");
                return true; // 有些设备可能不发送校验和，仍然接受
            }

            // 验证校验和
            try
            {
                string data = sentence.Substring(1, asteriskIndex - 1); // 去掉 $ 和 *
                string checksumStr = sentence.Substring(asteriskIndex + 1, 2);
                byte expectedChecksum = byte.Parse(checksumStr, System.Globalization.NumberStyles.HexNumber);
                byte actualChecksum = CalculateChecksum(data);

                if (expectedChecksum != actualChecksum)
                {
                    _logger.LogWarning($"[{_deviceType}] NMEA 校验和不匹配: 期望 {expectedChecksum:X2}, 实际 {actualChecksum:X2}, 语句: {sentence}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{_deviceType}] 验证 NMEA 校验和时发生错误: {sentence}");
                return false;
            }
        }

        /// <summary>
        /// 计算 NMEA 校验和
        /// </summary>
        private byte CalculateChecksum(string data)
        {
            byte checksum = 0;
            foreach (char c in data)
            {
                checksum ^= (byte)c;
            }
            return checksum;
        }
    }
}

