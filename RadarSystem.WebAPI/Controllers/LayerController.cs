using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using RadarSystem.WebAPI.Services;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 图层管理控制器 - 对应UCML系统的图层功能
    /// </summary>
    [ApiController]
    [Route("sloperadar/api")]
    [Authorize]
    public class LayerController : ControllerBase
    {
        private readonly ILayerService _layerService;
        private readonly ILogger<LayerController> _logger;

        public LayerController(
            ILayerService layerService,
            ILogger<LayerController> logger)
        {
            _layerService = layerService;
            _logger = logger;
        }

        /// <summary>
        /// 添加图层
        /// </summary>
        /// <remarks>GET /sloperadar/api/addlayer</remarks>
        [HttpGet("addlayer")]
        public async Task<IActionResult> AddLayer(
            [FromQuery] string oid,
            [FromQuery] string name,
            [FromQuery] string type,
            [FromQuery] string url,
            [FromQuery] string userid,
            [FromQuery] string postid,
            [FromQuery] string divisionid,
            [FromQuery] string orgid,
            [FromQuery] string treeid)
        {
            try
            {
                var request = new RadarSystem.WebAPI.Models.AddLayerRequest
                {
                    Oid = oid,
                    Name = name,
                    Type = type,
                    Url = url,
                    UserId = userid,
                    PostId = postid,
                    DivisionId = divisionid,
                    OrgId = orgid,
                    TreeId = treeid
                };
                
                var result = await _layerService.AddLayerAsync(request);
                return Ok(new { code = 200, data = result, message = "图层添加成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加图层失败");
                return Ok(new { code = 500, message = $"添加失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 删除图层
        /// </summary>
        /// <remarks>GET /sloperadar/api/deletelayer</remarks>
        [HttpGet("deletelayer")]
        public async Task<IActionResult> DeleteLayer([FromQuery] string oid)
        {
            try
            {
                await _layerService.DeleteLayerAsync(oid);
                return Ok(new { code = 200, message = "图层删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除图层失败");
                return Ok(new { code = 500, message = $"删除失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 启用/禁用图层
        /// </summary>
        /// <remarks>GET /sloperadar/api/enablelayer</remarks>
        [HttpGet("enablelayer")]
        public async Task<IActionResult> EnableLayer([FromQuery] string oid, [FromQuery] bool enable)
        {
            try
            {
                await _layerService.EnableLayerAsync(oid, enable);
                return Ok(new { code = 200, message = enable ? "图层已启用" : "图层已禁用" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启用/禁用图层失败");
                return Ok(new { code = 500, message = $"操作失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 显示/隐藏图层
        /// </summary>
        /// <remarks>GET /sloperadar/api/showlayer</remarks>
        [HttpGet("showlayer")]
        public async Task<IActionResult> ShowLayer([FromQuery] string oid, [FromQuery] bool show)
        {
            try
            {
                await _layerService.ShowLayerAsync(oid, show);
                return Ok(new { code = 200, message = show ? "图层已显示" : "图层已隐藏" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "显示/隐藏图层失败");
                return Ok(new { code = 500, message = $"操作失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取图层列表
        /// </summary>
        /// <remarks>GET /sloperadar/api/getlayer</remarks>
        [HttpGet("getlayer")]
        public async Task<IActionResult> GetLayers([FromQuery] string orgid)
        {
            try
            {
                var layers = await _layerService.GetLayersAsync(orgid);
                return Ok(new { code = 200, data = layers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取图层列表失败");
                return Ok(new { code = 500, message = $"获取失败: {ex.Message}" });
            }
        }
    }
}

