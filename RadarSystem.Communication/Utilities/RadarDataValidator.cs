using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Utilities
{
    /// <summary>
    /// 雷达数据验证器 - MD5完整性校验
    /// 参考Java实现：RadarConsumerThread.checkMD5
    /// </summary>
    public class RadarDataValidator
    {
        private readonly ILogger<RadarDataValidator> _logger;

        public RadarDataValidator(ILogger<RadarDataValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 验证数据包的MD5完整性
        /// 协议格式：[Header(12)] [Offset(4)] [MD5(16)] [Data...]
        /// </summary>
        /// <param name="packet">雷达数据包</param>
        /// <returns>校验是否通过</returns>
        public bool ValidateMD5(RadarDataPacket packet)
        {
            try
            {
                int offset = 12;  // 协议头长度
                int md5Start = offset + 4;   // offset + 4
                int md5End = offset + 20;    // offset + 20
                int md5Length = 16;          // MD5长度固定16字节

                // 检查数据长度
                if (packet.RawData.Length < md5End)
                {
                    _logger.LogWarning("数据包长度不足，无法进行MD5校验: Length={Length}, Required={Required}", 
                        packet.RawData.Length, md5End);
                    return false;
                }

                // 提取数据包中的MD5值
                byte[] packetMD5 = new byte[md5Length];
                Array.Copy(packet.RawData, md5Start, packetMD5, 0, md5Length);

                // 提取实际数据（MD5之后的部分）
                int dataLength = packet.RawData.Length - md5End;
                byte[] actualData = new byte[dataLength];
                Array.Copy(packet.RawData, md5End, actualData, 0, dataLength);

                // 计算实际数据的MD5
                byte[] computedMD5;
                using (var md5 = MD5.Create())
                {
                    computedMD5 = md5.ComputeHash(actualData);
                }

                // 比较MD5值
                bool isValid = packetMD5.SequenceEqual(computedMD5);

                if (!isValid)
                {
                    _logger.LogWarning("MD5校验失败: DeviceId={DeviceId}, SlaveId={SlaveId}, " +
                                      "Expected={Expected}, Computed={Computed}",
                        packet.DeviceId, packet.SlaveId,
                        BitConverter.ToString(packetMD5).Replace("-", ""),
                        BitConverter.ToString(computedMD5).Replace("-", ""));
                }
                else
                {
                    _logger.LogDebug("MD5校验通过: DeviceId={DeviceId}, DataLength={Length}",
                        packet.GetDeviceIdentifier(), dataLength);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MD5校验过程发生异常: {Packet}", packet);
                return false;
            }
        }

        /// <summary>
        /// 计算数据的MD5哈希值（辅助方法）
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>MD5哈希值的十六进制字符串</returns>
        public string ComputeMD5String(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 验证数据包格式是否正确
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns>是否为有效的雷达数据包</returns>
        public bool ValidatePacketFormat(byte[] data)
        {
            try
            {
                // 最小长度检查（至少需要包含协议头）
                if (data.Length < 12)
                {
                    _logger.LogWarning("数据包过短: Length={Length}", data.Length);
                    return false;
                }

                // 检查协议头（5A5A 或 3C3C）
                string header = BitConverter.ToString(data, 0, 2).Replace("-", "");
                
                if (header != "5A5A" && header != "3C3C")
                {
                    _logger.LogWarning("无效的协议头: {Header}, Expected: 5A5A or 3C3C", header);
                    return false;
                }

                _logger.LogDebug("数据包格式验证通过: Header={Header}, Length={Length}", 
                    header, data.Length);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证数据包格式时发生异常");
                return false;
            }
        }
    }
}

