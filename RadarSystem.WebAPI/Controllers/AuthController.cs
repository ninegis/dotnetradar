using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarSystem.WebAPI.Models;
using RadarSystem.WebAPI.Services;

namespace RadarSystem.WebAPI.Controllers
{
    /// <summary>
    /// 用户认证控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _userService.AuthenticateAsync(request.Username, request.Password);

                if (result == null)
                {
                    return Ok(ApiResponse<LoginResponse>.Fail(401, "用户名或密码错误"));
                }

                _logger.LogInformation("用户 {Username} 登录成功", request.Username);
                return Ok(ApiResponse<LoginResponse>.Ok(result, "登录成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                return Ok(ApiResponse<LoginResponse>.Fail(500, $"登录失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public ActionResult<ApiResponse<LogoutResponse>> Logout()
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("用户 {Username} 登出", username);
            return Ok(ApiResponse<object>.Ok(null, "登出成功"));
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PasswordChangeResponse>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(ApiResponse<object>.Fail(401, "未授权"));
                }

                var result = await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

                if (!result)
                {
                    return Ok(ApiResponse<object>.Fail(400, "原密码错误"));
                }

                return Ok(ApiResponse<object>.Ok(null, "密码修改成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改密码失败");
                return Ok(ApiResponse<object>.Fail(500, $"修改密码失败: {ex.Message}"));
            }
        }
    }
}

