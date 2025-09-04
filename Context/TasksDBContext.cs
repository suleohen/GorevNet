using GorevNet.Models;
using Microsoft.EntityFrameworkCore;

namespace GorevNet.Context
{
    public class TasksDBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Data Source=.\SQLEXPRESS;Database=UserTask;Integrated Security=True;TrustServerCertificate=True");
        }

        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Employee> Employees { get; set; }



    }
}
