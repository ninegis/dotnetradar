using System.Threading.Tasks;
using RadarSystem.Communication.Models;

namespace RadarSystem.Communication.Interfaces
{
    /// <summary>
    /// 雷达数据文件存储接口
    /// 按设备ID和日期组织文件存储
    /// </summary>
    public interface IRadarFileStorage
    {
        /// <summary>
        /// 生成文件路径
        /// 格式：{baseDataPath}/project/{projectId}/radar/{deviceId}/{yyyyMMdd}/{Type}{UUID}
        /// </summary>
        /// <param name="packet">雷达数据包</param>
        /// <returns>完整文件路径</returns>
        string GenerateFilePath(RadarDataPacket packet);

        /// <summary>
        /// 保存雷达数据到文件
        /// 自动创建目录结构
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="data">数据内容</param>
        /// <returns></returns>
        Task SaveRadarDataAsync(string filePath, byte[] data);

        /// <summary>
        /// 读取雷达数据文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件内容</returns>
        Task<byte[]> ReadRadarDataAsync(string filePath);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        bool FileExists(string filePath);

        /// <summary>
        /// 获取设备的数据文件列表
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="date">日期（可选）</param>
        /// <returns>文件路径列表</returns>
        Task<string[]> GetDeviceDataFilesAsync(string projectId, string deviceId, string? date = null);
    }
}

