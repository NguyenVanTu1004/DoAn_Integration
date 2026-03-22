namespace HRWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Personal")]
    public partial class Personal
    {
        [Key]
        [Column(TypeName = "numeric")]
        public decimal Employee_ID { get; set; }

        [StringLength(100)]
        public string First_Name { get; set; }

        [StringLength(100)]
        public string Last_Name { get; set; }

        public bool? Gender { get; set; }

        public bool? Shareholder_Status { get; set; }

        [StringLength(100)]
        public string Ethnicity { get; set; }

        public DateTime? BirthDate { get; set; }

        // Quan hệ 1-1 sang bảng Employment
        public virtual Employment Employment { get; set; }
    }
}