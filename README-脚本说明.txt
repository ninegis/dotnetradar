===============================================
   边坡雷达监测系统 - 脚本使用说明
===============================================

📜 可用脚本 (3个)
--------------

1. 启动系统.bat ⭐ 推荐
   功能: 一键编译、部署、启动
   前端: RadarSystem.WebAPI\ClientApp
   
2. 构建并部署前端.bat
   功能: 构建RadarContrl前端并部署
   前端: RadarContrl
   
3. build-frontend.bat (新)
   功能: 与"构建并部署前端.bat"相同
   前端: RadarContrl
   英文文件名，避免编码问题

📂 前端项目对比
--------------

RadarSystem.WebAPI\ClientApp\
  - TypeScript + Vue 3 + Vite
  - 输出到: ../wwwroot (直接)
  - 用于: 主要开发

RadarContrl\
  - JavaScript + Vue 3 + Vite  
  - 输出到: dist\ (需要复制到wwwroot)
  - 用于: 参考项目

🚀 快速启动
--------------

方式1: 使用ClientApp (推荐)
> 启动系统.bat

方式2: 使用RadarContrl
> build-frontend.bat
或
> 构建并部署前端.bat

🔄 构建流程
--------------

ClientApp流程:
  npm run build
    ↓
  直接输出到 ../wwwroot
    ↓
  dotnet build
    ↓
  dotnet run

RadarContrl流程:
  npm run build
    ↓
  输出到 dist\
    ↓
  xcopy 到 wwwroot\
    ↓
  dotnet build
    ↓
  dotnet run

⚙️ 脚本功能对比
--------------

启动系统.bat:
  ✅ 检查 .NET + Node.js
  ✅ 构建 ClientApp
  ✅ 还原后端依赖
  ✅ 编译后端
  ✅ 启动服务
  ✅ 打开浏览器

构建并部署前端.bat / build-frontend.bat:
  ✅ 检查 Node.js
  ✅ 构建 RadarContrl
  ✅ 部署到 wwwroot
  ✅ 编译后端
  ❌ 不启动服务

📝 使用建议
--------------

日常开发:
  使用 "启动系统.bat"
  
前端参考:
  使用 RadarContrl 作为参考
  使用 ClientApp 进行开发
  
构建部署:
  修改ClientApp → 启动系统.bat
  修改RadarContrl → build-frontend.bat

🔧 故障排除
--------------

问题: 中文文件名乱码
解决: 使用 build-frontend.bat (英文文件名)

问题: 前端构建失败
解决: 
  1. 检查 node_modules
  2. 删除并重新 npm install
  3. 查看错误信息

问题: 端口被占用
解决:
  netstat -ano | findstr "6098"
  taskkill /F /PID <PID>

🌐 访问地址
--------------
  前端: http://localhost:6098
  API:  http://localhost:8099
  文档: http://localhost:8099/swagger

  用户名: admin
  密码:   admin123

===============================================

