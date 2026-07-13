using System.Data;
using FluentValidation;
using StudentManagement.API.DTOs;

namespace StudentManagement.API.Validators.Students
{
    public class StudentCreateRequestValidator : AbstractValidator<StudentCreateRequestDto>
    {
        public StudentCreateRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is Requird")
                .MaximumLength(25).WithMessage("Name Cannot Excedd 25 Characters");
            
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email Address Requird")
                .EmailAddress().WithMessage("Email Address Requird");
            
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone Number Requird")
                .Matches(@"^(07\d{8}|947\d{8})$").WithMessage("Wrong Phone Number use 071xxxxxxxx or 947xxxxxxxx");

            RuleFor(x => x.Course)
                .IsInEnum().WithMessage("invalid Course Selected");

        }
    }
}