namespace FinalProject.DAL.Models
{
    public class SectionPermission
    {
        public int PermissionId { get; set; }
        public int SectionID { get; set; }
        public string ResponsiblePerson { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanEvaluate { get; set; }
    }
}
