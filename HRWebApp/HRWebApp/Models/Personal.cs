namespace HRWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Personal")]
    public partial class Personal
    {
        public Personal()
        {
            Job_History = new HashSet<Job_History>();
        }

        [Key]
        [Column(TypeName = "numeric")]
        public decimal Employee_ID { get; set; }

        [StringLength(100)] // Khớp với nvarchar(100) trong ảnh
        public string First_Name { get; set; }

        [StringLength(100)]
        public string Last_Name { get; set; }

        // Đã xóa City, State, Email... vì image_2f5fbf.png xác nhận không có các cột này

        public bool? Gender { get; set; } // Kiểu bit trong SQL map với bool trong C#

        public bool? Shareholder_Status { get; set; } // Kiểu bit trong SQL map với bool trong C#

        [StringLength(100)]
        public string Ethnicity { get; set; }

        public DateTime? BirthDate { get; set; }

        public virtual Employment Employment { get; set; }
        public virtual ICollection<Job_History> Job_History { get; set; }
    }
}