using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskHub_V1.Models;
using TaskHub_V1.Utilities;

namespace TaskHub_V1.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<ProjectTasks> ProjectTasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProject>()
                .HasKey(up => new { up.UserId, up.ProjectId });

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(p => p.Team)
                .HasForeignKey(u => u.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedTasks)
                .WithOne(pt => pt.User)
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ProjectTasks)
                .WithMany(pt => pt.Comments)
                .HasForeignKey(c => c.ProjectTasksId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            SeedRoles(modelBuilder);
        }
        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData
                (
                new IdentityRole() { Name = RoleNames.ProjectManager, ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "PROJECT MANAGER" },
                new IdentityRole() { Name = RoleNames.User, ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "USER" }
                );
        }
    }
}

