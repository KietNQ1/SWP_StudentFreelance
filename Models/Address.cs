using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Address
    {
        /// Khóa chính, tự tăng
        [Key]
        public int AddressID { get; set; }

        /// API Province ID (string)
        [StringLength(20)]
        public string ProvinceCode { get; set; }
        
        /// Province name for display
        [StringLength(255)]
        public string ProvinceName { get; set; }

        /// API District ID (string)
        [StringLength(20)]
        public string DistrictCode { get; set; }
        
        /// District name for display
        [StringLength(255)]
        public string DistrictName { get; set; }

        /// API Ward ID (string)
        [StringLength(20)]
        public string WardCode { get; set; }
        
        /// Ward name for display
        [StringLength(255)]
        public string WardName { get; set; }

        /// Chi tiết số nhà, đường, thôn, xóm (nvarchar(500))
        [StringLength(500)]
        public string DetailAddress { get; set; }

        /// Toàn bộ địa chỉ đầy đủ (nvarchar(1000))
        [StringLength(1000)]
        public string FullAddress { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;
    }
}
