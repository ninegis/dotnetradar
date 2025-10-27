using System;

namespace RadarSystem.Communication.Models
{
    /// <summary>
    /// 雷达数据包模型
    /// 不依赖设备是否在数据库中配置，只要数据完整就处理
    /// </summary>
    public class RadarDataPacket
    {
        /// <summary>
        /// 从机ID（SlaveId）- 来自雷达硬件
        /// </summary>
        public string SlaveId { get; set; } = string.Empty;

        /// <summary>
        /// 设备ID - 从设备映射表中查询，可能为空
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// 项目ID - 可能为空，使用默认值
        /// </summary>
        public string ProjectId { get; set; } = "DEFAULT";

        /// <summary>
        /// 命令码（如：0302=形变数据，0301=复散射数据）
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// 数据类型
        /// 00=形变(X), 01=复散射(F), 02=置信度(Z), 06=MIMO(M)
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// 原始数据字节
        /// </summary>
        public byte[] RawData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// MD5校验值（从数据包中提取）
        /// </summary>
        public byte[] MD5Hash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceiveTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 来源地址
        /// </summary>
        public string RemoteAddress { get; set; } = string.Empty;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int DataLength => RawData.Length;

        /// <summary>
        /// 获取数据类型前缀（用于文件名）
        /// </summary>
        public string GetDataTypePrefix()
        {
            return DataType switch
            {
                "00" => "X",  // 形变
                "01" => "F",  // 复散射
                "02" => "Z",  // 置信度
                "06" => "M",  // MIMO
                "61" => "F",  // 复散射（另一种格式）
                _ => "U"      // 未知
            };
        }

        /// <summary>
        /// 获取有效的设备标识（优先DeviceId，其次SlaveId）
        /// </summary>
        public string GetDeviceIdentifier()
        {
            return !string.IsNullOrEmpty(DeviceId) ? DeviceId : SlaveId;
        }

        public override string ToString()
        {
            return $"SlaveId={SlaveId}, DeviceId={DeviceId ?? "NULL"}, Command={Command}, " +
                   $"DataType={DataType}, Length={DataLength}, Time={ReceiveTime:yyyy-MM-dd HH:mm:ss.fff}";
        }
    }
}

