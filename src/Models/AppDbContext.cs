using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IS_distance_learning.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_distance_learning.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<AnswerStudent> AnswerStudent { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role[]
                {
                    new Role{Id=1, Name="admin"},
                    new Role{Id=2, Name="teacher"},
                    new Role{Id=3, Name="student"}
                });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 1,
                Login = "admin",
                Password = "admin",
                Name = "admin",
                MiddleName = "admin",
                LastName = "admin",
                RoleId = 1
            });
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                AccountId = 1
            });
        }
    }
}

