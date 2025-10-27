# 多阶段构建 Dockerfile
FROM node:18-alpine AS frontend-build

WORKDIR /app/frontend

# 复制前端项目文件
COPY RadarSystem.WebAPI/ClientApp/package*.json ./
RUN npm install

COPY RadarSystem.WebAPI/ClientApp/ ./
RUN npm run build

# 后端构建
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /app

# 复制解决方案和项目文件
COPY *.sln ./
COPY RadarSystem.Core/*.csproj ./RadarSystem.Core/
COPY RadarSystem.Data/*.csproj ./RadarSystem.Data/
COPY RadarSystem.Communication/*.csproj ./RadarSystem.Communication/
COPY RadarSystem.Alarm/*.csproj ./RadarSystem.Alarm/
COPY RadarSystem.Radar/*.csproj ./RadarSystem.Radar/
COPY RadarSystem.ImageAnalysis/*.csproj ./RadarSystem.ImageAnalysis/
COPY RadarSystem.WebAPI/*.csproj ./RadarSystem.WebAPI/

# 还原依赖
RUN dotnet restore

# 复制所有源代码
COPY . ./

# 复制前端构建产物到 wwwroot
COPY --from=frontend-build /app/frontend/../wwwroot ./RadarSystem.WebAPI/wwwroot

# 发布应用
RUN dotnet publish RadarSystem.WebAPI/RadarSystem.WebAPI.csproj -c Release -o /app/publish

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# 安装必要的依赖（用于图像处理）
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=backend-build /app/publish .

# 创建数据和日志目录
RUN mkdir -p /app/Data /app/logs

# 暴露端口
EXPOSE 8099

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:8099
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RadarSystem.WebAPI.dll"]

