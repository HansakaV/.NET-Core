using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql.Internal;
using StudentManagement.API.Data;
using StudentManagement.API.DTOs.Enrollment;
using StudentManagement.API.Events;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Middlewares.Exceptions;
using StudentManagement.API.Models;

namespace StudentManagement.API.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDBContext _context;
        public EnrollmentService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<EnrollmentResponseDTO> EnrollStudentAsync(EnrollmentRequestDTO enrollmentRequest, CancellationToken cancellationToken)
        {
           if (string.IsNullOrWhiteSpace(enrollmentRequest.IdempotencyKey))
           {
                throw new ArgumentException("Idempotency Key Is required for this oprtation");
           }
           var strategy =  _context.Database.CreateExecutionStrategy();
           return await strategy.ExecuteAsync<EnrollmentResponseDTO>(async () =>
           {
               await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

               try
               {
                var exitingProcessRequest = await _context.ProccedRequests
                        .FirstOrDefaultAsync(process => process.IdempotencyKey == enrollmentRequest.IdempotencyKey, cancellationToken);
                if(exitingProcessRequest != null)
                   {
                       var cachedResponse = JsonSerializer.Deserialize<EnrollmentResponseDTO>(exitingProcessRequest.ResponsePayload);
                       return cachedResponse!;
                   }

                var students = await _context.Students
                        .FirstOrDefaultAsync(student => student.Id == enrollmentRequest.StudentId, cancellationToken);
                if(students == null) throw new StudentNotFoundException(enrollmentRequest.StudentId);

                var courses = await _context.Courses
                        .FirstOrDefaultAsync(course => course.CourseId == enrollmentRequest.CourseId, cancellationToken);
                if(courses ==null) throw new KeyNotFoundException($"Selected Course {enrollmentRequest.CourseId} Not Found");

                if(courses.AvailableSeats <= 0) throw new CourseFullException(courses.CourseId);
                var isEnrolled = await _context.Enrollments
                        .AnyAsync(enrollment => enrollment.StudentId == enrollmentRequest.StudentId && enrollment.CourseId == enrollmentRequest.CourseId, cancellationToken);

                if (isEnrolled)
                {
                    throw new DuplicateEnrollmentException(enrollmentRequest.StudentId , enrollmentRequest.CourseId);
                }

                var enrollment = new Enrollment
                {
                    StudentId = enrollmentRequest.StudentId,
                    CourseId = enrollmentRequest.CourseId,
                    EnrolledAt = DateTime.UtcNow
                };
                _context.Enrollments.Add(enrollment);
                courses.AvailableSeats -=1;

                var auditLog = new AuditLog
                {
                    Action = "STUDENT_ENROLLED",
                    EntityName = nameof(Enrollment),
                    Details = $"Studnt {students.Id} enrolled on Course {courses.CourseId}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);

                var intergrationEvent = new StudentEnrolledEvent
                {
                    StuedntId = students.Id,
                    CourseId = courses.CourseId,
                    EnrolledAt = enrollment.EnrolledAt
                };

                var outBoxMessaage = new OutBoxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = nameof(StudentEnrolledEvent),
                    Payload = JsonSerializer.Serialize(intergrationEvent),
                    OccuredAt = DateTime.UtcNow
                };
                _context.OutBoxMessages.Add(outBoxMessaage);

                var enrollmentResponse = new EnrollmentResponseDTO
                {
                    Id = enrollment.Id,
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    EnrolledAt = enrollment.EnrolledAt
                };

                var proccedRequest = new ProccedRequest
                {
                    IdempotencyKey = enrollmentRequest.IdempotencyKey,
                    ResponsePayload = JsonSerializer.Serialize(enrollmentResponse),
                    ProcessedAt = DateTime.UtcNow
                };

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return enrollmentResponse;
                
               }
               catch 
               {
                await transaction.RollbackAsync(cancellationToken);
                throw;      
               }
           });
        }
    }
}