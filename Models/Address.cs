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

        /// FK → Province.ProvinceID (nullable)
        [ForeignKey(nameof(Province))]
        public int? ProvinceID { get; set; }

        /// FK → District.DistrictID (nullable)
        [ForeignKey(nameof(District))]
        public int? DistrictID { get; set; }

        /// FK → Ward.WardID (nullable)
        [ForeignKey(nameof(Ward))]
        public int? WardID { get; set; }

        /// Chi tiết số nhà, đường, thôn, xóm (nvarchar(500))
        [StringLength(500)]
        public string DetailAddress { get; set; }

        /// Toàn bộ địa chỉ đầy đủ (nvarchar(1000))
        [StringLength(1000)]
        public string FullAddress { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public Province Province { get; set; }
        public District District { get; set; }
        public Ward Ward { get; set; }
    }
}
