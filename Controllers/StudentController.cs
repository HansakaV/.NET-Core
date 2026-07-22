using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.DTOs;
using StudentManagement.API.DTOs.Students;
using StudentManagement.API.Interfaces;
using StudentManagement.API.util;

namespace StudentManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
        
    }

[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult<PagedResult<StudentResponseDto>>> GetStudents(
    [FromQuery] StudentQueryParameters queryParameters
)
{
    var result=  await _studentService.GetAllAsync(queryParameters);
    return Ok(result);
}

[HttpGet("{id}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<StudentResponseDto>> GetStudentById(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        return Ok(student);
    }

[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<StudentResponseDto>> CreateStudent(StudentCreateRequestDto request)
    {
        var createdStudent = await _studentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, createdStudent);
    }

[HttpPut("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> UpdateStudent(int id, StudentUpdateRequestDto request)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }
        await _studentService.UpdateAsync(request);
        return NoContent();
    }

[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> DeleteStudent(int id)
    {
        await _studentService.DeleteAsync(id);
        return NoContent();
    }

}

