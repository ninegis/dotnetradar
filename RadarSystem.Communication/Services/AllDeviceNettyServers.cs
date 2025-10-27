using System;
using System.Collections.Generic;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace RadarSystem.Communication.Services
{
    // ==================== 雷达设备系列 ====================

    /// <summary>
    /// MIMO 雷达 Netty 服务器
    /// 对应 Java: MIMORadarTCPNettyServer
    /// 默认端口: 1031
    /// </summary>
    public class MimoRadarNettyServer : DeviceNettyServerBase
    {
        public MimoRadarNettyServer(ILogger<MimoRadarNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "MIMO雷达";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// MIMO Lite 雷达 Netty 服务器
    /// 对应 Java: MIMOLiteRadarTCPNettyServer
    /// 默认端口: 1032
    /// </summary>
    public class MimoLiteRadarNettyServer : DeviceNettyServerBase
    {
        public MimoLiteRadarNettyServer(ILogger<MimoLiteRadarNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "MIMO Lite雷达";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// MIMO 雷达（通用）Netty 服务器
    /// 对应 Java: MIMOTCPNettyServer
    /// 默认端口: 1033
    /// </summary>
    public class MimoNettyServer : DeviceNettyServerBase
    {
        public MimoNettyServer(ILogger<MimoNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "MIMO雷达通用";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 建筑物 2D 雷达 Netty 服务器
    /// 对应 Java: Building2DRadarTCPNettyServer
    /// 默认端口: 1034
    /// </summary>
    public class Building2DRadarNettyServer : DeviceNettyServerBase
    {
        public Building2DRadarNettyServer(ILogger<Building2DRadarNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "建筑物2D雷达";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 建筑物雷达 Netty 服务器
    /// 对应 Java: BUILDTcpNettyServer
    /// 默认端口: 1035
    /// </summary>
    public class BuildingRadarNettyServer : DeviceNettyServerBase
    {
        public BuildingRadarNettyServer(ILogger<BuildingRadarNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "建筑物雷达";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 交通雷达 Netty 服务器
    /// 对应 Java: TrafficRadarTCPNettyServer
    /// 默认端口: 1036
    /// </summary>
    public class TrafficRadarNettyServer : DeviceNettyServerBase
    {
        public TrafficRadarNettyServer(ILogger<TrafficRadarNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "交通雷达";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    // ==================== 传感器设备系列 ====================

    /// <summary>
    /// GPS 设备 Netty 服务器
    /// 对应 Java: GPSTCPNettyServer
    /// 默认端口: 1040
    /// </summary>
    public class GpsNettyServer : DeviceNettyServerBase
    {
        public GpsNettyServer(ILogger<GpsNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "GPS设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// GPS V1 设备 Netty 服务器
    /// 对应 Java: GPSV1TCPNettyServer
    /// 默认端口: 1041
    /// </summary>
    public class GpsV1NettyServer : DeviceNettyServerBase
    {
        public GpsV1NettyServer(ILogger<GpsV1NettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "GPS V1设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 测斜计 Netty 服务器
    /// 对应 Java: InclinometerNettyServer
    /// 默认端口: 1042
    /// </summary>
    public class InclinometerNettyServer : DeviceNettyServerBase
    {
        public InclinometerNettyServer(ILogger<InclinometerNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "测斜计";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 倾斜仪 Netty 服务器
    /// 对应 Java: QXZTcpNettyServer
    /// 默认端口: 1043
    /// </summary>
    public class QxzNettyServer : DeviceNettyServerBase
    {
        public QxzNettyServer(ILogger<QxzNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "倾斜仪";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// CM 设备 Netty 服务器
    /// 对应 Java: CMTcpNettyServer
    /// 默认端口: 1044
    /// </summary>
    public class CmNettyServer : DeviceNettyServerBase
    {
        public CmNettyServer(ILogger<CmNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "CM设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 方向传感器 Netty 服务器
    /// 对应 Java: OrientationNettyServer
    /// 默认端口: 1045
    /// </summary>
    public class OrientationNettyServer : DeviceNettyServerBase
    {
        public OrientationNettyServer(ILogger<OrientationNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "方向传感器";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    // ==================== 控制设备系列 ====================

    /// <summary>
    /// 电机控制 Netty 服务器
    /// 对应 Java: MotoTCPNettyServer
    /// 默认端口: 1050
    /// </summary>
    public class MotorNettyServer : DeviceNettyServerBase
    {
        public MotorNettyServer(ILogger<MotorNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "电机控制";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// B 型电机 Netty 服务器
    /// 对应 Java: BMotoTCPNettyServer
    /// 默认端口: 1051
    /// </summary>
    public class BMotorNettyServer : DeviceNettyServerBase
    {
        public BMotorNettyServer(ILogger<BMotorNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "B型电机";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 俯仰电机 Netty 服务器
    /// 对应 Java: MotoPitchNettyServer
    /// 默认端口: 1052
    /// </summary>
    public class MotorPitchNettyServer : DeviceNettyServerBase
    {
        public MotorPitchNettyServer(ILogger<MotorPitchNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "俯仰电机";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 激光设备 Netty 服务器
    /// 对应 Java: LaserTCPNettyServer
    /// 默认端口: 1053
    /// </summary>
    public class LaserNettyServer : DeviceNettyServerBase
    {
        public LaserNettyServer(ILogger<LaserNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "激光设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 北纬设备 Netty 服务器
    /// 对应 Java: BWTCPNettyServer
    /// 默认端口: 1054
    /// </summary>
    public class BwNettyServer : DeviceNettyServerBase
    {
        public BwNettyServer(ILogger<BwNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "北纬设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 北纬 V1 设备 Netty 服务器
    /// 对应 Java: BWV1TCPNettyServer
    /// 默认端口: 1055
    /// </summary>
    public class BwV1NettyServer : DeviceNettyServerBase
    {
        public BwV1NettyServer(ILogger<BwV1NettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "北纬V1设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    // ==================== 报警设备系列 ====================

    /// <summary>
    /// 报警设备 Netty 服务器
    /// 对应 Java: AlarmTCPNettyServer
    /// 默认端口: 1060
    /// </summary>
    public class AlarmNettyServer : DeviceNettyServerBase
    {
        public AlarmNettyServer(ILogger<AlarmNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "报警设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 报警设备（通用）Netty 服务器
    /// 对应 Java: AlarmDeviceNettyServer
    /// 默认端口: 1061
    /// </summary>
    public class AlarmDeviceNettyServer : DeviceNettyServerBase
    {
        public AlarmDeviceNettyServer(ILogger<AlarmDeviceNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "报警设备通用";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }

    /// <summary>
    /// 4G 报警设备 Netty 服务器
    /// 对应 Java: AlarmDevice4GNettyServer
    /// 默认端口: 1062
    /// </summary>
    public class AlarmDevice4GNettyServer : DeviceNettyServerBase
    {
        public AlarmDevice4GNettyServer(ILogger<AlarmDevice4GNettyServer> logger, DeviceNettyConfiguration config, MqttService mqttService)
            : base(logger, config, mqttService) { }

        protected override string DeviceTypeName => "4G报警设备";
        protected override IChannelHandler CreateDecoder() => new GenericDeviceDecoder();
        protected override IChannelHandler CreateHandler() => new GenericDeviceHandler(this, _logger);
    }
}

