using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 雷达数据处理器接口
    /// </summary>
    public interface IRadarDataProcessor
    {
        /// <summary>
        /// 处理雷达数据
        /// </summary>
        /// <param name="receivedData">接收到的雷达数据</param>
        /// <returns>处理后的雷达数据</returns>
        Task<RadarData> ProcessRadarDataAsync(ReceivedRadarData receivedData);

        /// <summary>
        /// 计算差值图像
        /// </summary>
        /// <param name="currentData">当前数据</param>
        /// <param name="previousData">前一帧数据</param>
        /// <returns>差值图像数据</returns>
        Task<byte[]> CalculateDifferenceImageAsync(byte[] currentData, byte[] previousData);

        /// <summary>
        /// 处理雷达图像数据
        /// </summary>
        /// <param name="imageData">图像数据</param>
        /// <param name="rangeNumber">距离数量</param>
        /// <param name="angleNumber">角度数量</param>
        /// <param name="sarFileData">SAR文件数据</param>
        /// <returns>处理后的图像数据</returns>
        Task<byte[]> ProcessRadarImageDataAsync(byte[] imageData, int rangeNumber, int angleNumber, SarFileData sarFileData);

        /// <summary>
        /// 验证雷达数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="imageData">图像数据</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns>是否有效</returns>
        Task<bool> ValidateRadarDataAsync(string dataType, byte[] imageData, string deviceId);
    }
}
