using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Models;

namespace StudentManagement.API.Data;
public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasQueryFilter(s => !s.IsDeleted);
            
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Course)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        
        modelBuilder.Entity<Student>(property =>
        {
            property.HasIndex(s => s.Email).IsUnique();
            property.HasIndex(s => s.Name);
            property.HasIndex(s => s.CourseId);

        });
        modelBuilder.Entity<User>(property =>
        {
            property.HasIndex(u => u.Email).IsUnique();
        });
    }
    public DbSet<Student> Students { get; set; }    
    public DbSet<User> Users {get; set;}
}