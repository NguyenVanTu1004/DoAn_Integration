namespace HRWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Job_History")]
    public partial class Job_History
    {
        [Key]
        [Column(Order = 0, TypeName = "numeric")]
        public decimal Employee_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime Start_Date { get; set; }

        public DateTime? End_Date { get; set; }
        public string Job_Title { get; set; }
        public string Department { get; set; }

        public virtual Personal Personal { get; set; }
    }
}