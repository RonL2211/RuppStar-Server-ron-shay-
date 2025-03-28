namespace FinalProject.DAL.Models
{
    public class FormSection
    {
        public int SectionID { get; set; }
        public int FormId { get; set; }
        public int? ParentSectionID { get; set; }
        public byte Level { get; set; }
        public int OrderIndex { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Explanation { get; set; }
        public decimal? MaxPoints { get; set; }
        public int? ResponsibleEntity { get; set; }
        public string ResponsiblePerson { get; set; }
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public int? MaxOccurrences { get; set; }
    }
}
