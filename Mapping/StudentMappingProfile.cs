
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

            //custom mapping for update
            CreateMap<StudentUpdateRequestDto, Student>()
                .ForAllMembers(options => options.Condition((src, dest, srcMember) => srcMember != null)); //copy to entity only not null values
            
            //custom mapping 
            // CreateMap<Student, StudentResponseDto>()
            //     .ForMember(
            //         dest => dest.FullName,
            //         opt => opt.MapFrom(
            //             src => src.FirstName + "" + src.LastName
            //         )
            //     );

        }
    }
}