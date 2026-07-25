namespace StudentManagement.API.Models
{
    public class RefreshToken : BaseEntity
    {
        public int Id {get;set;}
        public string TokenHash {get;set;} = string.Empty;
        public DateTime ExpiresAt {get;set;}
        public DateTime? RevokedAt {get;set;}
        //Hash New Token In Rotation
        public string? ReplacedByTokenHash {get;set;}
        public string? CreatedByIp {get;set;}
        public string? RevoekdByIp {get;set;}
        //foreign Refernce
        public int UserId {get;set;}
        public User user {get;set;} = null!;
        //computed Properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool ISRevoked => RevokedAt is not null;
        public bool IsActive => !IsExpired && !ISRevoked;
        
    

    }
}