using Microsoft.EntityFrameworkCore;
using RadarSystem.Data.Context;
using RadarSystem.Data.Models;
using RadarSystem.WebAPI.Models;
using System.Security.Cryptography;
using System.Text;
using RadarSystem.Core.Models;

namespace RadarSystem.WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly RadarDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;

        public UserService(RadarDbContext context, ITokenService tokenService, ILogger<UserService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResponse?> AuthenticateAsync(string username, string password)
        {
            // 查找用户
            var user = await _context.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

            if (user == null)
            {
                _logger.LogWarning("用户不存在: {Username}", username);
                return null;
            }

            // 验证密码
            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                _logger.LogWarning("密码错误: {Username}", username);
                return null;
            }

            // 生成Token
            var token = _tokenService.GenerateToken(user.Id, user.Username, user.Role ?? "User");

            // 更新最后登录时间
            user.LastLoginTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(1440),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RealName = user.RealName,
                    Role = user.Role
                }
            };
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _context.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
                return false;

            var oldPasswordHash = HashPassword(oldPassword);
            if (user.PasswordHash != oldPasswordHash)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedTime = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {Username} 修改密码成功", user.Username);
            return true;
        }

        public async Task InitializeDefaultDataAsync()
        {
            // 1. 检查是否已有管理员用户
            var adminExists = await _context.Set<UserEntity>()
                .AnyAsync(u => u.Username == "admin");

            if (!adminExists)
            {
                // 创建默认管理员
                var admin = new UserEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",
                    PasswordHash = HashPassword("admin123"),
                    Email = "admin@radar.com",
                    RealName = "系统管理员",
                    Role = "Admin",
                    IsActive = true,
                    CreatedTime = DateTime.Now
                };

                _context.Set<UserEntity>().Add(admin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("创建默认管理员账号成功: admin/admin123");
            }

            // 2. 检查是否已有项目数据
            var projectExists = await _context.Projects.AnyAsync(p => !p.IsDeleted);

            if (!projectExists)
            {
                // 创建默认测试项目
                var defaultProject = new ProjectEntity
                {
                    ProjectId = "DEFAULT_PROJECT",
                    ProjectName = "默认测试项目",
                    Description = "系统初始化自动创建的测试项目",
                    Location = "苏州工业园区",
                    Status = "Active",
                    CreatedBy = "system",
                    StoragePath = "/data/projects/default",
                    ContactPerson = "系统管理员",
                    ContactPhone = "13800138000",
                    ContactEmail = "admin@radar.com",
                    Longitude = 120.6,
                    Latitude = 31.3,
                    Elevation = 10,
                    StartDate = DateTime.Now,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    IsDeleted = false
                };

                _context.Projects.Add(defaultProject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("创建默认测试项目成功: DEFAULT_PROJECT - 默认测试项目");
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

