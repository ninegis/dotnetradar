using System.Threading.Tasks;
using RadarSystem.Core.Models;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// 图像分析服务接口
    /// </summary>
    public interface IImageAnalysisService
    {
        /// <summary>
        /// 分析雷达图像
        /// </summary>
        Task<ImageAnalysisResult> AnalyzeRadarImageAsync(byte[] imageData, RadarData radarData);

        /// <summary>
        /// 计算差值图像
        /// </summary>
        Task<byte[]> CalculateDifferenceImageAsync(byte[] currentImage, byte[] previousImage);

        /// <summary>
        /// 检测目标
        /// </summary>
        Task<TargetDetectionResult> DetectTargetsAsync(byte[] imageData, DetectionParameters parameters);
    }
}
