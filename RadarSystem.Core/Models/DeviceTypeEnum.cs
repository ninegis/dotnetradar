using System;
using System.ComponentModel;

namespace RadarSystem.Core.Models
{
    /// <summary>
    /// 设备类型枚举（对应Java中的设备类型编码）
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// 边坡雷达
        /// </summary>
        [Description("边坡雷达")]
        SlopeRadar = 1,

        /// <summary>
        /// 视频
        /// </summary>
        [Description("视频")]
        Video = 2,

        /// <summary>
        /// 气象站
        /// </summary>
        [Description("气象站")]
        WeatherStation = 3,

        /// <summary>
        /// GNSS
        /// </summary>
        [Description("GNSS")]
        GNSS = 4,

        /// <summary>
        /// 建筑物雷达
        /// </summary>
        [Description("建筑物雷达")]
        BuildingRadar = 5,

        /// <summary>
        /// 边坡雷达 Mini
        /// </summary>
        [Description("边坡雷达 Mini")]
        SlopeRadarMini = 6,

        /// <summary>
        /// 建筑物雷达 2D
        /// </summary>
        [Description("建筑物雷达 2D")]
        BuildingRadar2D = 7,

        /// <summary>
        /// MIMO 雷达
        /// </summary>
        [Description("MIMO 雷达")]
        MIMORadar = 8,

        /// <summary>
        /// 普适雷达
        /// </summary>
        [Description("普适雷达")]
        UniversalRadar = 9,

        /// <summary>
        /// 球形摄像机
        /// </summary>
        [Description("球形摄像机")]
        DomeCamera = 10,

        /// <summary>
        /// 测斜计
        /// </summary>
        [Description("测斜计")]
        Inclinometer = 11,

        /// <summary>
        /// 振动传感器
        /// </summary>
        [Description("振动传感器")]
        VibrationSensor = 12,

        /// <summary>
        /// 电机 (外设)
        /// </summary>
        [Description("电机 (外设)")]
        Motor = 13
    }

    /// <summary>
    /// 设备类型扩展方法
    /// </summary>
    public static class DeviceTypeExtensions
    {
        /// <summary>
        /// 获取设备类型的描述
        /// </summary>
        public static string GetDescription(this DeviceType deviceType)
        {
            var field = deviceType.GetType().GetField(deviceType.ToString());
            if (field != null)
            {
                var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
            return deviceType.ToString();
        }

        /// <summary>
        /// 从描述获取设备类型
        /// </summary>
        public static DeviceType? FromDescription(string description)
        {
            foreach (DeviceType type in Enum.GetValues(typeof(DeviceType)))
            {
                if (type.GetDescription() == description)
                {
                    return type;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有设备类型列表
        /// </summary>
        public static DeviceTypeInfo[] GetAllDeviceTypes()
        {
            var types = Enum.GetValues(typeof(DeviceType));
            var result = new DeviceTypeInfo[types.Length];
            
            for (int i = 0; i < types.Length; i++)
            {
                var type = (DeviceType)types.GetValue(i)!;
                result[i] = new DeviceTypeInfo
                {
                    Code = (int)type,
                    Name = type.GetDescription(),
                    EnumValue = type
                };
            }
            
            return result;
        }
    }

    /// <summary>
    /// 设备类型信息
    /// </summary>
    public class DeviceTypeInfo
    {
        /// <summary>
        /// 类型编码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 枚举值
        /// </summary>
        public DeviceType EnumValue { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

