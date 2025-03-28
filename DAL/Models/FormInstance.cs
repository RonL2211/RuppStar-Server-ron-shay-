namespace FinalProject.DAL.Models
{
    public class FormInstance
    {
        public int InstanceId { get; set; }
        public int FormId { get; set; }
        public string UserID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CurrentStage { get; set; }
        public decimal? TotalScore { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string Comments { get; set; }
    }
}
