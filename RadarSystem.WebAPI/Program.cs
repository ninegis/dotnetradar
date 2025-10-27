using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Repositories;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Services;
using RadarSystem.ImageAnalysis.Services;
using RadarSystem.WebAPI.Middlewares;
using RadarSystem.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// 配置 Kestrel 监听端口（支持远程访问）
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6098); // 前端访问端口（支持远程访问）
    options.ListenAnyIP(8099); // API服务端口（支持远程访问）
});

// 配置 Serilog（从appsettings.json读取配置，避免重复输出）
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// 配置JSON序列化：使用camelCase命名（前端JavaScript标准）
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true; // 便于调试
    });

// 配置数据库
builder.Services.AddDbContext<RadarDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置 JWT 认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForRadarSystem2025";
var key = Encoding.ASCII.GetBytes(secretKey);
var issuer = jwtSettings["Issuer"] ?? "RadarSystem";
var audience = jwtSettings["Audience"] ?? "RadarUsers";

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 注册基础服务
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

// 注册Repository（使用已确认可用的）
builder.Services.AddScoped(typeof(AlarmRuleRepository));
builder.Services.AddScoped(typeof(AlarmRecordRepository));

// 注册Service（使用已确认可用的）
builder.Services.AddScoped<RadarSystem.Core.Interfaces.IAlarmRuleService, RadarSystem.Core.Services.AlarmRuleService>();
// 注意：ProjectService和DeviceService暂时不注册，因为它们的Repository实现有循环依赖问题
// 项目和设备相关的操作直接在Controller中使用DbContext
// builder.Services.AddScoped<RadarSystem.Core.Interfaces.IProjectService, ProjectService>();
// builder.Services.AddScoped<RadarSystem.Core.Interfaces.IDeviceService, DeviceService>();

// 注册其他Service（简化实现）
builder.Services.AddScoped<RadarSystem.WebAPI.Services.IAlarmRecordService, SimpleAlarmRecordService>();
builder.Services.AddScoped<RadarSystem.WebAPI.Services.IRadarImageService, SimpleRadarImageService>();
builder.Services.AddScoped<RadarSystem.WebAPI.Services.IDataManageService, SimpleDataManageService>();
builder.Services.AddScoped<RadarSystem.WebAPI.Services.ILayerService, SimpleLayerService>();
builder.Services.AddScoped<RadarSystem.WebAPI.Services.IGeoMarkService, SimpleGeoMarkService>();
builder.Services.AddScoped<RadarSystem.WebAPI.Services.IAlarmContactService, SimpleAlarmContactService>();
builder.Services.AddScoped<RadarSystem.WebAPI.Services.IRadarParamsService, SimpleRadarParamsService>();

// 注册图像分析服务
builder.Services.AddScoped<DeformationAnalyzer>();
builder.Services.AddScoped<ScatteringAnalyzer>();
builder.Services.AddScoped<VelocityAnalyzer>();
builder.Services.AddScoped<ImageTileGenerator>();

// 注册MQTT配置（暂时禁用）
// var mqttConfig = new RadarSystem.Communication.Services.MqttConfiguration
// {
//     BrokerHost = builder.Configuration.GetValue<string>("Mqtt:BrokerHost") ?? "localhost",
//     BrokerPort = builder.Configuration.GetValue<int>("Mqtt:BrokerPort", 1883),
//     ClientId = builder.Configuration.GetValue<string>("Mqtt:ClientId") ?? "RadarSystem",
//     Username = builder.Configuration.GetValue<string>("Mqtt:Username") ?? "",
//     Password = builder.Configuration.GetValue<string>("Mqtt:Password") ?? "",
//     KeepAliveInterval = builder.Configuration.GetValue<int>("Mqtt:KeepAliveInterval", 60),
//     ReconnectDelay = builder.Configuration.GetValue<int>("Mqtt:ReconnectDelay", 5000)
// };

// 注册MQTT服务（暂时禁用，需要MQTT Broker环境）
// builder.Services.AddSingleton<RadarSystem.Communication.Services.MqttService>(sp =>
// {
//     var logger = sp.GetRequiredService<ILogger<RadarSystem.Communication.Services.MqttService>>();
//     return new RadarSystem.Communication.Services.MqttService(logger, mqttConfig);
// });

// 注册所有设备Netty服务器（后台服务）（暂时禁用，需要MQTT和设备环境）
// builder.Services.AddHostedService<RadarSystem.Communication.Services.AllDeviceNettyServersHostedService>();

