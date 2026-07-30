namespace StudentManagement.API.DTOs.Authentication
{
    public class AuthenticationResultDto
    {  
        public string AccessToken {get;set;} = string.Empty;
        public string RefreshToken {get;set;} = string.Empty;
        public DateTime AccessTokenExpiresAt {get;set;}
        public UserResponseDto User {get;set;} = null!;
    }
}