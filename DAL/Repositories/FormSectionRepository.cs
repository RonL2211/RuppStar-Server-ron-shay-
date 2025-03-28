using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class FormSectionRepository : DBServices
    {
        public FormSectionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<FormSection> GetSectionsByFormId(int formId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", formId }
            };

            List<FormSection> sectionList = new List<FormSection>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetSectionsByFormId", paramDic);

                while (dataReader.Read())
                {
                    FormSection section = new FormSection
                    {
                        SectionID = Convert.ToInt32(dataReader["SectionID"]),
                        FormId = Convert.ToInt32(dataReader["formId"]),
                        ParentSectionID = dataReader["ParentSectionID"] != DBNull.Value ? Convert.ToInt32(dataReader["ParentSectionID"]) : null,
                        Level = Convert.ToByte(dataReader["Level"]),
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        Title = dataReader["Title"].ToString(),
                        Description = dataReader["Description"].ToString(),
                        Explanation = dataReader["Explanation"].ToString(),
                        MaxPoints = dataReader["MaxPoints"] != DBNull.Value ? Convert.ToDecimal(dataReader["MaxPoints"]) : null,
                        ResponsibleEntity = dataReader["ResponsibleEntity"] != DBNull.Value ? Convert.ToInt32(dataReader["ResponsibleEntity"]) : null,
                        ResponsiblePerson = dataReader["ResposiblePerson"]?.ToString(),
                        IsRequired = Convert.ToBoolean(dataReader["IsRequired"]),
                        IsVisible = Convert.ToBoolean(dataReader["IsVisible"]),
                        MaxOccurrences = dataReader["MaxOccurrences"] != DBNull.Value ? Convert.ToInt32(dataReader["MaxOccurrences"]) : null
                    };
                    sectionList.Add(section);
                }

                return sectionList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FormSection GetSectionById(int sectionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", sectionId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetSectionById", paramDic);
                FormSection section = null;

                if (dataReader.Read())
                {
                    section = new FormSection
                    {
                        SectionID = Convert.ToInt32(dataReader["SectionID"]),
                        FormId = Convert.ToInt32(dataReader["formId"]),
                        ParentSectionID = dataReader["ParentSectionID"] != DBNull.Value ? Convert.ToInt32(dataReader["ParentSectionID"]) : null,
                        Level = Convert.ToByte(dataReader["Level"]),
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        Title = dataReader["Title"].ToString(),
                        Description = dataReader["Description"].ToString(),
                        Explanation = dataReader["Explanation"].ToString(),
                        MaxPoints = dataReader["MaxPoints"] != DBNull.Value ? Convert.ToDecimal(dataReader["MaxPoints"]) : null,
                        ResponsibleEntity = dataReader["ResponsibleEntity"] != DBNull.Value ? Convert.ToInt32(dataReader["ResponsibleEntity"]) : null,
                        ResponsiblePerson = dataReader["ResposiblePerson"]?.ToString(),
                        IsRequired = Convert.ToBoolean(dataReader["IsRequired"]),
                        IsVisible = Convert.ToBoolean(dataReader["IsVisible"]),
                        MaxOccurrences = dataReader["MaxOccurrences"] != DBNull.Value ? Convert.ToInt32(dataReader["MaxOccurrences"]) : null
                    };
                }

                return section;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddSection(FormSection section)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", section.FormId },
                { "@ParentSectionID", section.ParentSectionID },
                { "@Level", section.Level },
                { "@OrderIndex", section.OrderIndex },
                { "@Title", section.Title },
                { "@Description", section.Description },
                { "@Explanation", section.Explanation },
                { "@MaxPoints", section.MaxPoints },
                { "@ResponsibleEntity", section.ResponsibleEntity },
                { "@ResponsiblePerson", section.ResponsiblePerson },
                { "@IsRequired", section.IsRequired },
                { "@IsVisible", section.IsVisible },
                { "@MaxOccurrences", section.MaxOccurrences }
            };

            try
            {
                int sectionId = Convert.ToInt32(ExecuteScalar("spAddSection", paramDic));
                return sectionId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateSection(FormSection section)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", section.SectionID },
                { "@Title", section.Title },
                { "@Description", section.Description },
                { "@Explanation", section.Explanation },
                { "@MaxPoints", section.MaxPoints },
                { "@ResponsibleEntity", section.ResponsibleEntity },
                { "@ResponsiblePerson", section.ResponsiblePerson },
                { "@IsRequired", section.IsRequired },
                { "@IsVisible", section.IsVisible },
                { "@MaxOccurrences", section.MaxOccurrences },
                { "@OrderIndex", section.OrderIndex }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdateSection", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DeleteSection(int sectionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", sectionId }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spDeleteSection", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FormSection> GetChildSections(int parentSectionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@ParentSectionId", parentSectionId }
            };

            List<FormSection> sectionList = new List<FormSection>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetChildSections", paramDic);

                while (dataReader.Read())
                {
                    FormSection section = new FormSection
                    {
                        SectionID = Convert.ToInt32(dataReader["SectionID"]),
                        FormId = Convert.ToInt32(dataReader["formId"]),
                        ParentSectionID = dataReader["ParentSectionID"] != DBNull.Value ? Convert.ToInt32(dataReader["ParentSectionID"]) : null,
                        Level = Convert.ToByte(dataReader["Level"]),
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        Title = dataReader["Title"].ToString(),
                        Description = dataReader["Description"].ToString(),
                        Explanation = dataReader["Explanation"].ToString(),
                        MaxPoints = dataReader["MaxPoints"] != DBNull.Value ? Convert.ToDecimal(dataReader["MaxPoints"]) : null,
                        ResponsibleEntity = dataReader["ResponsibleEntity"] != DBNull.Value ? Convert.ToInt32(dataReader["ResponsibleEntity"]) : null,
                        ResponsiblePerson = dataReader["ResposiblePerson"]?.ToString(),
                        IsRequired = Convert.ToBoolean(dataReader["IsRequired"]),
                        IsVisible = Convert.ToBoolean(dataReader["IsVisible"]),
                        MaxOccurrences = dataReader["MaxOccurrences"] != DBNull.Value ? Convert.ToInt32(dataReader["MaxOccurrences"]) : null
                    };
                    sectionList.Add(section);
                }

                return sectionList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}