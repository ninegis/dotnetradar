using System;
using System.Collections.Generic;

namespace RadarSystem.Core.Models
{
    /// <summary>
    /// 项目信息模型
    /// </summary>
    public class Project
    {
        public int Id { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string CreatedBy { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        
        // 联系人信息
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        
        // 地理位置信息
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Elevation { get; set; }
        
        // 时间信息
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        
        public List<Device> Devices { get; set; } = new List<Device>();
    }

    /// <summary>
    /// 设备信息模型
    /// </summary>
    public class Device
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public int DeviceTypeCode { get; set; }
        public string Status { get; set; } = "Offline";
        
        // 地理位置信息（独立字段）
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Elevation { get; set; }
        public string Location { get; set; } = string.Empty;  // 保留用于向后兼容
        
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string MqttTopic { get; set; } = string.Empty;
        
        // 雷达特有信息
        public string FactoryId { get; set; } = string.Empty;  // 出厂ID
        public double Orientation { get; set; }  // 零点朝向（度）
        
        // 雷达参数（前端需要）
        public Dictionary<string, object>? Params { get; set; }  // 雷达参数
        public Dictionary<string, object>? AlgorithmParam { get; set; }  // 算法参数
        
        public string Description { get; set; } = string.Empty;
        public DateTime LastUpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// 项目查询请求
    /// </summary>
    public class ProjectQueryRequest
    {
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 设备查询请求
    /// </summary>
    public class DeviceQueryRequest
    {
        public string? DeviceId { get; set; }
        public string? ProjectId { get; set; }
        public string? DeviceName { get; set; }
        public string? Status { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 项目创建请求
    /// </summary>
    public class CreateProjectRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        
        // 联系人信息
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        
        // 地理位置信息
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Elevation { get; set; }
        
        // 时间信息
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// 设备创建请求
    /// </summary>
    public class CreateDeviceRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public int DeviceTypeCode { get; set; }
        
        // 地理位置信息
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Elevation { get; set; }
        public string Location { get; set; } = string.Empty;  // 保留用于向后兼容
        
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string MqttTopic { get; set; } = string.Empty;
        
        // 雷达特有信息
        public string FactoryId { get; set; } = string.Empty;  // 出厂ID
        public double Orientation { get; set; }  // 零点朝向（度）
        
        public string Description { get; set; } = string.Empty;
    }
}

