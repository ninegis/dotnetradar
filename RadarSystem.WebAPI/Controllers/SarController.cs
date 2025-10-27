using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using RadarSystem.WebAPI.Services;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// SAR雷达图像控制器
    /// </summary>
    [ApiController]
    [Route("api/sar")]
    [Authorize]
    public class SarController : ControllerBase
    {
        private readonly IRadarImageService _radarImageService;
        private readonly ILogger<SarController> _logger;

        public SarController(
            IRadarImageService radarImageService,
            ILogger<SarController> logger)
        {
            _radarImageService = radarImageService;
            _logger = logger;
        }

        /// <summary>
        /// 查询SAR图像数量
        /// </summary>
        [HttpPost("image/count")]
        public async Task<IActionResult> QueryImageCount([FromBody] QueryImageRequest request)
        {
            try
            {
                var count = await _radarImageService.GetImageCountAsync(
                    request.ProjectId, 
                    request.DevId, 
                    request.StartDateTime, 
                    request.EndDateTime, 
                    request.Type?.ToString(), 
                    request.Status?.ToString());
                    
                return Ok(new { code = 200, data = new { count = 1, dataset = new[] { new[] { count } } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询SAR图像数量失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询SAR图像列表
        /// </summary>
        [HttpPost("image/list")]
        public async Task<IActionResult> QueryImageList([FromBody] QueryImageRequest request)
        {
            try
            {
                var images = await _radarImageService.GetImageListAsync(
                    request.ProjectId, 
                    request.DevId, 
                    request.StartDateTime, 
                    request.EndDateTime, 
                    request.Type?.ToString(), 
                    request.Status?.ToString(), 
                    request.PageRowSize);
                    
                return Ok(new { code = 200, data = new { dataset = images } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询SAR图像列表失败");
                return Ok(new { code = 500, message = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 生成SAR雷达图像
        /// </summary>
        [HttpPost("generate/image")]
        public async Task<IActionResult> GenerateImage([FromBody] GenerateSarImageRequest request)
        {
            try
            {
                _logger.LogInformation("生成SAR雷达图像: {DeviceId}, {ProjectId}", request.DeviceId, request.ProjectId);
                
                var generateRequest = new RadarSystem.WebAPI.Models.GenerateImageRequest
                {
                    ProjectId = request.ProjectId,
                    DeviceId = request.DeviceId,
                    StartTime = request.Ts.ToString(),
                    EndTime = request.Ts.ToString(),
                    ImageType = request.Type?.ToString() ?? "10"
                };
                
                var result = await _radarImageService.GenerateImageAsync(generateRequest);
                
                return Ok(new { code = 200, data = result, message = "图像生成任务已提交" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成SAR雷达图像失败");
                return Ok(new { code = 500, message = $"生成失败: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// 查询图像请求参数
    /// </summary>
    public class QueryImageRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string DevId { get; set; } = string.Empty;
        public string StartDateTime { get; set; } = string.Empty;
        public string EndDateTime { get; set; } = string.Empty;
        public int? Status { get; set; }
        public int? Type { get; set; }
        public int PageRowSize { get; set; } = 5;
        public int Page { get; set; } = 1;
    }

    /// <summary>
    /// 生成SAR图像请求参数
    /// </summary>
    public class GenerateSarImageRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TimeUnit { get; set; } = string.Empty;
        public long Ts { get; set; }
        public int? Type { get; set; }
    }
}

