using System.Xml;
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Data;
using StudentManagement.API.DTOs.Students;
using StudentManagement.API.Enums;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using StudentManagement.API.util;

namespace StudentManagement.API.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDBContext _context;
        public StudentRepository(AppDBContext context)
        {
            _context = context;
        }
        
        public async Task<PagedResult<Student>> GetAllAsync(StudentQueryParameters studentQuery)
        {
            var query = _context.Students
                .AsNoTracking()
                .Include(c => c.Course)
                .AsQueryable();
            
            query = ApplySearch(query, studentQuery.SearchTerm);
            query = ApplyFilter(query, studentQuery.CourseId);
            query = ApplySort(query, studentQuery.Sortby, studentQuery.IsDecending);
            
            var totalRecords = await query.CountAsync();
            var skip = (studentQuery.page -1 ) * studentQuery.pageSize;

            var items = await query
                .Skip(skip)
                .Take(studentQuery.pageSize)
                .ToListAsync();
            
            var totalPages = (int)Math.Ceiling((double)totalRecords / studentQuery.pageSize);
            return new PagedResult<Student>
            {
                Page = studentQuery.page,
                PageSize = studentQuery.pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasNextPage = studentQuery.page < totalPages,
                HasPreviousPage = studentQuery.page > 1,
                Items = items
            };
        }

        public async Task<Student?> GetByIdAsync(int id)            
        {
            return await _context.Students
                .Include(s => s.Course)
                .FirstOrDefaultAsync(x=> x.Id == id );
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

        private static IQueryable<Student> ApplySearch(IQueryable<Student> query, string? searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm)) return query;
            var search = searchTerm.Trim().ToLower();
            return query.Where(s =>
                s.Name.ToLower().Contains(search) || 
                s.Email.ToLower().Contains(search) || 
                s.Course.CourseName.ToLower().Contains(search)
            );
        }
        private static IQueryable<Student> ApplyFilter(IQueryable<Student>query, int? courseId)
        {
            if(!courseId.HasValue) return query;
            return query.Where(s => s.CourseId == courseId.Value);
        }

        private static IQueryable<Student> ApplySort(IQueryable<Student>query, string? sortby, bool isDecending)
        {
            if(string.IsNullOrWhiteSpace(sortby))
            {
                return isDecending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id);
            }
            return sortby.ToLower() switch
            {
                "name" => isDecending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "email" => isDecending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
                "course" => isDecending ? query.OrderByDescending(s => s.Course.CourseName) : query.OrderBy(s => s.Course.CourseName),
                "phone" => isDecending ? query.OrderByDescending(s => s.Phone) : query.OrderBy(s => s.Phone),
            _       => isDecending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
            };
        }
    }
}