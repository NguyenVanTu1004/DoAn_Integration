namespace HRWebApp.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Benefit_Plans")]
    public partial class Benefit_Plans
    {
        [Key]
        [Column(TypeName = "numeric")]
        public decimal Benefit_Plan_ID { get; set; }

        [StringLength(50)]
        public string Plan_Name { get; set; }

        public decimal? Deductable { get; set; }
        public int? Percentage_CoPay { get; set; }
    }
}