using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using StudentManagement.API.DTOs;
using Microsoft.Extensions.Caching.Memory;


namespace StudentManagement.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _isStudentRepository;
        private readonly IMemoryCache _cache;
        private const string StudentCacheKey = "students_cache_key";
        public StudentService(IStudentRepository isStudentRepository, IMemoryCache cache)
        {
            _isStudentRepository = isStudentRepository;
            _cache = cache;
        }

        public async Task<List<StudentResponseDto>> GetAllAsync()
        {
            if(!_cache.TryGetValue(StudentCacheKey, out List<StudentResponseDto>? cachedStudents))
            {
                var students = await _isStudentRepository.GetAllAsync();

                cachedStudents = students.Select(student => new StudentResponseDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    Course = student.Course,
                    Phone = student.Phone
                }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(StudentCacheKey, cachedStudents, cacheOptions);

            }
            return cachedStudents!;
        }
        public async Task<StudentResponseDto?> GetByIdAsync(int id)
        {
            var student = await _isStudentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return null;
            }

            return new StudentResponseDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Course = student.Course,
                Phone = student.Phone
            };
        }

        public async Task<StudentResponseDto> CreateAsync(StudentCreateRequestDto request)
        {
            var student = new Student
            {
                Name = request.Name,
                Email = request.Email,
                Course = request.Course,
                Phone = request.Phone
            };

            var createdStudent = await _isStudentRepository.CreateAsync(student);
            _cache.Remove(StudentCacheKey); // Invalidate the cache after creating a new student

            return new StudentResponseDto
            {
                Id = createdStudent.Id,
                Name = createdStudent.Name,
                Email = createdStudent.Email,
                Course = createdStudent.Course,
                Phone = createdStudent.Phone
            };
        }

        public async Task UpdateAsync(StudentUpdateRequestDto request)
        {
            var student = await _isStudentRepository.GetByIdAsync(request.Id);
            if (student == null)
            {
                throw new KeyNotFoundException($"Student with ID {request.Id} not found.");
            }
            var updatedStudent = new Student
            {
                Id = request.Id,
                Name = request.Name ?? student.Name,
                Email = request.Email ?? student.Email,
                Course = request.Course ?? student.Course,
                Phone = request.Phone ?? student.Phone
            };
            await _isStudentRepository.UpdateAsync(updatedStudent);
            _cache.Remove(StudentCacheKey); // Invalidate the cache after updating a student
        }

        public async Task DeleteAsync(int id)
        {
            var rowsEffected = await _isStudentRepository.DeleteAsync(id);
            if (rowsEffected == 0)
            {
                throw new KeyNotFoundException($"Student with ID {id} not found.");
            }
            _cache.Remove(StudentCacheKey); // Invalidate the cache after deleting a student
        }
        
    }
}