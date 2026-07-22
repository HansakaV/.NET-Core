
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
            CreateMap<Student, StudentResponseDto>()
                .ForMember(dest => dest.Course, opt =>opt.MapFrom(src => src.Course != null? src.Course.CourseName : string.Empty));
        
            //custom mapping for update
            CreateMap<StudentUpdateRequestDto, Student>()
                .ForAllMembers(options => options.Condition((src, dest, srcMember) => srcMember != null)); //copy to entity only not null values'

        }
    }
}