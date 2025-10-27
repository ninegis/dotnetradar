using System;
using System.Drawing;
using RadarSystem.ImageAnalysis.Models;

namespace RadarSystem.ImageAnalysis.Utilities
{
    /// <summary>
    /// 颜色映射工具类
    /// </summary>
    public class ColorMap
    {
        private readonly ColorMapConfig _config;
        
        public ColorMap(ColorMapConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// 根据数值获取对应的颜色
        /// </summary>
        public Color GetColor(double value)
        {
            // 检查是否需要过滤
            if (_config.FilterEnable)
            {
                if (value >= _config.FilterMin && value <= _config.FilterMax)
                {
                    // 在过滤范围内，返回透明或指定透明度的颜色
                    return Color.FromArgb(_config.FilterAlpha, 0, 0, 0);
                }
            }
            
            // 归一化值到 [0, 1]
            double normalized = (_config.MaxValue - _config.MinValue) != 0
                ? (value - _config.MinValue) / (_config.MaxValue - _config.MinValue)
                : 0;
            
            // 限制在 [0, 1] 范围内
            normalized = Math.Max(0, Math.Min(1, normalized));
            
            // 计算HSL色相
            double hue = _config.HslHStart + (_config.HslHEnd - _config.HslHStart) * normalized;
            
            // HSL to RGB 转换
            return HslToRgb(hue, _config.HslS, _config.HslL);
        }
        
        /// <summary>
        /// HSL转RGB
        /// </summary>
        private Color HslToRgb(double h, double s, double l)
        {
            // 确保色相在 0-360 范围内
            h = h % 360;
            if (h < 0) h += 360;
            
            // 计算中间值
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;
            
            double r = 0, g = 0, b = 0;
            
            if (h >= 0 && h < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (h >= 60 && h < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (h >= 120 && h < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (h >= 180 && h < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (h >= 240 && h < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (h >= 300 && h < 360)
            {
                r = c; g = 0; b = x;
            }
            
            // 转换为 0-255 范围
            int rInt = (int)Math.Round((r + m) * 255);
            int gInt = (int)Math.Round((g + m) * 255);
            int bInt = (int)Math.Round((b + m) * 255);
            
            // 确保在有效范围内
            rInt = Math.Max(0, Math.Min(255, rInt));
            gInt = Math.Max(0, Math.Min(255, gInt));
            bInt = Math.Max(0, Math.Min(255, bInt));
            
            return Color.FromArgb(rInt, gInt, bInt);
        }
        
        /// <summary>
        /// 创建默认的形变颜色映射
        /// </summary>
        public static ColorMap CreateDeformationColorMap(double minValue = -50, double maxValue = 50)
        {
            return new ColorMap(new ColorMapConfig
            {
                MinValue = minValue,
                MaxValue = maxValue,
                HslHStart = 240,  // 蓝色
                HslHEnd = 0,      // 红色
                HslS = 1.0,
                HslL = 0.5,
                FilterEnable = false
            });
        }
        
        /// <summary>
        /// 创建默认的散射颜色映射（灰度）
        /// </summary>
        public static ColorMap CreateScatteringColorMap(double minValue = 0, double maxValue = 1)
        {
            return new ColorMap(new ColorMapConfig
            {
                MinValue = minValue,
                MaxValue = maxValue,
                HslHStart = 0,
                HslHEnd = 0,
                HslS = 0,  // 无饱和度 = 灰度
                HslL = 0.5,
                FilterEnable = false
            });
        }
        
        /// <summary>
        /// 创建默认的速度颜色映射
        /// </summary>
        public static ColorMap CreateVelocityColorMap(double minValue = -10, double maxValue = 10)
        {
            return new ColorMap(new ColorMapConfig
            {
                MinValue = minValue,
                MaxValue = maxValue,
                HslHStart = 120,  // 绿色
                HslHEnd = 0,      // 红色
                HslS = 1.0,
                HslL = 0.5,
                FilterEnable = false
            });
        }
    }
}

