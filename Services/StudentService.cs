using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using StudentManagement.API.DTOs;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;


namespace StudentManagement.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _isStudentRepository;
        private readonly IMemoryCache _cache;
        private const string StudentCacheKey = "students_cache_key";
        private readonly IMapper _mapper;
        public StudentService(IStudentRepository isStudentRepository, IMemoryCache cache, IMapper mapper)
        {
            _isStudentRepository = isStudentRepository;
            _cache = cache;
            _mapper= mapper;

        }

        public async Task<List<StudentResponseDto>> GetAllAsync()
        {
            if(!_cache.TryGetValue(StudentCacheKey, out List<StudentResponseDto>? cachedStudents))
            {
                var students = await _isStudentRepository.GetAllAsync();

                cachedStudents =  _mapper.Map<List<StudentResponseDto>>(students);

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

            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<StudentResponseDto> CreateAsync(StudentCreateRequestDto request)
        {
            var exitedStudent = await _isStudentRepository.GetByEmailAsync(request.Email);
            if(exitedStudent != null) throw new Exception("Email Already Exists !");

            var student = _mapper.Map<Student>(request);

            var createdStudent = await _isStudentRepository.CreateAsync(student);
            _cache.Remove(StudentCacheKey); // Invalidate the cache after creating a new student

            return _mapper.Map<StudentResponseDto>(createdStudent);
            
        }

        public async Task UpdateAsync(StudentUpdateRequestDto request)
        {
            var student = await _isStudentRepository.GetByIdAsync(request.Id);
            if (student == null)
            {
                throw new KeyNotFoundException($"Student with ID {request.Id} not found.");
            }
            var updatedStudent = _mapper.Map(request, student);
            
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