namespace StudentManagement.API.Models
{
    public class ProccedRequest : BaseEntity
    {
        public string IdempotencyKey {get;set;} = string.Empty;
        public string ResponsePayload {get;set;} = string.Empty;
        public DateTime ProcessedAt {get;set;} = DateTime.UtcNow;
    }
}