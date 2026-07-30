namespace StudentManagement.API.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public string AccessToken{get;set;} = string.Empty;
        public DateTime AccessTokenExpireAt {get;set;}
        public UserResponseDto User {get;set;} = null!;
    }
}