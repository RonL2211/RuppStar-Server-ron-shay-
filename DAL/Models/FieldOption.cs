namespace FinalProject.DAL.Models
{
    public class FieldOption
    {
        public int OptionID { get; set; }
        public int FieldID { get; set; }
        public string OptionValue { get; set; }
        public string OptionLabel { get; set; }
        public decimal? ScoreValue { get; set; }
        public int OrderIndex { get; set; }
        public bool IsDefault { get; set; }
    }
}
