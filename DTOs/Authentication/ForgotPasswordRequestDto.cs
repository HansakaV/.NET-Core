using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.API.DTOs.Authentication
{
    public class ForgotPasswordRequestDto
    {
        public string Email{get;set;} = string.Empty;
        public string VerificationCode{get;set;} = string.Empty;
        public string NewPassword{get;set;} = string.Empty;
    }
}