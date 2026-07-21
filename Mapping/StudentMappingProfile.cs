
using AutoMapper;
using StudentManagement.API.DTOs;
using StudentManagement.API.Models;

namespace StudentManagement.API.Mapping
{
    public class StudentMappingProfile : Profile
    {
        public StudentMappingProfile()
        {
            CreateMap<StudentCreateRequestDto, Student>();
            CreateMap<Student, StudentResponseDto>();
        }
    }
}