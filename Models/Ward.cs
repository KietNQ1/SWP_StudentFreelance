using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Ward
    {
        /// Khóa chính, tự tăng
        [Key]
        public int WardID { get; set; }

        /// Mã phường/xã (varchar(255))
        [Required, StringLength(255)]
        public string Code { get; set; }

        /// Tên phường/xã (nvarchar(255))
        [Required, StringLength(255)]
        public string Name { get; set; }

        /// FK → District.DistrictID
        [ForeignKey(nameof(District))]
        public int DistrictID { get; set; }

        /// Quan hệ nhiều Ward thuộc District
        public District District { get; set; }

        /// Các Address tham chiếu Ward này
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
