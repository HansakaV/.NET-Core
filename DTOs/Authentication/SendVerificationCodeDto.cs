using System.ComponentModel.DataAnnotations;

namespace StudentManagement.API.DTOs.Authentication
{
    public class SendVerificationCodeDto
    {
        [EmailAddress]
        public string Email{get;set;} = string.Empty;
    }
}