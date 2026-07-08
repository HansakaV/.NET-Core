using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Models;

namespace StudentManagement.API.Data;
public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
    public DbSet<Student> Students { get; set; }    
}