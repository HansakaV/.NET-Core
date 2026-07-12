using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using StudentManagement.API.DTOs;


namespace StudentManagement.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _isStudentRepository;
        public StudentService(IStudentRepository isStudentRepository)
        {
            _isStudentRepository = isStudentRepository;
        }

        public async Task<List<StudentResponseDto>> GetAllAsync()
        {
            var students = await _isStudentRepository.GetAllAsync();

            return students.Select(student => new StudentResponseDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Course = student.Course,
                Phone = student.Phone
                
            }).ToList();
            
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

        }

        public async Task DeleteAsync(int id)
        {
            var rowsEffected = await _isStudentRepository.DeleteAsync(id);
            if (rowsEffected == 0)
            {
                throw new KeyNotFoundException($"Student with ID {id} not found.");
            }
        }
        
    }
}