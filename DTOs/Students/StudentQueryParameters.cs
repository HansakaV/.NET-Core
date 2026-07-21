using System.ComponentModel.DataAnnotations;

namespace StudentManagement.API.DTOs.Students
{
    public class StudentQueryParameters
    {
        [Range(1,int.MaxValue)]
         public int page {get;set;} = 1;
        [Range(1,100)]
        public int pageSize {get;set;} = 10;
    }
}