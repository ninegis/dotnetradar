namespace RadarSystem.Core.Constants
{
    /// <summary>
    /// SAR数据类型常量 - 完整对应Java SarFileData中的所有DATA_TYPE常量
    /// </summary>
    public static class SarDataType
    {
        /// <summary>
        /// 形变数据类型
        /// </summary>
        public const string DATA_TYPE_DEFO = "00";

        /// <summary>
        /// 散射数据类型
        /// </summary>
        public const string DATA_TYPE_SCA = "01";

        /// <summary>
        /// 置信度数据类型
        /// </summary>
        public const string DATA_TYPE_CONF = "02";

        /// <summary>
        /// 报警数据类型
        /// </summary>
        public const string DATA_TYPE_ALARM = "07";

        /// <summary>
        /// 速度形变数据类型
        /// </summary>
        public const string DATA_TYPE_SPEED_DEFO = "03";

        /// <summary>
        /// 速度断点数据类型
        /// </summary>
        public const string DATA_TYPE_SPEED_BREAKPOINT = "04";

        /// <summary>
        /// 速度反演数据类型
        /// </summary>
        public const string DATA_TYPE_SPEED_INVERSE = "08";

        /// <summary>
        /// 敏感数据类型
        /// </summary>
        public const string DATA_TYPE_SENSITIVE = "09";

        /// <summary>
        /// 速度手动数据类型
        /// </summary>
        public const string DATA_TYPE_SPEED_MANUAL = "05";

        /// <summary>
        /// 移动数据类型
        /// </summary>
        public const string DATA_TYPE_MOVE = "06";

        /// <summary>
        /// 监测点数据类型
        /// </summary>
        public const string DATA_TYPE_MONITOR_POINT = "63";

        /// <summary>
        /// 监测多边形数据类型
        /// </summary>
        public const string DATA_TYPE_MONITOR_POLYGON = "64";

        /// <summary>
        /// 断点形变数据类型
        /// </summary>
        public const string DATA_TYPE_BREAKPOINT_DEFO = "10";

        /// <summary>
        /// 差值形变数据类型
        /// </summary>
        public const string DATA_TYPE_DVALUE_DEFO = "11";

        /// <summary>
        /// 断点形变调整数据类型
        /// </summary>
        public const string DATA_TYPE_BREAKPOINT_DEFO_ADJUST = "12";

        /// <summary>
        /// 断点子数据类型
        /// </summary>
        public const string DATA_TYPE_BREAKPOINT_SUB = "13";

        /// <summary>
        /// 速度数据类型
        /// </summary>
        public const string DATA_TYPE_SPEED = "20";

        /// <summary>
        /// 加速度数据类型
        /// </summary>
        public const string DATA_TYPE_ACCELERATION = "30";

        /// <summary>
        /// 高度数据类型
        /// </summary>
        public const string DATA_TYPE_HEIGHT = "40";

        /// <summary>
        /// 建筑形变数据类型
        /// </summary>
        public const string DATA_TYPE_BUILD_DEFO = "50";

        /// <summary>
        /// 建筑散射数据类型
        /// </summary>
        public const string DATA_TYPE_BUILD_SCAT = "51";

        /// <summary>
        /// 建筑形变监测数据类型
        /// </summary>
        public const string DATA_TYPE_BUILD_DEFO_MONITOR = "52";

        /// <summary>
        /// MIMO形变数据类型
        /// </summary>
        public const string DATA_TYPE_MIMO_DEFO = "60";

        /// <summary>
        /// MIMO散射数据类型
        /// </summary>
        public const string DATA_TYPE_MIMO_SCAT = "61";

        /// <summary>
        /// MIMO置信度数据类型
        /// </summary>
        public const string DATA_TYPE_MIMO_CONF = "62";

        /// <summary>
        /// 2D建筑形变数据类型
        /// </summary>
        public const string DATA_TYPE_BUILD2D_DEFO = "70";

        /// <summary>
        /// 2D建筑散射数据类型
        /// </summary>
        public const string DATA_TYPE_BUILD2D_SCAT = "71";

        /// <summary>
        /// 2D建筑形变监测数据类型
        /// </summary>
        public const string DATA_TYPE_BUILD2D_DEFO_MONITOR = "72";

        /// <summary>
        /// 检查数据类型是否需要Snappy压缩
        /// </summary>
        public static bool RequiresCompression(string dataType)
        {
            return dataType == DATA_TYPE_DEFO ||
                   dataType == DATA_TYPE_SPEED ||
                   dataType == DATA_TYPE_ALARM ||
                   dataType == DATA_TYPE_ACCELERATION ||
                   dataType == DATA_TYPE_HEIGHT ||
                   dataType == DATA_TYPE_CONF ||
                   dataType == DATA_TYPE_SPEED_DEFO ||
                   dataType == DATA_TYPE_SPEED_BREAKPOINT ||
                   dataType == DATA_TYPE_SPEED_INVERSE ||
                   dataType == DATA_TYPE_SENSITIVE ||
                   dataType == DATA_TYPE_SPEED_MANUAL ||
                   dataType == DATA_TYPE_BREAKPOINT_DEFO ||
                   dataType == DATA_TYPE_DVALUE_DEFO ||
                   dataType == DATA_TYPE_MIMO_DEFO ||
                   dataType == DATA_TYPE_BUILD2D_DEFO;
        }

        /// <summary>
        /// 获取数据类型的描述
        /// </summary>
        public static string GetDescription(string dataType)
        {
            return dataType switch
            {
                DATA_TYPE_DEFO => "形变数据",
                DATA_TYPE_SCA => "散射数据",
                DATA_TYPE_CONF => "置信度数据",
                DATA_TYPE_ALARM => "报警数据",
                DATA_TYPE_SPEED_DEFO => "速度形变数据",
                DATA_TYPE_SPEED_BREAKPOINT => "速度断点数据",
                DATA_TYPE_SPEED_INVERSE => "速度反演数据",
                DATA_TYPE_SENSITIVE => "敏感数据",
                DATA_TYPE_SPEED_MANUAL => "速度手动数据",
                DATA_TYPE_MOVE => "移动数据",
                DATA_TYPE_MONITOR_POINT => "监测点数据",
                DATA_TYPE_MONITOR_POLYGON => "监测多边形数据",
                DATA_TYPE_BREAKPOINT_DEFO => "断点形变数据",
                DATA_TYPE_DVALUE_DEFO => "差值形变数据",
                DATA_TYPE_BREAKPOINT_DEFO_ADJUST => "断点形变调整数据",
                DATA_TYPE_BREAKPOINT_SUB => "断点子数据",
                DATA_TYPE_SPEED => "速度数据",
                DATA_TYPE_ACCELERATION => "加速度数据",
                DATA_TYPE_HEIGHT => "高度数据",
                DATA_TYPE_BUILD_DEFO => "建筑形变数据",
                DATA_TYPE_BUILD_SCAT => "建筑散射数据",
                DATA_TYPE_BUILD_DEFO_MONITOR => "建筑形变监测数据",
                DATA_TYPE_MIMO_DEFO => "MIMO形变数据",
                DATA_TYPE_MIMO_SCAT => "MIMO散射数据",
                DATA_TYPE_MIMO_CONF => "MIMO置信度数据",
                DATA_TYPE_BUILD2D_DEFO => "2D建筑形变数据",
                DATA_TYPE_BUILD2D_SCAT => "2D建筑散射数据",
                DATA_TYPE_BUILD2D_DEFO_MONITOR => "2D建筑形变监测数据",
                _ => "未知数据类型"
            };
        }
    }
}
