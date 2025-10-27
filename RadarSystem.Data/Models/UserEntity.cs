using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadarSystem.Data.Models
{
    /// <summary>
    /// 用户实体
    /// </summary>
    [Table("users")]
    public class UserEntity
    {
        [Key]
        [MaxLength(64)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? RealName { get; set; }

        [MaxLength(50)]
        public string? Role { get; set; } = "User";

        [MaxLength(64)]
        public string? ProjectId { get; set; } // 用户所属项目（可为空，表示管理员）

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public DateTime? UpdatedTime { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

