using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.DTOs.Enrollment;
using StudentManagement.API.Interfaces;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EnrollmentResponseDTO>> EnrollStudent(
            [FromBody] EnrollmentRequestDTO enrollmentRequest, CancellationToken cancellationToken)
        {
             await _enrollmentService.EnrollStudentAsync(enrollmentRequest,cancellationToken);
            return Created();
        }
    }
}