
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
            CreateMap<StudentUpdateRequestDto, Student>()
                .ForAllMembers(options => options.Condition((src, dest, srcMember) => srcMember != null)); //copy to entity only not null values
        }
    }
}