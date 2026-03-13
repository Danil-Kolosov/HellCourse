using buildingCompany.Models;
using Microsoft.EntityFrameworkCore;

namespace buildingCompany.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<BuildingSite> BuildingSites { get; set; }
        public DbSet<WorkType> WorkTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей
            modelBuilder.Entity<WorkLog>()
                .HasOne(w => w.BuildingSite)
                .WithMany(b => b.WorkLogs)
                .HasForeignKey(w => w.BuildingSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkLog>()
                .HasOne(w => w.WorkType)
                .WithMany(wt => wt.WorkLogs)
                .HasForeignKey(w => w.WorkTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkLog>()
                .HasOne(w => w.Employee)
                .WithMany(e => e.WorkLogs)
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Data (начальные данные) - добавляем только если таблицы пустые
        }
    }
}