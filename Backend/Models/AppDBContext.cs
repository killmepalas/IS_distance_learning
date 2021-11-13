using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IS_distance_learning.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_distance_learning
{
    public class AppDBContext : DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<Test> Test { get; set; }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

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
                RoleId = 1,
                Role = "admin",
            });
        }
    }
}

