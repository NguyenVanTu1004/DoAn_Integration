namespace HRWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Employment")]
    public partial class Employment
    {
        [Key]
        [Column(TypeName = "numeric")]
        public decimal Employee_ID { get; set; }

        [Column(TypeName = "money")]
        public decimal? Salary { get; set; }

        public DateTime? Hire_Date { get; set; }

        public int? Vacation_Days { get; set; }

        [StringLength(50)]
        public string Employment_Status { get; set; }

        [StringLength(50)]
        public string Benefit_Plan { get; set; }

        // ĐỊNH NGHĨA QUAN HỆ: Employee_ID ở đây trỏ trực tiếp sang Employee_ID của Personal
        [ForeignKey("Employee_ID")]
        public virtual Personal Personal { get; set; }
    }
}