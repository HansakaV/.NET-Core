using System.ComponentModel.DataAnnotations;

namespace StudentManagement.API.DTOs.Students
{
    public class StudentQueryParameters
    {
        [Range(1,int.MaxValue)]
         public int page {get;set;} = 1;
        [Range(1,100)]
        public int pageSize {get;set;} = 10;
        public string? SearchTerm{get;set;}
        public string? Sortby{get;set;} = "id";
        public bool IsDecending{get;set;} = false;
        public int? CourseId{get;set;}
    }
}