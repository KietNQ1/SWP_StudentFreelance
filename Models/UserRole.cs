using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models
{
    public class UserRole
    {
        /// Khóa chính, tự tăng
        [Key]
        public int RoleID { get; set; }

        /// Tên vai trò (Admin, Student, Business,…)
        [Required, StringLength(450)]
        public string RoleName { get; set; }

        /// Mô tả chi tiết về quyền hạn của vai trò
        public string Description { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        /// Các User có vai trò này
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
