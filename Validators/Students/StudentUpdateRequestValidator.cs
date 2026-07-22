
using FluentValidation;
using StudentManagement.API.DTOs;


namespace StudentManagement.API.Validators.Students
{
    public class StudentUpdateRequestValidator : AbstractValidator<StudentUpdateRequestDto>
    {
        public StudentUpdateRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("There is no selcted student for this opration");
            
            RuleFor(x => x.Name)
                .MaximumLength(25)
                .Unless(x => string.IsNullOrEmpty(x.Name))
                .WithMessage("Name Too long ");
            
            RuleFor(x => x.Email)
                .EmailAddress()
                .Unless(x => string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid Email Address");
            
            RuleFor(x => x.Phone)
                .Matches(@"^(07\d{8}|947\d{8})$")
                .Unless(x => string.IsNullOrEmpty(x.Phone))
                .WithMessage("Wrong Phone Number use 071xxxxxxxx or 947xxxxxxxx");
            
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .When(x => x.CourseId.HasValue)
                .WithMessage("Invalid Course");
        }
    }
}