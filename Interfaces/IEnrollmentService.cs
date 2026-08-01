using StudentManagement.API.DTOs.Enrollment;

namespace StudentManagement.API.Interfaces
{
    public interface IEnrollmentService
    {
        Task<EnrollmentResponseDTO> EnrollStudentAsync (EnrollmentRequestDTO enrollmentRequest , CancellationToken cancellationToken);
    }
}