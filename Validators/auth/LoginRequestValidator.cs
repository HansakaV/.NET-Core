
using FluentValidation;
using StudentManagement.API.DTOs.Authentication;

namespace StudentManagement.API.Validators.auth
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}