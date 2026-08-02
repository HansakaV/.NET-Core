using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using StudentManagement.API.DTOs;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using StudentManagement.API.DTOs.Students;
using Microsoft.Extensions.Primitives;
using System.Threading;
using StudentManagement.API.util;
using StudentManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Middlewares.Exceptions;

namespace StudentManagement.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _isStudentRepository;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly AppDBContext _context;
        private ILogger<StudentService> _logger;

        private static CancellationTokenSource _resetCacheToken = new();

        public StudentService(IStudentRepository isStudentRepository, IMemoryCache cache, IMapper mapper, AppDBContext context,ILogger<StudentService> logger)
        {
            _isStudentRepository = isStudentRepository;
            _cache = cache;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<StudentResponseDto>> GetAllAsync(StudentQueryParameters query)
        {
            var cacheKey = $"students_page_{query.page}_size_{query.pageSize}_q{query.SearchTerm}_c{query.CourseId}_sort{query.Sortby}_desc{query.IsDecending}";

            if (!_cache.TryGetValue(cacheKey, out PagedResult<StudentResponseDto>? cachedStudents))
            {
                var students = await _isStudentRepository.GetAllAsync(query);

                var mappedItems = _mapper.Map<List<StudentResponseDto>>(students.Items);
                cachedStudents = new PagedResult<StudentResponseDto>
                {
                    Page = students.Page,
                    PageSize = students.PageSize,
                    TotalRecords = students.TotalRecords,
                    TotalPages = students.TotalPages,
                    HasNextPage = students.HasNextPage,
                    HasPreviousPage = students.HasPreviousPage,
                    Items = mappedItems
                };

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
            if (student == null) throw new StudentNotFoundException(id);

            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<StudentResponseDto> CreateAsync(StudentCreateRequestDto request)
        {
            var normalizeEmail = request.Email.Trim().ToLowerInvariant();

            _logger.LogDebug("Begining Student Create For Email Domain {EmailDomain}", ExtractEmailDomain(normalizeEmail));
            var exitedStudent = await _isStudentRepository.GetByEmailAsync(request.Email);
            if (exitedStudent != null) throw new ArgumentException("Email Already Exists !");

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
                throw new StudentNotFoundException(request.Id);
            }
            _context.Entry(student)
                .Property(student => student.Version)
                .OriginalValue = request.Version;
            
            var updatedStudent = _mapper.Map(request, student);
            updatedStudent.Version = request.Version + 1;

            try
            {
                await _isStudentRepository.UpdateAsync(updatedStudent);
                ClearAllStudentCaches();
            }
            catch(DbUpdateConcurrencyException)
            {
                throw new StudentConcurrencyException(
                    "The record you attempted to edit was modified by another user after you got the original value. Please refresh the page and try again.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var rowsEffected = await _isStudentRepository.DeleteAsync(id);
            if (rowsEffected == 0)
            {
                throw new StudentNotFoundException(id);
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

        private static string ExtractEmailDomain(string email)
        {
            var seperatorIndex = email.LastIndexOf('@');
            
            return seperatorIndex >= 0 
                ? email[(seperatorIndex+1)..]
                : "unknown";

        }
    }
}