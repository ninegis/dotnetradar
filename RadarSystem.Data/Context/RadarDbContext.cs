using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Models;

namespace RadarSystem.Data.Context
{
    /// <summary>
    /// 雷达系统数据库上下文
    /// </summary>
    public class RadarDbContext : DbContext
    {
        public RadarDbContext(DbContextOptions<RadarDbContext> options) : base(options)
        {
        }

        // 原有表
        public DbSet<RadarDataEntity> RadarData { get; set; }
        public DbSet<AlarmRecordEntity> AlarmRecords { get; set; }
        public DbSet<AlarmHandleRecordEntity> AlarmHandleRecords { get; set; }
        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<DeviceEntity> Devices { get; set; }

        // 用户表
        public DbSet<UserEntity> Users { get; set; }

        // 新增配置表
        public DbSet<GeoMarkEntity> GeoMarks { get; set; }
        public DbSet<AlarmRuleEntity> AlarmRules { get; set; }
        public DbSet<ColorSettingEntity> ColorSettings { get; set; }
        public DbSet<PanelConfigEntity> PanelConfigs { get; set; }
        public DbSet<ImageMarkEntity> ImageMarks { get; set; }
        public DbSet<ImageAnalysisConfigEntity> ImageAnalysisConfigs { get; set; }

        // 告警管理表
        public DbSet<AlarmContactEntity> AlarmContacts { get; set; }
        public DbSet<SmsConfigEntity> SmsConfigs { get; set; }

        // 雷达图像表
        public DbSet<RadarImageEntity> RadarImages { get; set; }
        public DbSet<ImageGenerationTaskEntity> ImageGenerationTasks { get; set; }

        // 系统管理表
        public DbSet<LayerEntity> Layers { get; set; }
        public DbSet<SystemLogEntity> SystemLogs { get; set; }
        public DbSet<SystemConfigEntity> SystemConfigs { get; set; }
        public DbSet<DiskStorageConfigEntity> DiskStorageConfigs { get; set; }
        public DbSet<RadarParamConfigEntity> RadarParamConfigs { get; set; }

        // 新增配置表（替代JSON配置文件）
        public DbSet<ProjectConfigurationEntity> ProjectConfigurations { get; set; }
        public DbSet<ImageDiffAnalysisConfigEntity> ImageDiffAnalysisConfigs { get; set; }
        public DbSet<HiddenAreaAnalysisConfigEntity> HiddenAreaAnalysisConfigs { get; set; }
        public DbSet<TiltMotorConfigEntity> TiltMotorConfigs { get; set; }

        // 指令下发记录表
        public DbSet<CommandRecordEntity> CommandRecords { get; set; }

        // 算法配置表
        public DbSet<AlgorithmConfigEntity> AlgorithmConfigs { get; set; }

        // 速度指标配置表
        public DbSet<SpeedIndexEntity> SpeedIndices { get; set; }

        // 色条配置表
        public DbSet<ColorBarConfigEntity> ColorBarConfigs { get; set; }
        
        // 高程图颜色配置表
        public DbSet<TerrainColorConfigEntity> TerrainColorConfigs { get; set; }
        
        // 数据存储配置表
        public DbSet<DataStorageConfigEntity> DataStorageConfigs { get; set; }
        
