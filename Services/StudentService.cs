using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using StudentManagement.API.DTOs;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using StudentManagement.API.DTOs.Students;
using Microsoft.Extensions.Primitives;
using System.Threading;

namespace StudentManagement.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _isStudentRepository;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        private static CancellationTokenSource _resetCacheToken = new();

        public StudentService(IStudentRepository isStudentRepository, IMemoryCache cache, IMapper mapper)
        {
            _isStudentRepository = isStudentRepository;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<List<StudentResponseDto>> GetAllAsync(StudentQueryParameters query)
        {
            var cacheKey = $"students_page_{query.page}_size_{query.pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<StudentResponseDto>? cachedStudents))
            {
                var students = await _isStudentRepository.GetAllAsync(query);

                cachedStudents = _mapper.Map<List<StudentResponseDto>>(students);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                    .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                _cache.Set(cacheKey, cachedStudents, cacheOptions);
            }

            return cachedStudents!;
        }

        public async Task<StudentResponseDto?> GetByIdAsync(int id)
        {
            var student = await _isStudentRepository.GetByIdAsync(id);
            if (student == null) return null;

            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<StudentResponseDto> CreateAsync(StudentCreateRequestDto request)
        {
            var exitedStudent = await _isStudentRepository.GetByEmailAsync(request.Email);
            if (exitedStudent != null) throw new Exception("Email Already Exists !");

            var student = _mapper.Map<Student>(request);
            var createdStudent = await _isStudentRepository.CreateAsync(student);

            ClearAllStudentCaches();

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

            ClearAllStudentCaches();
        }

        public async Task DeleteAsync(int id)
        {
            var rowsEffected = await _isStudentRepository.DeleteAsync(id);
            if (rowsEffected == 0)
            {
                throw new KeyNotFoundException($"Student with ID {id} not found.");
            }

            ClearAllStudentCaches();
        }

        private static void ClearAllStudentCaches()
        {
            if (!_resetCacheToken.IsCancellationRequested)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
                _resetCacheToken = new CancellationTokenSource();
            }
        }
    }
}