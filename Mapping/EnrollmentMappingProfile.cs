using AutoMapper;
using StudentManagement.API.DTOs.Enrollment;
using StudentManagement.API.Models;

namespace StudentManagement.API.Mapping
{
    public class EnrollmentMappingProfile : Profile
    {
        public EnrollmentMappingProfile()
        {
            CreateMap<EnrollmentRequestDTO, Enrollment>();
            CreateMap<Enrollment, EnrollmentResponseDTO>()
                    .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.Name))
                    .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
        }
    }
} 