using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using RadarSystem.Communication.Utilities;
using System;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// MIMO 通用雷达数据处理器
    /// 端口: 11129
    /// 协议: 5A5A/3C3C
    /// </summary>
    public class MimoCommonHandler : RadarHandlerBase<MimoRadarData>
    {
        public MimoCommonHandler(
            ILogger<MimoCommonHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "MIMO Common";

        protected override string[] SupportedImageTypes => new[]
        {
            "00", // 形变图
            "61", // 散斑图
            "02", // 相干图
            "06"  // 动目标图
        };

        protected override MimoRadarData CreateRadarData(
            byte[] msgBytes,
            string deviceId,
            string imageType,
            string imageTypeName,
            string filePath)
        {
            var radarData = new MimoRadarData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = DateTime.Now,
                ImageType = imageType,
                CommandType = imageTypeName,
                RawData = msgBytes,
                DataLength = msgBytes.Length,
                FilePath = filePath
            };

            // 提取图像数据（跳过协议头）
            int headerLength = msgBytes[0] == 0x5A ? 12 : 13;
            if (msgBytes.Length > headerLength)
            {
                radarData.ImageData = ByteUtil.SubBytes(msgBytes, headerLength, msgBytes.Length - headerLength);
            }

            return radarData;
        }

        protected override byte[] GetRawData(MimoRadarData radarData) => radarData.RawData;

        protected override string GetFilePath(MimoRadarData radarData) => radarData.FilePath;

        protected override string GetDeviceId(MimoRadarData radarData) => radarData.DeviceId;
    }
}

