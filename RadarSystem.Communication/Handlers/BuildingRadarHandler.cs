using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using RadarSystem.Communication.Utilities;
using System;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 建筑物雷达数据处理器
    /// 端口: 1060
    /// 协议: 5A5A/3C3C
    /// </summary>
    public class BuildingRadarHandler : RadarHandlerBase<BuildingRadarData>
    {
        public BuildingRadarHandler(
            ILogger<BuildingRadarHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Building Radar";

        protected override string[] SupportedImageTypes => new[]
        {
            "00", // 形变图
            "61"  // 散斑图
        };

        protected override BuildingRadarData CreateRadarData(
            byte[] msgBytes,
            string deviceId,
            string imageType,
            string imageTypeName,
            string filePath)
        {
            var radarData = new BuildingRadarData
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

        protected override byte[] GetRawData(BuildingRadarData radarData) => radarData.RawData;

        protected override string GetFilePath(BuildingRadarData radarData) => radarData.FilePath;

        protected override string GetDeviceId(BuildingRadarData radarData) => radarData.DeviceId;
    }
}

