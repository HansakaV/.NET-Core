using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.DTOs;
using StudentManagement.API.Models;

namespace StudentManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentController : ControllerBase
{
    private readonly Data.AppDBContext _context;
    public StudentController(Data.AppDBContext context)
    {
        _context = context;
        
    }

[HttpGet]
public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
{
    return await _context.Students.ToListAsync();
}

[HttpGet("{id}")]
public async Task<ActionResult<StudentResponseDto>> GetStudentById(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }
         var response  = new StudentResponseDto
         {
             Id = student.Id,
             Name = student.Name,
             Email = student.Email,
             Course = student.Course,
             Phone = student.Phone
         };
         return Ok(response);
    }

[HttpPost]
public async Task<ActionResult<StudentResponseDto>> CreateStudent(StudentCreateRequestDto request)
{
    var student  = new Student
    {
        Name = request.Name,
        Email = request.Email,
        Course = request.Course,
        Phone = request.Phone
    };
    _context.Students.Add(student);
    await _context.SaveChangesAsync();

    var response = new StudentResponseDto
    {
        Id = student.Id,
        Name = student.Name,
        Email = student.Email,
        Course = student.Course,
        Phone = student.Phone
    };

    return CreatedAtAction(nameof(GetStudentById),
        new { id = student.Id }, 
        response);
}

}

