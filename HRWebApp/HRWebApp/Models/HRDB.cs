namespace HRWebApp.Models
{
    using System.Data.Entity;

    public partial class HRDB : DbContext
    {
        public HRDB() : base("name=HRDB") { }

        // Phải có đủ 4 dòng này thì các Controller khác mới chạy được
        public virtual DbSet<Employment> Employments { get; set; }
        public virtual DbSet<Personal> Personals { get; set; }
        public virtual DbSet<Job_History> Job_History { get; set; } // Thêm lại dòng nàyB
        public virtual DbSet<Benefit_Plans> Benefit_Plans { get; set; } // Thêm lại dòng này

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Personal>()
                .Property(e => e.Employee_ID).HasPrecision(18, 0);

            modelBuilder.Entity<Employment>()
                .Property(e => e.Employee_ID).HasPrecision(18, 0);

            modelBuilder.Entity<Employment>()
                .Property(e => e.Salary).HasPrecision(19, 4);

            modelBuilder.Entity<Personal>()
                .HasOptional(e => e.Employment)
                .WithRequired(e => e.Personal);
        }
    }
}