        // 雷达参数配置（独立字段，不使用JSON）
        public DbSet<RadarParamEntity> RadarParams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置雷达数据表
            modelBuilder.Entity<RadarDataEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.DeviceId, e.Timestamp });
                entity.HasIndex(e => new { e.ProjectId, e.Timestamp });
                entity.HasIndex(e => e.Timestamp);
            });

            // 配置报警记录表
            modelBuilder.Entity<AlarmRecordEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ProjectId, e.Timestamp });
                entity.HasIndex(e => new { e.RuleId, e.Timestamp });
                entity.HasIndex(e => e.Timestamp);
            });

            // 配置报警处理记录表
            modelBuilder.Entity<AlarmHandleRecordEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.HandleId);
            });

            // 配置项目表
            modelBuilder.Entity<ProjectEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(e => e.ProjectId); // 配置 ProjectId 为备用键
                entity.HasIndex(e => e.ProjectName);
                entity.HasIndex(e => e.Status);
            });

            // 配置设备表
            modelBuilder.Entity<DeviceEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DeviceId).IsUnique();
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId });
            });

            // 配置用户表
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => new { e.Username, e.IsDeleted });
                entity.HasIndex(e => e.Email);
            });

            // 配置地理标记表
            modelBuilder.Entity<GeoMarkEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.IsDeleted });
                entity.HasIndex(e => e.Name);
                
                // 配置与ProjectEntity的关系
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置报警规则表
            modelBuilder.Entity<AlarmRuleEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.Enable, e.IsDeleted });
                entity.HasIndex(e => e.RuleName);
                
                // 配置与ProjectEntity的关系：使用ProjectId而不是主键Id
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置颜色设置表
            modelBuilder.Entity<ColorSettingEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.SettingType });
                
                // 配置与ProjectEntity的关系
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置面板配置表
            modelBuilder.Entity<PanelConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.PanelType }).IsUnique();
                
                // 配置与ProjectEntity的关系
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置图像标记表
            modelBuilder.Entity<ImageMarkEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.IsDeleted });
                entity.HasIndex(e => e.ImageId);
                
                // 配置与ProjectEntity的关系
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置图像分析配置表
            modelBuilder.Entity<ImageAnalysisConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique();
                
                // 配置与ProjectEntity的关系
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置告警联系人表
            modelBuilder.Entity<AlarmContactEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.IsDeleted });
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置短信配置表
            modelBuilder.Entity<SmsConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique();
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置雷达图像表
            modelBuilder.Entity<RadarImageEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.DeviceId);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId, e.CaptureTime });
                entity.HasIndex(e => new { e.Status, e.IsDeleted });
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置图像生成任务表
            modelBuilder.Entity<ImageGenerationTaskEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.Status, e.CreateTime });
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置图层表
            modelBuilder.Entity<LayerEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Oid).IsUnique();
                entity.HasIndex(e => e.OrgId);
                entity.HasIndex(e => new { e.OrgId, e.IsDeleted });
                entity.HasIndex(e => e.ProjectId);
                entity.Property(e => e.ProjectId).IsRequired(false); // ProjectId可为空
                
                // 如果ProjectId不为空，建立外键关系
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull); // 项目删除时，将图层的ProjectId设为NULL
            });

            // 配置系统日志表
            modelBuilder.Entity<SystemLogEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreateTime);
                entity.HasIndex(e => new { e.ProjectCode, e.CreateTime });
                entity.HasIndex(e => new { e.LogType, e.CreateTime });
            });

            // 配置系统配置表
            modelBuilder.Entity<SystemConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ConfigKey).IsUnique();
                entity.HasIndex(e => e.Category);
            });

            // 配置磁盘存储配置表
            modelBuilder.Entity<DiskStorageConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // 配置雷达参数配置表
            modelBuilder.Entity<RadarParamConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId, e.ParamType }).IsUnique();
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置项目完整配置表（替代JSON配置文件）
            modelBuilder.Entity<ProjectConfigurationEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique(); // 一个项目只有一个配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade); // 项目删除时级联删除配置
            });

            // 配置图像差分分析配置表
            modelBuilder.Entity<ImageDiffAnalysisConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId }).IsUnique(); // 每个设备一个配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置隐患区域分析配置表
            modelBuilder.Entity<HiddenAreaAnalysisConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique(); // 每个项目一个配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置俯仰电机配置表
            modelBuilder.Entity<TiltMotorConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DeviceId).IsUnique(); // 每个设备一个电机配置
                entity.HasIndex(e => e.ProjectId);
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置指令下发记录表
            modelBuilder.Entity<CommandRecordEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.DeviceId);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId, e.CreateTime });
                entity.HasIndex(e => new { e.Status, e.CreateTime });
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置算法配置表
            modelBuilder.Entity<AlgorithmConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId }).IsUnique(); // 每个设备一个算法配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置速度指标表
            modelBuilder.Entity<SpeedIndexEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique(); // 每个项目一个配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置色条配置表
            modelBuilder.Entity<ColorBarConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => new { e.ProjectId, e.Mode }).IsUnique(); // 每个项目每种模式一个色条配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 高程图颜色配置
            modelBuilder.Entity<TerrainColorConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique(); // 每个项目一个高程图配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 数据存储配置
            modelBuilder.Entity<DataStorageConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId).IsUnique(); // 每个项目一个存储配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 雷达参数配置
            modelBuilder.Entity<RadarParamEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.DeviceId);
                entity.HasIndex(e => new { e.ProjectId, e.DeviceId }).IsUnique(); // 每个设备一个雷达参数配置
                
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .HasPrincipalKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Device)
                    .WithMany()
                    .HasForeignKey(e => e.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
