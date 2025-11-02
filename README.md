# 边坡雷达监测系统

## 🚀 快速启动

### Windows 系统

```batch
# 双击运行
启动系统.bat
```

或使用 PowerShell：

```powershell
# 标准启动
.\start.ps1

# 跳过前端构建（前端已构建）
.\start.ps1 -SkipBuild

# 开发模式（支持热重载）
.\start.ps1 -DevMode
```

## 📋 功能说明

`启动系统.bat` 脚本会自动完成以下步骤：

1. ✅ **检查 .NET 环境** - 验证 .NET 8.0 SDK
2. ✅ **检查 Node.js 环境** - 验证 Node.js（可选）
3. ✅ **构建前端项目** - 自动检测并构建前端
4. ✅ **还原后端依赖** - dotnet restore
5. ✅ **编译后端项目** - dotnet build (Release)
6. ✅ **启动系统服务** - 自动打开浏览器

## 🌐 访问地址

启动成功后，访问以下地址：

- **前端界面**: http://localhost:6098
- **API 服务**: http://localhost:8099
- **API 文档**: http://localhost:8099/swagger

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
├── 启动系统.bat              # 一键启动脚本（推荐）
├── start.ps1                  # PowerShell 启动脚本
├── 使用说明.txt               # 详细使用说明
├── RadarSystem.WebAPI/        # 后端 Web API 项目
│   ├── ClientApp/             # 前端项目（主要）
│   ├── ClientApp_OLD/         # 前端项目（备用）
│   ├── Controllers/           # API 控制器
│   ├── Data/                  # 数据库文件
│   ├── wwwroot/               # 前端构建输出
│   └── Program.cs             # 应用程序入口
├── RadarSystem.Core/          # 核心业务逻辑
├── RadarSystem.Data/          # 数据访问层
├── RadarSystem.Communication/ # 通信模块
├── RadarSystem.Alarm/         # 告警模块
├── RadarSystem.ImageAnalysis/ # 图像分析模块
├── RadarSystem.Radar/         # 雷达处理模块
└── RadarContrl/               # 预构建的前端文件
    └── dist/                  # 前端构建产物
```

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
- 确保已安装 Node.js
- 删除 `node_modules` 目录后重试
- 或使用预构建文件（脚本会自动检测）

---

### 3. 端口被占用

```
❌ 端口 8099 已被占用
```

**解决方案**:
- 关闭占用端口的程序
- 或修改 `RadarSystem.WebAPI/Program.cs` 中的端口配置

---

### 4. 数据库错误

```
❌ 无法访问数据库
```

**解决方案**:
- 检查 `RadarSystem.WebAPI/Data/radar.db` 文件权限
- 删除数据库文件，让系统自动重建

---

### 5. 查看详细错误

启动失败时：
- 查看命令行窗口的错误信息
- 检查 `RadarSystem.WebAPI/logs/` 目录下的日志文件

---

## 🛑 停止系统

在命令行窗口中按 `Ctrl + C`

## 📚 开发文档

详细的开发规范和指南：

- [系统开发规则总览.md](./系统开发规则总览.md)
- [后端开发规则_JavaReference.md](./后端开发规则_JavaReference.md)
- [前端开发规则_RadarContrl.md](./前端开发规则_RadarContrl.md)

## 🐳 Docker 部署

```bash
# 构建镜像
docker build -t radar-system .

# 运行容器
docker run -d -p 6098:6098 -p 8099:8099 \
  -v $(pwd)/data:/app/Data \
  -v $(pwd)/logs:/app/logs \
  radar-system
```

## 📝 版本信息

- **.NET 版本**: 8.0
- **前端框架**: Vue 3 + Vite
- **数据库**: SQLite / TDengine
- **地图引擎**: Cesium

---

**最后更新**: 2025-11-02

