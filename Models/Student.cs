using Org.BouncyCastle.Pqc.Crypto.Falcon;

namespace StudentManagement.API.Models;
    public class Student : BaseEntity
{
    public int Id{get; set;}
    public string Name{get; set;} = string.Empty;
    public string Email{get; set;} = string.Empty;
    public string Phone{get; set;} = string.Empty;
    public int CourseId {get;set;}
    public Course Course{get;set;} =null!;

}