// 配置 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "边坡雷达监测系统 API",
        Version = "v1",
        Description = "Radar Monitoring System RESTful API",
        Contact = new OpenApiContact
        {
            Name = "雷达系统开发团队",
            Email = "dev@radar.com"
        }
    });

    // 配置 JWT认证
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 自定义Schema ID生成器 - 使用简洁的类名避免混乱
    c.CustomSchemaIds(type =>
    {
        // 递归函数处理嵌套泛型
        string GetFriendlyTypeName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;
            
            var genericTypeName = t.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
            
            // 递归处理所有泛型参数
            var genericArgs = t.GetGenericArguments()
                .Select(arg => GetFriendlyTypeName(arg));
            
            return $"{genericTypeName}_{string.Join("_", genericArgs)}";
        }
        
        return GetFriendlyTypeName(type);
    });

    // 启用XML注释
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // 使用枚举的字符串值而非数字
    c.UseInlineDefinitionsForEnums();
    
    // 忽略过时的成员
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();

    // 按Controller名称对API进行分组
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };
            
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Default" };
    });
    
    c.DocInclusionPredicate((name, api) => true);
});

var app = builder.Build();

// 配置静态文件支持（用于 Vue 前端）
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Radar API V1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(-1); // 隐藏Schemas部分
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator(null); // 禁用外部验证器
});

// ✅ 启用WebSocket支持
app.UseWebSockets();

