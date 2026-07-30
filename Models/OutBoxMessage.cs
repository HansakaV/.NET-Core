namespace StudentManagement.API.Models
{
    public class OutBoxMessage : BaseEntity
    {
        public Guid Id {get;set;} = Guid.NewGuid();
        public string Type {get;set;} = string.Empty;
        public string Payload {get;set;} = string.Empty;
        public DateTime OccuredAt {get;set;} 
        public DateTime? ProcessedAt {get;set;}
        public string? Error {get;set;}
        public int RetryCount {get;set;}
    }
}