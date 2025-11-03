# 边坡雷达监测系统

## 🚀 快速启动

### 开发环境启动

```batch
# Windows 系统 - 开发模式
.\start.ps1

# 跳过前端构建（前端已构建）
.\start.ps1 -SkipBuild

# 开发模式（支持热重载）
.\start.ps1 -DevMode
```

### 生产环境启动

```batch
# 1. 编译和部署（在项目根目录）
.\部署到Deploy.bat

# 2. 启动系统（在Deploy目录）
cd Deploy
启动系统.bat
```

## 🌐 访问地址

启动成功后，访问以下地址：

- **前端界面**: http://localhost:6098
- **API 服务**: http://localhost:8099
- **API 文档**: http://localhost:8099/swagger
- **EMQX 管理界面**: http://localhost:18083 (admin/public)

## 👤 默认账户

```
用户名: admin
密码:   admin123
```

## ⚙️ 环境要求

### 必需

- ✅ **.NET 8.0 SDK**  
  下载: https://dotnet.microsoft.com/download

### 可选（仅前端开发）

- ⭕ **Node.js 18+**  
  下载: https://nodejs.org

> 💡 如果不需要修改前端代码，可以跳过 Node.js 安装，使用预构建的前端文件

## 📁 项目结构

```
dotnetradar/
├── 部署到Deploy.bat              # 完整部署脚本
├── build-frontend.bat             # 前端构建脚本
├── start.ps1                      # 开发环境启动脚本
├── RadarSystem.WebAPI/            # 后端 Web API 项目
│   ├── Controllers/               # API 控制器
│   ├── Services/                  # 业务服务
│   └── Program.cs                 # 应用程序入口
├── RadarSystem.Core/              # 核心业务逻辑
├── RadarSystem.Data/              # 数据访问层
├── RadarSystem.Communication/    # 通信模块（雷达数据接收）
├── RadarSystem.Alarm/             # 告警模块
├── RadarSystem.ImageAnalysis/     # 图像分析模块
├── RadarSystem.Radar/             # 雷达处理模块
├── RadarContrl/                   # 前端项目源码
│   └── dist/                      # 前端构建产物
└── Deploy/                        # 生产部署目录
    ├── RadarSystem.WebAPI.dll    # 后端程序
    ├── RadarSystem.WebAPI.exe    # 后端启动器
    ├── conf/                      # 配置文件
    │   └── appsettings.json
    ├── db/                        # 数据库文件
    │   └── radar.db
    ├── Data/                      # 雷达数据存储
    ├── logs/                      # 日志文件
    ├── wwwroot/                   # 前端文件
    ├── emqx/                      # EMQX MQTT Broker
    ├── 启动系统.bat               # 启动脚本
    └── 停止系统.bat               # 停止脚本
```

## 📚 参考代码规定

本项目是从 Java 项目升级到 .NET Core，以下为参考代码位置：

### 1. 圆弧雷达数据接收参考

**目录**: `C:\kotradar2025\3RadarArcsarParse`

**说明**:
- 用于圆弧雷达数据接收部分程序参数参考
- Java 项目，包含圆弧雷达数据解析逻辑
- 参考该项目的协议解析和数据接收参数

### 2. 前端代码参考

**目录**: `C:\kotradar2025\RadarContrl`

**说明**:
- 前端代码参考（Vue 3 项目）
- 配置文件保存在 JSON 中的实现参考
- UI 组件和交互逻辑参考

### 3. 完整 Java 项目参考

**目录**: `C:\kotradar2025\kotjavrradar`

**说明**:
- 整个项目的 Java 参考代码（除上述 2 个外的所有业务逻辑）
- 包含完整的业务模块：
  - 设备管理
  - 数据处理
  - 告警规则
  - 报告生成
  - API 接口设计
  - 数据库结构

### 4. 升级后部署项目

**目录**: `C:\kotradar2025\dotnetradar\Deploy`

**说明**:
- 升级后的 .NET Core 项目部署目录
- 包含所有运行时文件、配置、数据库、前端静态文件
- 生产环境使用此目录部署和运行

## 🔧 编译和部署

### 完整部署流程

```batch
# 1. 构建前端并部署
.\build-frontend.bat

# 2. 完整部署（构建前端 + 后端 + 发布到Deploy）
.\部署到Deploy.bat
```

### 手动部署步骤

1. **构建前端**
   ```batch
   cd RadarContrl
   npm install
   npm run build
   ```

2. **构建后端**
   ```batch
   cd RadarSystem.WebAPI
   dotnet restore
   dotnet build --configuration Release
   ```

3. **发布后端**
   ```batch
   dotnet publish --configuration Release --output ..\Deploy
   ```

4. **复制配置和数据库**
   - 复制 `RadarSystem.WebAPI\appsettings.json` → `Deploy\conf\appsettings.json`
   - 复制 `RadarSystem.WebAPI\Data\*.db` → `Deploy\db\`
   - 复制前端构建产物到 `Deploy\wwwroot\`

## 🔧 故障排除

### 1. .NET SDK 未安装

```
❌ 错误: 未找到 .NET SDK
```

**解决方案**: 安装 [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

---

### 2. 前端构建失败

```
❌ 前端构建失败
```

**解决方案**:
- 确保已安装 Node.js 18+
- 删除 `node_modules` 目录后重试
- 或使用预构建文件

---

### 3. 端口被占用

```
❌ 端口 8099 已被占用
```

**解决方案**:
- 关闭占用端口的程序
- 或修改 `Deploy\conf\appsettings.json` 中的端口配置

---

### 4. 数据库错误

```
❌ 无法访问数据库
```

**解决方案**:
- 检查 `Deploy\db\radar.db` 文件权限
- 删除数据库文件，让系统自动重建

---

### 5. MQTT 连接失败

```
❌ MQTT 连接失败
```

**解决方案**:
- 确保 EMQX 已启动（`Deploy\emqx\bin\emqx.cmd start`）
- 检查 EMQX 配置（`Deploy\emqx\etc\emqx.conf`）
- 确保端口 1883 和 8083 未被占用

---

### 6. 查看详细错误

启动失败时：
- 查看命令行窗口的错误信息
- 检查 `Deploy\logs\` 目录下的日志文件

## 🛑 停止系统

### 开发环境

在命令行窗口中按 `Ctrl + C`

### 生产环境

```batch
cd Deploy
停止系统.bat
```

或手动停止：
```batch
# 停止 .NET 进程
taskkill /F /IM RadarSystem.WebAPI.exe

# 停止 EMQX
cd Deploy\emqx\bin
emqx.cmd stop
```

## 📊 数据目录结构

雷达数据按照以下结构保存：

```
Deploy\Data\
└── {ProjectId}_{DeviceId}_{SlaveId}\
    └── {yyyyMMdd}\
        ├── X{uuid}.dat  (变形数据)
        ├── F{uuid}.dat  (后向散射数据)
        └── Z{uuid}.dat  (置信度数据)
```

## 📝 版本信息

- **.NET 版本**: 8.0
- **前端框架**: Vue 3 + Vite
- **数据库**: SQLite / TDengine
- **地图引擎**: Cesium
- **MQTT Broker**: EMQX 5.3.2
- **网络框架**: DotNetty

---

**最后更新**: 2025-11-03
