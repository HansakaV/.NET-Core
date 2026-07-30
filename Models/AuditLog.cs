namespace StudentManagement.API.Models
{
    public class AuditLog :BaseEntity
    {
        public int Id {get;set;}
        public string Action {get;set;} = string.Empty;
        public string EntityName {get;set;} = string.Empty;
        public string Details {get;set;} = string.Empty;
        public string? CreatedBy {get;set;}
    }
}