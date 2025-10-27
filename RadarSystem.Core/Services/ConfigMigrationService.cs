using Microsoft.Extensions.Logging;
using RadarSystem.Core.Interfaces;
using RadarSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadarSystem.Core.Services
{
    /// <summary>
    /// 配置迁移服务 - 从JSON文件迁移到SQLite数据库
    /// </summary>
    public class ConfigMigrationService
    {
        private readonly IGeoService _geoService;
        private readonly IAlarmRuleService _alarmRuleService;
        private readonly IColorSettingService _colorSettingService;
        private readonly IPanelConfigService _panelConfigService;
        private readonly IImageMarkService _imageMarkService;
        private readonly IImageAnalysisConfigService _imageAnalysisConfigService;
        private readonly ILogger<ConfigMigrationService> _logger;

        public ConfigMigrationService(
            IGeoService geoService,
            IAlarmRuleService alarmRuleService,
            IColorSettingService colorSettingService,
            IPanelConfigService panelConfigService,
            IImageMarkService imageMarkService,
            IImageAnalysisConfigService imageAnalysisConfigService,
            ILogger<ConfigMigrationService> logger)
        {
            _geoService = geoService;
            _alarmRuleService = alarmRuleService;
            _colorSettingService = colorSettingService;
            _panelConfigService = panelConfigService;
            _imageMarkService = imageMarkService;
            _imageAnalysisConfigService = imageAnalysisConfigService;
            _logger = logger;
        }

        /// <summary>
        /// 迁移结果统计
        /// </summary>
        public class MigrationResult
        {
            public int TotalFiles { get; set; }
            public int SuccessCount { get; set; }
            public int FailureCount { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public Dictionary<string, int> TypeCounts { get; set; } = new Dictionary<string, int>();
        }

        /// <summary>
        /// 从目录迁移所有JSON配置文件
        /// </summary>
        public async Task<MigrationResult> MigrateFromDirectoryAsync(string directoryPath, string projectId)
        {
            var result = new MigrationResult();
            
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    throw new DirectoryNotFoundException($"目录不存在: {directoryPath}");
                }

                var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
                result.TotalFiles = jsonFiles.Length;

                _logger.LogInformation($"开始迁移配置文件，共 {result.TotalFiles} 个文件");

                foreach (var filePath in jsonFiles)
                {
                    try
                    {
                        await MigrateJsonFileAsync(filePath, projectId, result);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        var error = $"迁移文件失败 [{Path.GetFileName(filePath)}]: {ex.Message}";
                        result.Errors.Add(error);
                        _logger.LogError(ex, error);
                    }
                }

                _logger.LogInformation($"迁移完成: 成功 {result.SuccessCount}, 失败 {result.FailureCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置迁移过程出错");
                throw;
            }
        }

        /// <summary>
        /// 迁移单个JSON文件
        /// </summary>
        private async Task MigrateJsonFileAsync(string filePath, string projectId, MigrationResult result)
        {
            var fileName = Path.GetFileName(filePath);
            var fileContent = await File.ReadAllTextAsync(filePath);

            // 根据文件名或内容识别配置类型
            if (fileName.Contains("geomark", StringComparison.OrdinalIgnoreCase) || 
                fileName.Contains("地理标记", StringComparison.OrdinalIgnoreCase))
            {
                await MigrateGeoMarksAsync(fileContent, projectId, result);
            }
            else if (fileName.Contains("alarm", StringComparison.OrdinalIgnoreCase) || 
                     fileName.Contains("报警", StringComparison.OrdinalIgnoreCase))
            {
                await MigrateAlarmRulesAsync(fileContent, projectId, result);
            }
            else if (fileName.Contains("color", StringComparison.OrdinalIgnoreCase) || 
                     fileName.Contains("颜色", StringComparison.OrdinalIgnoreCase))
            {
                await MigrateColorSettingsAsync(fileContent, projectId, result);
            }
            else if (fileName.Contains("panel", StringComparison.OrdinalIgnoreCase) || 
                     fileName.Contains("面板", StringComparison.OrdinalIgnoreCase))
            {
                await MigratePanelConfigsAsync(fileContent, projectId, result);
            }
            else if (fileName.Contains("imagemark", StringComparison.OrdinalIgnoreCase) || 
                     fileName.Contains("图像标记", StringComparison.OrdinalIgnoreCase))
            {
                await MigrateImageMarksAsync(fileContent, projectId, result);
            }
            else if (fileName.Contains("imageanalysis", StringComparison.OrdinalIgnoreCase) || 
                     fileName.Contains("图像分析", StringComparison.OrdinalIgnoreCase))
            {
                await MigrateImageAnalysisConfigAsync(fileContent, projectId, result);
            }
            else
            {
                _logger.LogWarning($"未识别的配置文件类型: {fileName}");
            }
        }

        /// <summary>
        /// 迁移地理标记配置
        /// </summary>
        private async Task MigrateGeoMarksAsync(string jsonContent, string projectId, MigrationResult result)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var geoMarks = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent, options);

                if (geoMarks == null || geoMarks.Count == 0)
                    return;

                foreach (var item in geoMarks)
                {
                    var request = new CreateGeoMarkRequest
                    {
                        ProjectId = projectId,
                        Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? "未命名" : "未命名",
                        Type = item.TryGetProperty("type", out var type) ? type.GetString() ?? "Point" : "Point",
                        CoordinatesJson = item.TryGetProperty("coordinates", out var coords) ? coords.GetRawText() : null,
                        Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        Color = item.TryGetProperty("color", out var color) ? color.GetString() : "#FF0000",
                        Icon = item.TryGetProperty("icon", out var icon) ? icon.GetString() : null
                    };

                    await _geoService.CreateGeoMarkAsync(request);
                    IncrementTypeCount(result, "GeoMark");
                }

                _logger.LogInformation($"成功迁移 {geoMarks.Count} 个地理标记");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移地理标记失败");
                throw;
            }
        }

        /// <summary>
        /// 迁移报警规则配置
        /// </summary>
        private async Task MigrateAlarmRulesAsync(string jsonContent, string projectId, MigrationResult result)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var alarmRules = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent, options);

                if (alarmRules == null || alarmRules.Count == 0)
                    return;

                foreach (var item in alarmRules)
                {
                    var request = new CreateAlarmRuleRequest
                    {
                        ProjectId = projectId,
                        RuleName = item.TryGetProperty("ruleName", out var ruleName) ? ruleName.GetString() ?? "未命名规则" : "未命名规则",
                        RuleDescription = item.TryGetProperty("ruleDescription", out var desc) ? desc.GetString() : null,
                        AlarmContent = item.TryGetProperty("alarmContent", out var content) ? content.GetString() : null,
                        RuleOperator = item.TryGetProperty("alarmRule", out var rule) ? rule.GetString() ?? ">" : ">",
                        AlarmLevel = item.TryGetProperty("alarmLevel", out var level) ? level.GetInt32() : 1,
                        Enable = item.TryGetProperty("enable", out var enable) ? enable.GetBoolean() : true,
                        AlarmThreshold = item.TryGetProperty("alarmThreshold", out var threshold) ? threshold.GetDouble() : 0.0,
                        DevicesJson = item.TryGetProperty("devices", out var devices) ? devices.GetRawText() : null,
                        GeoMarkArrayJson = item.TryGetProperty("geoMarkArray", out var geoMarkArray) ? geoMarkArray.GetRawText() : null,
                        DataSource = item.TryGetProperty("dataSource", out var dataSource) ? dataSource.GetString() : null,
                        TargetType = item.TryGetProperty("targetType", out var targetType) ? targetType.GetString() : null,
                        Mode = item.TryGetProperty("mode", out var mode) ? mode.GetString() : null
                    };

                    await _alarmRuleService.AddAlarmRuleAsync(request);
                    IncrementTypeCount(result, "AlarmRule");
                }

                _logger.LogInformation($"成功迁移 {alarmRules.Count} 个报警规则");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移报警规则失败");
                throw;
            }
        }

        /// <summary>
        /// 迁移颜色配置
        /// </summary>
        private async Task MigrateColorSettingsAsync(string jsonContent, string projectId, MigrationResult result)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var colorSettings = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent, options);

                if (colorSettings == null || colorSettings.Count == 0)
                    return;

                foreach (var item in colorSettings)
                {
                    var request = new CreateColorSettingRequest
                    {
                        ProjectId = projectId,
                        SettingType = item.TryGetProperty("settingType", out var settingType) ? settingType.GetString() ?? "terrain" : "terrain",
                        Type = item.TryGetProperty("type", out var type) ? type.GetInt32() : 0,
                        MinValue = item.TryGetProperty("minValue", out var minValue) ? minValue.GetDouble() : -30.0,
                        MaxValue = item.TryGetProperty("maxValue", out var maxValue) ? maxValue.GetDouble() : 30.0,
                        HslHStart = item.TryGetProperty("hslHStart", out var hslHStart) ? hslHStart.GetInt32() : 240,
                        HslHEnd = item.TryGetProperty("hslHEnd", out var hslHEnd) ? hslHEnd.GetInt32() : 0,
                        HslDirection = item.TryGetProperty("hslDirection", out var hslDirection) ? hslDirection.GetInt32() : -1,
                        FilterEnable = item.TryGetProperty("filterEnable", out var filterEnable) ? filterEnable.GetBoolean() : false,
                        FilterMin = item.TryGetProperty("filterMin", out var filterMin) ? filterMin.GetDouble() : (double?)null,
                        FilterMax = item.TryGetProperty("filterMax", out var filterMax) ? filterMax.GetDouble() : (double?)null,
                        FilterAlpha = item.TryGetProperty("filterAlpha", out var filterAlpha) ? filterAlpha.GetDouble() : (double?)null,
                        HslS = item.TryGetProperty("hslS", out var hslS) ? hslS.GetDouble() : 1.0,
                        HslL = item.TryGetProperty("hslL", out var hslL) ? hslL.GetDouble() : 0.5,
                        ValueArrayJson = item.TryGetProperty("valueArray", out var valueArray) ? valueArray.GetRawText() : null,
                        ColorArrayJson = item.TryGetProperty("colorArray", out var colorArray) ? colorArray.GetRawText() : null,
                        AutoMode = item.TryGetProperty("auto", out var auto) ? auto.GetBoolean() : false
                    };

                    await _colorSettingService.CreateOrUpdateColorSettingAsync(request);
                    IncrementTypeCount(result, "ColorSetting");
                }

                _logger.LogInformation($"成功迁移 {colorSettings.Count} 个颜色配置");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移颜色配置失败");
                throw;
            }
        }

        /// <summary>
        /// 迁移面板配置
        /// </summary>
        private async Task MigratePanelConfigsAsync(string jsonContent, string projectId, MigrationResult result)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var panelConfigs = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent, options);

                if (panelConfigs == null || panelConfigs.Count == 0)
                    return;

                foreach (var item in panelConfigs)
                {
                    var request = new CreatePanelConfigRequest
                    {
                        ProjectId = projectId,
                        PanelType = item.TryGetProperty("panelType", out var panelType) ? panelType.GetString() ?? "target" : "target",
                        ConfigJson = item.TryGetProperty("config", out var config) ? config.GetRawText() : "{}"
                    };

                    await _panelConfigService.CreateOrUpdatePanelConfigAsync(request);
                    IncrementTypeCount(result, "PanelConfig");
                }

                _logger.LogInformation($"成功迁移 {panelConfigs.Count} 个面板配置");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移面板配置失败");
                throw;
            }
        }

        /// <summary>
        /// 迁移图像标记配置
        /// </summary>
        private async Task MigrateImageMarksAsync(string jsonContent, string projectId, MigrationResult result)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var imageMarks = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent, options);

                if (imageMarks == null || imageMarks.Count == 0)
                    return;

                foreach (var item in imageMarks)
                {
                    var request = new CreateImageMarkRequest
                    {
                        ProjectId = projectId,
                        ImageId = item.TryGetProperty("imageId", out var imageId) ? imageId.GetString() : null,
                        Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? "未命名" : "未命名",
                        MarkType = item.TryGetProperty("markType", out var markType) ? markType.GetString() ?? "Point" : "Point",
                        CoordinatesJson = item.TryGetProperty("coordinates", out var coordinates) ? coordinates.GetRawText() : null,
                        Description = item.TryGetProperty("description", out var description) ? description.GetString() : null,
                        Color = item.TryGetProperty("color", out var color) ? color.GetString() : "#FF0000"
                    };

                    await _imageMarkService.CreateImageMarkAsync(request);
                    IncrementTypeCount(result, "ImageMark");
                }

                _logger.LogInformation($"成功迁移 {imageMarks.Count} 个图像标记");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移图像标记失败");
                throw;
            }
        }

        /// <summary>
        /// 迁移图像分析配置
        /// </summary>
        private async Task MigrateImageAnalysisConfigAsync(string jsonContent, string projectId, MigrationResult result)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<JsonElement>(jsonContent, options);

                var request = new CreateImageAnalysisConfigRequest
                {
                    ProjectId = projectId,
                    StandardImageSidePixel = config.TryGetProperty("standardImageSidePixel", out var standardImageSidePixel) ? standardImageSidePixel.GetInt32() : 16384,
                    CompressImageSidePixel = config.TryGetProperty("compressImageSidePixel", out var compressImageSidePixel) ? compressImageSidePixel.GetInt32() : 1024,
                    MatrixTileRngNum = config.TryGetProperty("matrixTileRngNum", out var matrixTileRngNum) ? matrixTileRngNum.GetInt32() : 1203,
                    MatrixTileAngNum = config.TryGetProperty("matrixTileAngNum", out var matrixTileAngNum) ? matrixTileAngNum.GetInt32() : 61,
                    GenDefo = config.TryGetProperty("genDefo", out var genDefo) ? genDefo.GetBoolean() : false,
                    GenScat = config.TryGetProperty("genScat", out var genScat) ? genScat.GetBoolean() : true,
                    GenSpeed = config.TryGetProperty("genSpeed", out var genSpeed) ? genSpeed.GetBoolean() : false,
                    GenAcceleration = config.TryGetProperty("genAcceleration", out var genAcceleration) ? genAcceleration.GetBoolean() : false,
                    ConfigJson = jsonContent
                };

                await _imageAnalysisConfigService.CreateOrUpdateConfigAsync(request);
                IncrementTypeCount(result, "ImageAnalysisConfig");

                _logger.LogInformation("成功迁移图像分析配置");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移图像分析配置失败");
                throw;
            }
        }

        private void IncrementTypeCount(MigrationResult result, string typeName)
        {
            if (!result.TypeCounts.ContainsKey(typeName))
            {
                result.TypeCounts[typeName] = 0;
            }
            result.TypeCounts[typeName]++;
        }

        /// <summary>
        /// 生成迁移报告
        /// </summary>
        public string GenerateMigrationReport(MigrationResult result)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("====================================");
            report.AppendLine("配置迁移报告");
            report.AppendLine("====================================");
            report.AppendLine($"总文件数: {result.TotalFiles}");
            report.AppendLine($"成功: {result.SuccessCount}");
            report.AppendLine($"失败: {result.FailureCount}");
            report.AppendLine();
            
            if (result.TypeCounts.Count > 0)
            {
                report.AppendLine("迁移统计:");
                foreach (var kvp in result.TypeCounts)
                {
                    report.AppendLine($"  {kvp.Key}: {kvp.Value} 个");
                }
                report.AppendLine();
            }

            if (result.Errors.Count > 0)
            {
                report.AppendLine("错误列表:");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  - {error}");
                }
            }

            report.AppendLine("====================================");
            return report.ToString();
        }
    }
}

