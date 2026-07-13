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
            .Property(s => s.Course)
            .HasConversion<string>();
    }
    public DbSet<Student> Students { get; set; }    
    public DbSet<User> Users {get; set;}
}