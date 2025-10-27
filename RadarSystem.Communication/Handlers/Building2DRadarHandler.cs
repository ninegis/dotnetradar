using Microsoft.Extensions.Logging;
using RadarSystem.Communication.Models;
using RadarSystem.Communication.Utilities;
using System;

namespace RadarSystem.Communication.Handlers
{
    /// <summary>
    /// 建筑物 2D 雷达数据处理器
    /// 端口: 11135
    /// 协议: 5A5A/3C3C
    /// </summary>
    public class Building2DRadarHandler : RadarHandlerBase<Building2DRadarData>
    {
        public Building2DRadarHandler(
            ILogger<Building2DRadarHandler> logger,
            string projectId,
            string dataPath)
            : base(logger, projectId, dataPath)
        {
        }

        protected override string DeviceTypeName => "Building 2D Radar";

        protected override string[] SupportedImageTypes => new[]
        {
            "00", // 形变图
            "61", // 散斑图
            "02"  // 相干图
        };

        protected override Building2DRadarData CreateRadarData(
            byte[] msgBytes,
            string deviceId,
            string imageType,
            string imageTypeName,
            string filePath)
        {
            var radarData = new Building2DRadarData
            {
                DeviceId = deviceId,
                SlaveId = deviceId,
                Timestamp = DateTime.Now,
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

        protected override byte[] GetRawData(Building2DRadarData radarData) => radarData.RawData;

        protected override string GetFilePath(Building2DRadarData radarData) => radarData.FilePath;

        protected override string GetDeviceId(Building2DRadarData radarData) => radarData.DeviceId;
    }
}

