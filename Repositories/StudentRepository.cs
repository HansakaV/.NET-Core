using System.Xml;
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Data;
using StudentManagement.API.DTOs.Students;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;

namespace StudentManagement.API.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDBContext _context;
        public StudentRepository(AppDBContext context)
        {
            _context = context;
        }
        
        public async Task<List<Student>> GetAllAsync(StudentQueryParameters studentQuery)
        {
            var skip = (studentQuery.page -1 ) * studentQuery.pageSize;
            return await _context.Students
                .AsNoTracking()
                .OrderBy(student => student.Id)
                .Skip(skip)
                .Take(studentQuery.pageSize)
                .ToListAsync();

        }

        public async Task<Student?> GetByIdAsync(int id)            
        {
            return await _context.Students.FirstOrDefaultAsync(x=> x.Id == id );
        }

        public async Task<Student> CreateAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _context.Students
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
        }

        public Task<Student?> GetByEmailAsync(string email)
        {
            return _context.Students.FirstOrDefaultAsync(s => s.Email == email);
        }
    }
}