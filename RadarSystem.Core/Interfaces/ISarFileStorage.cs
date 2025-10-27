using System.Threading.Tasks;

namespace RadarSystem.Core.Interfaces
{
    /// <summary>
    /// SAR文件存储接口
    /// </summary>
    public interface ISarFileStorage
    {
        /// <summary>
        /// 保存SAR文件
        /// </summary>
        /// <param name="fileData">文件数据</param>
        /// <param name="fileName">文件名</param>
        /// <param name="projectName">项目名称</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="timestamp">时间戳</param>
        /// <returns>返回[完整路径, 相对路径]</returns>
        Task<string[]> SaveAsync(byte[] fileData, string fileName, string projectName, string deviceId, long timestamp);

        /// <summary>
        /// 读取SAR文件
        /// </summary>
        Task<byte[]> ReadAsync(string filePath);

        /// <summary>
        /// 删除SAR文件
        /// </summary>
        Task<bool> DeleteAsync(string filePath);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        bool Exists(string filePath);
    }
}