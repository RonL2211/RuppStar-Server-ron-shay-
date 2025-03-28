namespace FinalProject.DAL.Models
{
    public class Form
    {
        public int FormID { get; set; }
        public string FormName { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public string AcademicYear { get; set; }
        public char? Semester { get; set; }
        public DateTime? StartDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
    }
}
