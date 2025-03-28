using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class FormRepository : DBServices
    {
        public FormRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Form> GetAllForms()
        {
            List<Form> formList = new List<Form>();
            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetAllForms", null);

                while (dataReader.Read())
                {
                    Form form = new Form
                    {
                        FormID = Convert.ToInt32(dataReader["FormID"]),
                        FormName = dataReader["formName"].ToString(),
                        CreationDate = dataReader["creationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["creationDate"]) : null,
                        DueDate = dataReader["dueDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["dueDate"]) : null,
                        Description = dataReader["description"].ToString(),
                        Instructions = dataReader["instructions"].ToString(),
                        AcademicYear = dataReader["AcademicYear"].ToString(),
                        Semester = dataReader["Semester"] != DBNull.Value ? Convert.ToChar(dataReader["Semester"]) : null,
                        StartDate = dataReader["StartDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["StartDate"]) : null,
                        CreatedBy = dataReader["CreatedBy"].ToString(),
                        LastModifiedBy = dataReader["LastModifiedBy"]?.ToString(),
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                        IsPublished = Convert.ToBoolean(dataReader["IsPublished"])
                    };
                    formList.Add(form);
                }

                return formList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Form GetFormById(int formId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", formId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFormById", paramDic);
                Form form = null;

                if (dataReader.Read())
                {
                    form = new Form
                    {
                        FormID = Convert.ToInt32(dataReader["FormID"]),
                        FormName = dataReader["formName"].ToString(),
                        CreationDate = dataReader["creationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["creationDate"]) : null,
                        DueDate = dataReader["dueDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["dueDate"]) : null,
                        Description = dataReader["description"].ToString(),
                        Instructions = dataReader["instructions"].ToString(),
                        AcademicYear = dataReader["AcademicYear"].ToString(),
                        Semester = dataReader["Semester"] != DBNull.Value ? Convert.ToChar(dataReader["Semester"]) : null,
                        StartDate = dataReader["StartDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["StartDate"]) : null,
                        CreatedBy = dataReader["CreatedBy"].ToString(),
                        LastModifiedBy = dataReader["LastModifiedBy"]?.ToString(),
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                        IsPublished = Convert.ToBoolean(dataReader["IsPublished"])
                    };
                }

                return form;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddForm(Form form)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormName", form.FormName },
                { "@Description", form.Description },
                { "@Instructions", form.Instructions },
                { "@AcademicYear", form.AcademicYear },
                { "@Semester", form.Semester },
                { "@StartDate", form.StartDate },
                { "@DueDate", form.DueDate },
                { "@CreatedBy", form.CreatedBy },
                { "@IsActive", form.IsActive },
                { "@IsPublished", form.IsPublished }
            };

            try
            {
                int formId = Convert.ToInt32(ExecuteScalar("spAddForm", paramDic));
                return formId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateForm(Form form)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", form.FormID },
                { "@FormName", form.FormName },
                { "@Description", form.Description },
                { "@Instructions", form.Instructions },
                { "@AcademicYear", form.AcademicYear },
                { "@Semester", form.Semester },
                { "@StartDate", form.StartDate },
                { "@DueDate", form.DueDate },
                { "@LastModifiedBy", form.LastModifiedBy },
                { "@IsActive", form.IsActive },
                { "@IsPublished", form.IsPublished }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdateForm", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int PublishForm(int formId, string modifiedBy)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", formId },
                { "@LastModifiedBy", modifiedBy }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spPublishForm", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Form> GetFormsByAcademicYear(string academicYear)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@AcademicYear", academicYear }
            };

            List<Form> formList = new List<Form>();
            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFormsByAcademicYear", paramDic);

                while (dataReader.Read())
                {
                    Form form = new Form
                    {
                        FormID = Convert.ToInt32(dataReader["FormID"]),
                        FormName = dataReader["formName"].ToString(),
                        CreationDate = dataReader["creationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["creationDate"]) : null,
                        DueDate = dataReader["dueDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["dueDate"]) : null,
                        Description = dataReader["description"].ToString(),
                        Instructions = dataReader["instructions"].ToString(),
                        AcademicYear = dataReader["AcademicYear"].ToString(),
                        Semester = dataReader["Semester"] != DBNull.Value ? Convert.ToChar(dataReader["Semester"]) : null,
                        StartDate = dataReader["StartDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["StartDate"]) : null,
                        CreatedBy = dataReader["CreatedBy"].ToString(),
                        LastModifiedBy = dataReader["LastModifiedBy"]?.ToString(),
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                        IsPublished = Convert.ToBoolean(dataReader["IsPublished"])
                    };
                    formList.Add(form);
                }

                return formList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}