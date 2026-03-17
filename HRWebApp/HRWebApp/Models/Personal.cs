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

        [StringLength(50)]
        public string First_Name { get; set; }

        [StringLength(50)]
        public string Last_Name { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone_Number { get; set; }

        [StringLength(50)]
        public string Marital_Status { get; set; }

        // Kiểu int? để nhận giá trị 0/1 từ SQL
        public int? Gender { get; set; }

        public int Shareholder_Status { get; set; }

        [StringLength(50)]
        public string Ethnicity { get; set; }

        public DateTime? BirthDate { get; set; }

        public virtual Employment Employment { get; set; }
        public virtual ICollection<Job_History> Job_History { get; set; }
    }
}