namespace HRWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Employment")]
    public partial class Employment
    {
        [Key]
        [Column(TypeName = "numeric")]
        public decimal Employee_ID { get; set; }

        // Sửa lỗi CS1061 cho 'Salary' (Dòng 74 trong Controller)
        [Column(TypeName = "money")]
        public decimal? Salary { get; set; }

        // Sửa lỗi CS1061 cho 'HireDate' (Dòng 78, 79 trong Controller)
        // Lưu ý: Tên phải là HireDate (không có dấu gạch dưới) để khớp với code Controller của bạn
        public DateTime? HireDate { get; set; }

        // Sửa lỗi CS1061 cho 'Vacation_Days' (Dòng 37, 72 trong Controller)
        public int? Vacation_Days { get; set; }

        [StringLength(50)]
        public string Employment_Status { get; set; }

        [StringLength(50)]
        public string Benefit_Plan { get; set; }

        // Thiết lập mối quan hệ với bảng Personal
        public virtual Personal Personal { get; set; }
    }
}