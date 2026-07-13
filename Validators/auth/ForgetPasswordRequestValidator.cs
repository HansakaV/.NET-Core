using FluentValidation;
using StudentManagement.API.DTOs.Authentication;

namespace StudentManagement.API.Validators.auth
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            
            RuleFor(x => x.VerificationCode)
                .NotEmpty()
                .MinimumLength(4);
            
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}