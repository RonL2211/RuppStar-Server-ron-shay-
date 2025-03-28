namespace FinalProject.DAL.Models
{
    public class SectionField
    {
        public int FieldID { get; set; }
        public int? SectionID { get; set; }
        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public string HelpText { get; set; }
        public int OrderIndex { get; set; }
        public bool IsVisible { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string ScoreCalculationRule { get; set; }
        public bool IsActive { get; set; }
    }
}
