using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Category
    {
        /// Khóa chính, tự tăng
        [Key]
        public int CategoryID { get; set; }

        /// Tên danh mục (nvarchar(100))
        [Required, StringLength(100)]
        public string CategoryName { get; set; }

        /// Loại danh mục (PROJECT, SKILL, INDUSTRY, MAJOR)
        [Required, StringLength(50)]
        public string CategoryType { get; set; }

        /// Mô tả thêm
        public string Description { get; set; }

        /// FK → Category.CategoryID (parent)
        [ForeignKey(nameof(ParentCategory))]
        public int? ParentCategoryID { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        /// Danh mục cha (nếu có)
        public Category ParentCategory { get; set; }

        /// Danh sách con (subcategories)
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        /// Các Project thuộc Category này
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        /// Các Skill thuộc Category này
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
