@echo off
chcp 65001 >nul
echo.
echo ========================================
echo   色条配置问题快速诊断
echo ========================================
echo.

echo 步骤1: 备份当前ColorConfig.vue
copy /Y "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue" "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue.backup"
echo ✅ 已备份

echo.
echo 步骤2: 使用简化版本替换
copy /Y "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig_Simple.vue" "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue"
echo ✅ 已替换为简化版本

echo.
echo 步骤3: 构建前端
cd RadarContrl
call npm run build

if errorlevel 1 (
    echo.
    echo ❌ 构建失败！恢复原文件...
    cd ..
    copy /Y "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue.backup" "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue"
    del "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue.backup"
    pause
    exit /b 1
)

cd ..

echo.
echo ========================================
echo   测试说明
echo ========================================
echo.
echo ✅ 简化版本已部署
echo.
echo 请执行以下步骤：
echo 1. 刷新浏览器（Ctrl + F5）
echo 2. 点击"工具" → "色条配置"
echo 3. 查看是否显示绿色测试框
echo.
echo 如果显示：说明组件加载正常，问题出在完整版代码
echo 如果不显示：说明组件导入/注册有问题
echo.
echo ========================================
echo.
echo 按任意键恢复原版本...
pause >nul

echo.
echo 恢复原版本...
copy /Y "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue.backup" "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue"
del "RadarContrl\src\components\ToolBar\Tool\ProjectConfig\ColorConfig.vue.backup"

echo.
echo ✅ 已恢复原版本
echo.
pause

