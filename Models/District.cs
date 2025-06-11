using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class District
    {
        /// Khóa chính, tự tăng
        [Key]
        public int DistrictID { get; set; }

        /// Mã quận/huyện (varchar(255))
        [Required, StringLength(255)]
        public string Code { get; set; }

        /// Tên quận/huyện (nvarchar(255))
        [Required, StringLength(255)]
        public string Name { get; set; }

        /// FK → Province.ProvinceID
        [ForeignKey(nameof(Province))]
        public int ProvinceID { get; set; }

        /// Quan hệ nhiều District thuộc Province
        public Province Province { get; set; }

        /// Các Ward thuộc District này
        public ICollection<Ward> Wards { get; set; } = new List<Ward>();

        /// Các Address tham chiếu District này
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
