using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models
{
    public class Province
    {
        /// Khóa chính, tự tăng
        [Key]
        public int ProvinceID { get; set; }

        /// Mã tỉnh/thành (varchar(255))
        [Required, StringLength(255)]
        public string Code { get; set; }

        /// Tên tỉnh/thành (nvarchar(255))
        [Required, StringLength(255)]
        public string Name { get; set; }

        /// Các District thuộc Province này
        public ICollection<District> Districts { get; set; } = new List<District>();

        /// Các Address tham chiếu Province này
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
