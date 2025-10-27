#!/bin/bash

# 前端构建脚本 (Linux/macOS)
echo "开始构建前端项目..."

# 检查 Node.js 是否安装
if ! command -v node &> /dev/null; then
    echo "错误: 未安装 Node.js，请先安装 Node.js"
    exit 1
fi

echo "Node.js 版本: $(node --version)"

# 进入前端项目目录
cd RadarSystem.WebAPI/ClientApp

# 检查 node_modules 是否存在
if [ ! -d "node_modules" ]; then
    echo "安装依赖包..."
    npm install
    if [ $? -ne 0 ]; then
        echo "依赖安装失败"
        cd ../..
        exit 1
    fi
fi

# 构建项目
echo "构建 Vue 项目..."
npm run build

if [ $? -eq 0 ]; then
    echo "前端构建成功！"
    echo "输出目录: RadarSystem.WebAPI/wwwroot"
else
    echo "前端构建失败"
    cd ../..
    exit 1
fi

# 返回根目录
cd ../..

echo "完成！"

