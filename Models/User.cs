using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class User
    {
        /// Khóa chính, tự tăng
        [Key]
        public int UserID { get; set; }

        /// Tên đăng nhập, duy nhất
        [Required, StringLength(50)]
        public string Username { get; set; }

        /// Email dùng để đăng nhập và nhận thông báo
        [Required, StringLength(100)]
        public string Email { get; set; }

        /// Hash của mật khẩu
        [Required]
        public string PasswordHash { get; set; }

        /// FK → UserRole.RoleID
        [ForeignKey(nameof(UserRole))]
        public int RoleID { get; set; }

        /// FK → Address.AddressID (nullable nếu chưa có địa chỉ)
        [ForeignKey(nameof(Address))]
        public int? AddressID { get; set; }

        /// Số dư ví (nạp/rút trong hệ thống)
        [Column(TypeName = "decimal(15,2)")]
        public decimal WalletBalance { get; set; }

        /// Cờ VIP (true nếu là tài khoản VIP)
        public bool VipStatus { get; set; }

        /// Tên trường/đại học (nếu là student)
        public string? University { get; set; }

        /// Chuyên ngành (nếu là student)
        public string? Major { get; set; }

        /// Tên công ty (nếu là business)
        public string? CompanyName { get; set; }

        /// Lĩnh vực hoạt động (nếu là business)
        public string? Industry { get; set; }

        /// Cờ bật/tắt profile public
        public bool ProfileStatus { get; set; }

        /// Điểm đánh giá trung bình
        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; }

        /// Tổng số dự án đã tham gia thành công
        public int TotalProjects { get; set; }

        /// Tổng số dự án đã đăng
        public int TotalProjectsPosted { get; set; }

        /// Đường dẫn lưu avatar/profile picture
        public string? ProfilePicturePath { get; set; }

        /// Ngày tạo tài khoản
        public DateTime CreatedAt { get; set; }

        /// Ngày cập nhật thông tin gần nhất
        public DateTime UpdatedAt { get; set; }

        /// Tên đầy đủ của người dùng
        [Required]
        public string FullName { get; set; }

        /// Số điện thoại của người dùng
        public string? PhoneNumber { get; set; }

        /// Avatar của người dùng
        public string? Avatar { get; set; }

        /// Trạng thái hoạt động của tài khoản
        public bool IsActive { get; set; } = true;

        /// FK → AccountStatus.StatusID
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }

        // Navigation properties
        public UserRole UserRole { get; set; }
        public Address Address { get; set; }
        public AccountStatus Status { get; set; }
    }
}
