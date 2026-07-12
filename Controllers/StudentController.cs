using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.DTOs;
using StudentManagement.API.Interfaces;

namespace StudentManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
        
    }

[HttpGet]
public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetStudents()
{
    return await _studentService.GetAllAsync();
}

[HttpGet("{id}")]
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
public async Task<ActionResult<StudentResponseDto>> CreateStudent(StudentCreateRequestDto request)
    {
        var createdStudent = await _studentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, createdStudent);
    }

[HttpPut("{id}")]
public async Task<IActionResult> UpdateStudent(int id, StudentUpdateRequestDto request)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            await _studentService.UpdateAsync(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteStudent(int id)
    {
        try
        {
            await _studentService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

}