// 使用中间件
app.UseMiddleware<WebSocketMiddleware>(); // ✅ WebSocket中间件
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 所有未匹配的路由重定向到 index.html（支持 Vue Router 历史模式）
app.MapFallbackToFile("index.html");

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    // 确保数据库目录存在
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, @"Data Source=(.+?)(?:;|$)");
        if (match.Success)
        {
            var dbPath = match.Groups[1].Value;
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
                Log.Information("已创建数据库目录: {Directory}", dbDirectory);
            }
        }
    }
    
    var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
    db.Database.EnsureCreated();
    Log.Information("数据库初始化完成");
    
    // 应用数据库迁移（添加新字段和新表）
    try
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        using (var command = connection.CreateCommand())
        {
            // ========== 为Devices表添加新字段 ==========
            command.CommandText = "PRAGMA table_info(Devices);";
            var reader = await command.ExecuteReaderAsync();
            var deviceColumns = new System.Collections.Generic.List<string>();
            while (await reader.ReadAsync())
            {
                deviceColumns.Add(reader.GetString(1));
            }
            reader.Close();
            
            if (!deviceColumns.Contains("Longitude"))
            {
                command.CommandText = "ALTER TABLE Devices ADD COLUMN Longitude REAL DEFAULT 0;";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: Devices.Longitude");
            }
            
            if (!deviceColumns.Contains("Latitude"))
            {
                command.CommandText = "ALTER TABLE Devices ADD COLUMN Latitude REAL DEFAULT 0;";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: Devices.Latitude");
            }
            
            if (!deviceColumns.Contains("Elevation"))
            {
                command.CommandText = "ALTER TABLE Devices ADD COLUMN Elevation REAL DEFAULT 0;";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: Devices.Elevation");
            }
            
            if (!deviceColumns.Contains("FactoryId"))
            {
                command.CommandText = "ALTER TABLE Devices ADD COLUMN FactoryId TEXT DEFAULT '';";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: Devices.FactoryId");
            }
            
            if (!deviceColumns.Contains("Orientation"))
            {
                command.CommandText = "ALTER TABLE Devices ADD COLUMN Orientation REAL DEFAULT 0;";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: Devices.Orientation");
            }

            // ========== 为users表添加ProjectId字段 ==========
            command.CommandText = "PRAGMA table_info(users);";
            reader = await command.ExecuteReaderAsync();
            var userColumns = new System.Collections.Generic.List<string>();
            while (await reader.ReadAsync())
            {
                userColumns.Add(reader.GetString(1));
            }
            reader.Close();
            
            if (!userColumns.Contains("ProjectId"))
            {
                command.CommandText = "ALTER TABLE users ADD COLUMN ProjectId TEXT DEFAULT NULL;";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: users.ProjectId");
            }

            // ========== 为AlarmHandleRecords表添加ProjectId字段 ==========
            command.CommandText = "PRAGMA table_info(AlarmHandleRecords);";
            reader = await command.ExecuteReaderAsync();
            var alarmHandleColumns = new System.Collections.Generic.List<string>();
            while (await reader.ReadAsync())
            {
                alarmHandleColumns.Add(reader.GetString(1));
            }
            reader.Close();
            
            if (!alarmHandleColumns.Contains("ProjectId"))
            {
                command.CommandText = "ALTER TABLE AlarmHandleRecords ADD COLUMN ProjectId TEXT DEFAULT '';";
                await command.ExecuteNonQueryAsync();
                Log.Information("已添加字段: AlarmHandleRecords.ProjectId");
            }

            // ========== 为layers表添加ProjectId字段 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='layers';";
            reader = await command.ExecuteReaderAsync();
            var hasLayersTable = reader.Read();
            reader.Close();
            
            if (hasLayersTable)
            {
                command.CommandText = "PRAGMA table_info(layers);";
                reader = await command.ExecuteReaderAsync();
                var layerColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync())
                {
                    layerColumns.Add(reader.GetString(1));
                }
                reader.Close();
                
                if (!layerColumns.Contains("project_id"))
                {
                    command.CommandText = "ALTER TABLE layers ADD COLUMN project_id TEXT DEFAULT NULL;";
                    await command.ExecuteNonQueryAsync();
                    Log.Information("已添加字段: layers.project_id");
                }
            }

            // ========== 为tilt_motor_configs表添加ProjectId字段 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='tilt_motor_configs';";
            reader = await command.ExecuteReaderAsync();
            var hasTiltMotorTable = reader.Read();
            reader.Close();
            
            if (hasTiltMotorTable)
            {
                command.CommandText = "PRAGMA table_info(tilt_motor_configs);";
                reader = await command.ExecuteReaderAsync();
                var tiltMotorColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync())
                {
                    tiltMotorColumns.Add(reader.GetString(1));
                }
                reader.Close();
                
                if (!tiltMotorColumns.Contains("project_id"))
                {
                    command.CommandText = "ALTER TABLE tilt_motor_configs ADD COLUMN project_id TEXT DEFAULT '';";
                    await command.ExecuteNonQueryAsync();
                    Log.Information("已添加字段: tilt_motor_configs.project_id");
                }
            }

            // ========== 创建command_records表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='command_records';";
            reader = await command.ExecuteReaderAsync();
            var hasCommandRecordsTable = reader.Read();
            reader.Close();
            
            if (!hasCommandRecordsTable)
            {
                command.CommandText = @"
                    CREATE TABLE command_records (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        device_id TEXT NOT NULL,
                        command_type TEXT NOT NULL,
                        command_content TEXT NOT NULL,
                        command_params_json TEXT,
                        operator TEXT,
                        status TEXT DEFAULT 'pending',
                        send_time TEXT,
                        response_time TEXT,
                        response_content TEXT,
                        error_message TEXT,
                        retry_count INTEGER DEFAULT 0,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT,
                        FOREIGN KEY (device_id) REFERENCES Devices(DeviceId) ON DELETE RESTRICT
                    );
                    CREATE INDEX idx_command_records_project_id ON command_records(project_id);
                    CREATE INDEX idx_command_records_device_id ON command_records(device_id);
                    CREATE INDEX idx_command_records_status ON command_records(status, create_time);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: command_records");
            }

            // ========== 创建algorithm_configs表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='algorithm_configs';";
            reader = await command.ExecuteReaderAsync();
            var hasAlgorithmConfigsTable = reader.Read();
            reader.Close();
            
            if (!hasAlgorithmConfigsTable)
            {
                command.CommandText = @"
                    CREATE TABLE algorithm_configs (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        device_id TEXT NOT NULL,
                        filter_type INTEGER DEFAULT 0,
                        alpha_filter INTEGER DEFAULT 0,
                        beta_filter INTEGER DEFAULT 0,
                        de_noise_thread INTEGER DEFAULT 0,
                        sens_coef INTEGER DEFAULT 0,
                        defo_image_dec TEXT DEFAULT '1',
                        scat_image_dec TEXT DEFAULT '1',
                        win_coheren INTEGER DEFAULT 0,
                        atm_pha_err_est_func_switch TEXT DEFAULT '0',
                        filter_width INTEGER DEFAULT 0,
                        monitor_mode TEXT DEFAULT '0',
                        ipv4 TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT,
                        FOREIGN KEY (device_id) REFERENCES Devices(DeviceId) ON DELETE RESTRICT,
                        UNIQUE (project_id, device_id)
                    );
                    CREATE INDEX idx_algorithm_configs_project_id ON algorithm_configs(project_id);
                    CREATE UNIQUE INDEX idx_algorithm_configs_project_device ON algorithm_configs(project_id, device_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: algorithm_configs");
            }

            // ========== 创建speed_indices表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='speed_indices';";
            reader = await command.ExecuteReaderAsync();
            var hasSpeedIndicesTable = reader.Read();
            reader.Close();
            
            if (!hasSpeedIndicesTable)
            {
                command.CommandText = @"
                    CREATE TABLE speed_indices (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        time_units TEXT DEFAULT '04',
                        enable_30min INTEGER DEFAULT 0,
                        enable_1hour INTEGER DEFAULT 0,
                        enable_1day INTEGER DEFAULT 1,
                        enable_3day INTEGER DEFAULT 0,
                        enable_1week INTEGER DEFAULT 0,
                        enable_1month INTEGER DEFAULT 0,
                        auto_gen_speed_image INTEGER DEFAULT 0,
                        speed_image_interval INTEGER DEFAULT 60,
                        auto_gen_acceleration_image INTEGER DEFAULT 0,
                        acceleration_image_interval INTEGER DEFAULT 120,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT
                    );
                    CREATE INDEX idx_speed_indices_project_id ON speed_indices(project_id);
                    CREATE UNIQUE INDEX idx_speed_indices_project ON speed_indices(project_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: speed_indices");
            }
            else
            {
                // ✅ 检查并添加新字段
                command.CommandText = "PRAGMA table_info(speed_indices);";
                reader = await command.ExecuteReaderAsync();
                var speedIndicesColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync()) { speedIndicesColumns.Add(reader.GetString(1)); }
                reader.Close();

                var columnsToAdd = new Dictionary<string, string>
                {
                    {"auto_gen_speed_image", "ALTER TABLE speed_indices ADD COLUMN auto_gen_speed_image INTEGER DEFAULT 0;"},
                    {"speed_image_interval", "ALTER TABLE speed_indices ADD COLUMN speed_image_interval INTEGER DEFAULT 60;"},
                    {"auto_gen_acceleration_image", "ALTER TABLE speed_indices ADD COLUMN auto_gen_acceleration_image INTEGER DEFAULT 0;"},
                    {"acceleration_image_interval", "ALTER TABLE speed_indices ADD COLUMN acceleration_image_interval INTEGER DEFAULT 120;"}
                };

                foreach (var col in columnsToAdd)
                {
                    if (!speedIndicesColumns.Contains(col.Key))
                    {
                        command.CommandText = col.Value;
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"已添加字段: speed_indices.{col.Key}");
                    }
                }
            }

            // ========== 创建colorbar_configs表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='colorbar_configs';";
            reader = await command.ExecuteReaderAsync();
            var hasColorbarConfigsTable = reader.Read();
            reader.Close();
            
            if (!hasColorbarConfigsTable)
            {
                command.CommandText = @"
                    CREATE TABLE colorbar_configs (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        mode TEXT NOT NULL,
                        min_value REAL DEFAULT -100,
                        max_value REAL DEFAULT 100,
                        hsl_h_start INTEGER DEFAULT 0,
                        hsl_h_end INTEGER DEFAULT 240,
                        filter_alpha REAL DEFAULT 0.8,
                        filter_min REAL DEFAULT -1000,
                        filter_max REAL DEFAULT 1000,
                        filter_enable INTEGER DEFAULT 0,
                        custom_ranges TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT,
                        UNIQUE (project_id, mode)
                    );
                    CREATE INDEX idx_colorbar_configs_project_id ON colorbar_configs(project_id);
                    CREATE UNIQUE INDEX idx_colorbar_configs_project_mode ON colorbar_configs(project_id, mode);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: colorbar_configs");
            }
            else
            {
                // ✅ 检查并添加新字段
                command.CommandText = "PRAGMA table_info(colorbar_configs);";
                reader = await command.ExecuteReaderAsync();
                var colorbarColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync()) { colorbarColumns.Add(reader.GetString(1)); }
                reader.Close();

                var colorbarColumnsToAdd = new Dictionary<string, string>
                {
                    {"custom_ranges", "ALTER TABLE colorbar_configs ADD COLUMN custom_ranges TEXT;"},
                    {"color_scheme_type", "ALTER TABLE colorbar_configs ADD COLUMN color_scheme_type INTEGER DEFAULT 0;"},
                    {"class_count", "ALTER TABLE colorbar_configs ADD COLUMN class_count INTEGER DEFAULT 5;"},
                    {"auto_adapt_range", "ALTER TABLE colorbar_configs ADD COLUMN auto_adapt_range INTEGER DEFAULT 0;"},
                    {"adapt_buffer_ratio", "ALTER TABLE colorbar_configs ADD COLUMN adapt_buffer_ratio REAL DEFAULT 0.1;"},
                    {"hsl_direction", "ALTER TABLE colorbar_configs ADD COLUMN hsl_direction INTEGER DEFAULT 0;"},
                    {"hsl_s", "ALTER TABLE colorbar_configs ADD COLUMN hsl_s REAL DEFAULT 1.0;"},
                    {"hsl_l", "ALTER TABLE colorbar_configs ADD COLUMN hsl_l REAL DEFAULT 0.5;"}
                };

                foreach (var col in colorbarColumnsToAdd)
                {
                    if (!colorbarColumns.Contains(col.Key))
                    {
                        command.CommandText = col.Value;
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"已添加字段: colorbar_configs.{col.Key}");
                    }
                }
            }
            
            // ========== 创建或更新 terrain_color_configs 表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='terrain_color_configs';";
            var terrainColorTableExists = await command.ExecuteScalarAsync() != null;
            
            if (!terrainColorTableExists)
            {
                command.CommandText = @"
                    CREATE TABLE terrain_color_configs (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL UNIQUE,
                        color_scheme_type INTEGER DEFAULT 0,
                        min_elevation REAL DEFAULT 0,
                        max_elevation REAL DEFAULT 1000,
                        hsl_h_start INTEGER DEFAULT 120,
                        hsl_h_end INTEGER DEFAULT 0,
                        hsl_s REAL DEFAULT 1.0,
                        hsl_l REAL DEFAULT 0.5,
                        class_count INTEGER DEFAULT 5,
                        auto_adapt_range INTEGER DEFAULT 1,
                        adapt_buffer_ratio REAL DEFAULT 0.1,
                        custom_ranges TEXT,
                        enable INTEGER DEFAULT 0,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT
                    );
                    CREATE INDEX idx_terrain_color_project_id ON terrain_color_configs(project_id);
                    CREATE UNIQUE INDEX idx_terrain_color_project ON terrain_color_configs(project_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: terrain_color_configs");
            }
            
            // ========== 创建或更新 data_storage_configs 表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='data_storage_configs';";
            var dataStorageTableExists = await command.ExecuteScalarAsync() != null;
            
            if (!dataStorageTableExists)
            {
                command.CommandText = @"
                    CREATE TABLE data_storage_configs (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        project_id TEXT NOT NULL UNIQUE,
                        auto_cleanup_enable INTEGER DEFAULT 0,
                        disk_threshold_percent INTEGER DEFAULT 80,
                        data_retention_days INTEGER DEFAULT 90,
                        delete_raw_data INTEGER DEFAULT 0,
                        delete_image_data INTEGER DEFAULT 0,
                        delete_analysis_data INTEGER DEFAULT 0,
                        image_quality INTEGER DEFAULT 85,
                        image_compression_enable INTEGER DEFAULT 1,
                        storage_path TEXT DEFAULT './Data',
                        backup_path TEXT,
                        auto_backup_enable INTEGER DEFAULT 0,
                        backup_interval_days INTEGER DEFAULT 7,
                        max_backup_count INTEGER DEFAULT 5,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE CASCADE
                    );
                    CREATE INDEX idx_data_storage_project_id ON data_storage_configs(project_id);
                    CREATE UNIQUE INDEX idx_data_storage_project ON data_storage_configs(project_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: data_storage_configs");
            }

            // ========== 创建或更新hidden_area_analysis_configs表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='hidden_area_analysis_configs';";
            reader = await command.ExecuteReaderAsync();
            var hasHiddenAreaTable = reader.Read();
            reader.Close();
            
            if (!hasHiddenAreaTable)
            {
                command.CommandText = @"
                    CREATE TABLE hidden_area_analysis_configs (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        threshold REAL DEFAULT 10.0,
                        area_threshold REAL DEFAULT 1.0,
                        analysis_dec INTEGER DEFAULT 1,
                        auto_analysis_flag INTEGER DEFAULT 0,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT
                    );
                    CREATE INDEX idx_hidden_area_configs_project_id ON hidden_area_analysis_configs(project_id);
                    CREATE UNIQUE INDEX idx_hidden_area_configs_project ON hidden_area_analysis_configs(project_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: hidden_area_analysis_configs");
            }

            // ========== 创建或更新alarm_rules表（添加新字段） ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='alarm_rules';";
            reader = await command.ExecuteReaderAsync();
            var hasAlarmRulesTable = reader.Read();
            reader.Close();
            
            if (!hasAlarmRulesTable)
            {
                command.CommandText = @"
                    CREATE TABLE alarm_rules (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        rule_name TEXT NOT NULL,
                        alarm_content TEXT,
                        enable INTEGER DEFAULT 1,
                        devices TEXT,
                        geo_mark_array TEXT,
                        data_source TEXT DEFAULT '10',
                        target_flag INTEGER DEFAULT 0,
                        enable_displacement INTEGER DEFAULT 1,
                        displacement_blue REAL,
                        displacement_yellow REAL,
                        displacement_orange REAL,
                        displacement_red REAL,
                        enable_speed INTEGER DEFAULT 0,
                        speed_time_unit TEXT,
                        speed_blue REAL,
                        speed_yellow REAL,
                        speed_orange REAL,
                        speed_red REAL,
                        enable_acceleration INTEGER DEFAULT 0,
                        acceleration_time_unit TEXT,
                        acceleration_blue REAL,
                        acceleration_yellow REAL,
                        acceleration_orange REAL,
                        acceleration_red REAL,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        is_deleted INTEGER DEFAULT 0,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT
                    );
                    CREATE INDEX idx_alarm_rules_project_id ON alarm_rules(project_id);
                    CREATE INDEX idx_alarm_rules_enable ON alarm_rules(enable, is_deleted);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: alarm_rules");
            }
            else
            {
                // 检查并添加新字段
                command.CommandText = "PRAGMA table_info(alarm_rules);";
                reader = await command.ExecuteReaderAsync();
                var alarmRuleColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync()) { alarmRuleColumns.Add(reader.GetString(1)); }
                reader.Close();

                var columnsToAdd = new Dictionary<string, string>
                {
                    {"devices", "ALTER TABLE alarm_rules ADD COLUMN devices TEXT;"},
                    {"geo_mark_array", "ALTER TABLE alarm_rules ADD COLUMN geo_mark_array TEXT;"},
                    {"data_source", "ALTER TABLE alarm_rules ADD COLUMN data_source TEXT DEFAULT '10';"},
                    {"target_flag", "ALTER TABLE alarm_rules ADD COLUMN target_flag INTEGER DEFAULT 0;"},
                    {"enable_displacement", "ALTER TABLE alarm_rules ADD COLUMN enable_displacement INTEGER DEFAULT 1;"},
                    {"displacement_blue", "ALTER TABLE alarm_rules ADD COLUMN displacement_blue REAL;"},
                    {"displacement_yellow", "ALTER TABLE alarm_rules ADD COLUMN displacement_yellow REAL;"},
                    {"displacement_orange", "ALTER TABLE alarm_rules ADD COLUMN displacement_orange REAL;"},
                    {"displacement_red", "ALTER TABLE alarm_rules ADD COLUMN displacement_red REAL;"},
                    {"enable_speed", "ALTER TABLE alarm_rules ADD COLUMN enable_speed INTEGER DEFAULT 0;"},
                    {"speed_time_unit", "ALTER TABLE alarm_rules ADD COLUMN speed_time_unit TEXT;"},
                    {"speed_blue", "ALTER TABLE alarm_rules ADD COLUMN speed_blue REAL;"},
                    {"speed_yellow", "ALTER TABLE alarm_rules ADD COLUMN speed_yellow REAL;"},
                    {"speed_orange", "ALTER TABLE alarm_rules ADD COLUMN speed_orange REAL;"},
                    {"speed_red", "ALTER TABLE alarm_rules ADD COLUMN speed_red REAL;"},
                    {"enable_acceleration", "ALTER TABLE alarm_rules ADD COLUMN enable_acceleration INTEGER DEFAULT 0;"},
                    {"acceleration_time_unit", "ALTER TABLE alarm_rules ADD COLUMN acceleration_time_unit TEXT;"},
                    {"acceleration_blue", "ALTER TABLE alarm_rules ADD COLUMN acceleration_blue REAL;"},
                    {"acceleration_yellow", "ALTER TABLE alarm_rules ADD COLUMN acceleration_yellow REAL;"},
                    {"acceleration_orange", "ALTER TABLE alarm_rules ADD COLUMN acceleration_orange REAL;"},
                    {"acceleration_red", "ALTER TABLE alarm_rules ADD COLUMN acceleration_red REAL;"}
                };

                foreach (var col in columnsToAdd)
                {
                    if (!alarmRuleColumns.Contains(col.Key))
                    {
                        command.CommandText = col.Value;
                        await command.ExecuteNonQueryAsync();
                        Log.Information("已添加字段: alarm_rules.{Column}", col.Key);
                    }
                }
            }

            // ========== 创建或更新sms_configs表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='sms_configs';";
            reader = await command.ExecuteReaderAsync();
            var hasSmsConfigsTable = reader.Read();
            reader.Close();
            
            if (!hasSmsConfigsTable)
            {
                command.CommandText = @"
                    CREATE TABLE sms_configs (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL,
                        enable INTEGER DEFAULT 0,
                        notify_channel TEXT DEFAULT '00',
                        access_key_id TEXT,
                        access_key_secret TEXT,
                        sign_name TEXT,
                        template_code TEXT,
                        provider TEXT,
                        api_key TEXT,
                        api_secret TEXT,
                        template_content TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT
                    );
                    CREATE INDEX idx_sms_configs_project_id ON sms_configs(project_id);
                    CREATE UNIQUE INDEX idx_sms_configs_project ON sms_configs(project_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: sms_configs");
            }
            else
            {
                // 检查并添加新字段
                command.CommandText = "PRAGMA table_info(sms_configs);";
                reader = await command.ExecuteReaderAsync();
                var smsConfigColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync()) { smsConfigColumns.Add(reader.GetString(1)); }
                reader.Close();

                var columnsToAdd = new Dictionary<string, string>
                {
                    {"enable", "ALTER TABLE sms_configs ADD COLUMN enable INTEGER DEFAULT 0;"},
                    {"notify_channel", "ALTER TABLE sms_configs ADD COLUMN notify_channel TEXT DEFAULT '00';"},
                    {"access_key_id", "ALTER TABLE sms_configs ADD COLUMN access_key_id TEXT;"},
                    {"access_key_secret", "ALTER TABLE sms_configs ADD COLUMN access_key_secret TEXT;"},
                    {"sign_name", "ALTER TABLE sms_configs ADD COLUMN sign_name TEXT;"},
                    {"template_code", "ALTER TABLE sms_configs ADD COLUMN template_code TEXT;"},
                    {"provider", "ALTER TABLE sms_configs ADD COLUMN provider TEXT;"},
                    {"api_key", "ALTER TABLE sms_configs ADD COLUMN api_key TEXT;"},
                    {"api_secret", "ALTER TABLE sms_configs ADD COLUMN api_secret TEXT;"},
                    {"template_content", "ALTER TABLE sms_configs ADD COLUMN template_content TEXT;"}
                };

                foreach (var col in columnsToAdd)
                {
                    if (!smsConfigColumns.Contains(col.Key))
                    {
                        command.CommandText = col.Value;
                        await command.ExecuteNonQueryAsync();
                        Log.Information("已添加字段: sms_configs.{Column}", col.Key);
                    }
                }
            }
            
            // ========== 创建或更新 image_analysis_configs 表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='image_analysis_configs';";
            var imageAnalysisTableExists = await command.ExecuteScalarAsync() != null;
            
            if (!imageAnalysisTableExists)
            {
                command.CommandText = @"
                    CREATE TABLE image_analysis_configs (
                        id TEXT PRIMARY KEY,
                        project_id TEXT NOT NULL UNIQUE,
                        standard_image_side_pixel INTEGER DEFAULT 16384,
                        compress_image_side_pixel INTEGER DEFAULT 1024,
                        matrix_tile_rng_num INTEGER DEFAULT 1203,
                        matrix_tile_ang_num INTEGER DEFAULT 61,
                        gen_defo INTEGER DEFAULT 0,
                        gen_scat INTEGER DEFAULT 1,
                        gen_speed INTEGER DEFAULT 0,
                        gen_acceleration INTEGER DEFAULT 0,
                        gen_image_type TEXT DEFAULT '01',
                        defo_interval INTEGER DEFAULT 60,
                        scat_interval INTEGER DEFAULT 60,
                        defo_number INTEGER DEFAULT 10,
                        scat_number INTEGER DEFAULT 10,
                        config_json TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE CASCADE
                    );
                    CREATE INDEX idx_image_analysis_project_id ON image_analysis_configs(project_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: image_analysis_configs");
            }
            else
            {
                // 检查并添加新字段
                command.CommandText = "PRAGMA table_info(image_analysis_configs);";
                reader = await command.ExecuteReaderAsync();
                var imageAnalysisColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync()) { imageAnalysisColumns.Add(reader.GetString(1)); }
                reader.Close();

                var columnsToAdd = new Dictionary<string, string>
                {
                    {"gen_image_type", "ALTER TABLE image_analysis_configs ADD COLUMN gen_image_type TEXT DEFAULT '01';"},
                    {"defo_interval", "ALTER TABLE image_analysis_configs ADD COLUMN defo_interval INTEGER DEFAULT 60;"},
                    {"scat_interval", "ALTER TABLE image_analysis_configs ADD COLUMN scat_interval INTEGER DEFAULT 60;"},
                    {"defo_number", "ALTER TABLE image_analysis_configs ADD COLUMN defo_number INTEGER DEFAULT 10;"},
                    {"scat_number", "ALTER TABLE image_analysis_configs ADD COLUMN scat_number INTEGER DEFAULT 10;"}
                };

                foreach (var col in columnsToAdd)
                {
                    if (!imageAnalysisColumns.Contains(col.Key))
                    {
                        command.CommandText = col.Value;
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"已添加字段: image_analysis_configs.{col.Key}");
                    }
                }
            }
            
            // ========== 创建 radar_params 表 ==========
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='radar_params';";
            var radarParamsTableExists = await command.ExecuteScalarAsync() != null;
            
            if (!radarParamsTableExists)
            {
                command.CommandText = @"
                    CREATE TABLE radar_params (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        project_id TEXT NOT NULL,
                        device_id TEXT NOT NULL,
                        img_angle_start REAL DEFAULT 0,
                        img_angle_end REAL DEFAULT 360,
                        rng_min REAL DEFAULT 0,
                        rng_max REAL DEFAULT 1000,
                        freq_band TEXT DEFAULT '0',
                        ante_beam_half REAL DEFAULT 60,
                        data_version TEXT DEFAULT '0',
                        model_select TEXT,
                        create_time TEXT NOT NULL,
                        update_time TEXT,
                        FOREIGN KEY (project_id) REFERENCES Projects(ProjectId) ON DELETE RESTRICT,
                        FOREIGN KEY (device_id) REFERENCES Devices(DeviceId) ON DELETE CASCADE
                    );
                    CREATE INDEX idx_radar_params_project_id ON radar_params(project_id);
                    CREATE INDEX idx_radar_params_device_id ON radar_params(device_id);
                    CREATE UNIQUE INDEX idx_radar_params_project_device ON radar_params(project_id, device_id);
                ";
                await command.ExecuteNonQueryAsync();
                Log.Information("已创建表: radar_params");
            }
            else
            {
                // 检查并添加新字段
                command.CommandText = "PRAGMA table_info(radar_params);";
                reader = await command.ExecuteReaderAsync();
                var radarParamsColumns = new System.Collections.Generic.List<string>();
                while (await reader.ReadAsync()) { radarParamsColumns.Add(reader.GetString(1)); }
                reader.Close();

                var columnsToAdd = new Dictionary<string, string>
                {
                    {"img_angle_start", "ALTER TABLE radar_params ADD COLUMN img_angle_start REAL DEFAULT 0;"},
                    {"img_angle_end", "ALTER TABLE radar_params ADD COLUMN img_angle_end REAL DEFAULT 360;"},
                    {"rng_min", "ALTER TABLE radar_params ADD COLUMN rng_min REAL DEFAULT 0;"},
                    {"rng_max", "ALTER TABLE radar_params ADD COLUMN rng_max REAL DEFAULT 1000;"},
                    {"freq_band", "ALTER TABLE radar_params ADD COLUMN freq_band TEXT DEFAULT '0';"},
                    {"ante_beam_half", "ALTER TABLE radar_params ADD COLUMN ante_beam_half REAL DEFAULT 60;"},
                    {"data_version", "ALTER TABLE radar_params ADD COLUMN data_version TEXT DEFAULT '0';"},
                    {"model_select", "ALTER TABLE radar_params ADD COLUMN model_select TEXT;"}
                };

                foreach (var col in columnsToAdd)
                {
                    if (!radarParamsColumns.Contains(col.Key))
                    {
                        command.CommandText = col.Value;
                        await command.ExecuteNonQueryAsync();
                        Log.Information($"已添加字段: radar_params.{col.Key}");
                    }
                }
            }
        }
        await connection.CloseAsync();
        Log.Information("数据库迁移完成");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "数据库迁移失败");
    }
    
    // 初始化默认数据
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userService.InitializeDefaultDataAsync();
}

var frontendUrl = "http://localhost:6098";
var apiUrl = "http://localhost:8099";
var swaggerUrl = "http://localhost:8099/swagger";
Log.Information("边坡雷达监测系统已启动");
Log.Information("前端访问地址: {FrontendUrl}", frontendUrl);
Log.Information("API服务地址: {ApiUrl}", apiUrl);
Log.Information("Swagger文档: {SwaggerUrl}", swaggerUrl);

// 自动打开前端系统
try
{
    var processStartInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = frontendUrl,
        UseShellExecute = true
    };
    System.Diagnostics.Process.Start(processStartInfo);
    Log.Information("已自动打开前端系统");
}
catch (Exception ex)
{
    Log.Warning(ex, "无法自动打开浏览器");
}

app.Run();

