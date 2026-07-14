using System.Globalization;
using Microsoft.AspNetCore.SignalR;

namespace StudentManagement.API.Models
{
    public class User
    {
        public int Id{get;set;}
        public string Name{get;set;} = string.Empty;
        public string Email{get;set;} = string.Empty;
        public string PasswordHash{get;set;} = string.Empty;
        public string Role{get;set;} = "User";
        public DateTime CreatedAt{get; set;} = DateTime.UtcNow;
        public string? VerificationCode{get; set;} 
        public DateTime? VerificationCodeExpiry{get; set;}

    }
}