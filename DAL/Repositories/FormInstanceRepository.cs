using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class FormInstanceRepository : DBServices
    {
        public FormInstanceRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<FormInstance> GetInstancesByUserId(string userId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            List<FormInstance> instanceList = new List<FormInstance>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetInstancesByUserId", paramDic);

                while (dataReader.Read())
                {
                    FormInstance instance = new FormInstance
                    {
                        InstanceId = Convert.ToInt32(dataReader["InstanceId"]),
                        FormId = Convert.ToInt32(dataReader["FormId"]),
                        UserID = dataReader["UserID"].ToString(),
                        CreatedDate = dataReader["createdDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["createdDate"]) : null,
                        CurrentStage = dataReader["CurrentStage"].ToString(),
                        TotalScore = dataReader["TotalScore"] != DBNull.Value ? Convert.ToDecimal(dataReader["TotalScore"]) : null,
                        SubmissionDate = dataReader["SubmissionDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["SubmissionDate"]) : null,
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        Comments = dataReader["Comments"].ToString()
                    };
                    instanceList.Add(instance);
                }

                return instanceList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FormInstance GetInstanceById(int instanceId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@InstanceId", instanceId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetInstanceById", paramDic);
                FormInstance instance = null;

                if (dataReader.Read())
                {
                    instance = new FormInstance
                    {
                        InstanceId = Convert.ToInt32(dataReader["InstanceId"]),
                        FormId = Convert.ToInt32(dataReader["FormId"]),
                        UserID = dataReader["UserID"].ToString(),
                        CreatedDate = dataReader["createdDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["createdDate"]) : null,
                        CurrentStage = dataReader["CurrentStage"].ToString(),
                        TotalScore = dataReader["TotalScore"] != DBNull.Value ? Convert.ToDecimal(dataReader["TotalScore"]) : null,
                        SubmissionDate = dataReader["SubmissionDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["SubmissionDate"]) : null,
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        Comments = dataReader["Comments"].ToString()
                    };
                }

                return instance;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int CreateInstance(FormInstance instance)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", instance.FormId },
                { "@UserId", instance.UserID },
                { "@CurrentStage", instance.CurrentStage }
            };

            try
            {
                int instanceId = Convert.ToInt32(ExecuteScalar("spCreateInstance", paramDic));
                return instanceId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateInstanceStatus(int instanceId, string currentStage, string comments)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@InstanceId", instanceId },
                { "@CurrentStage", currentStage },
                { "@Comments", comments }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdateInstanceStatus", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int SubmitInstance(int instanceId, decimal totalScore)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@InstanceId", instanceId },
                { "@TotalScore", totalScore }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spSubmitInstance", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FormInstance> GetInstancesByFormId(int formId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FormId", formId }
            };

            List<FormInstance> instanceList = new List<FormInstance>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetInstancesByFormId", paramDic);

                while (dataReader.Read())
                {
                    FormInstance instance = new FormInstance
                    {
                        InstanceId = Convert.ToInt32(dataReader["InstanceId"]),
                        FormId = Convert.ToInt32(dataReader["FormId"]),
                        UserID = dataReader["UserID"].ToString(),
                        CreatedDate = dataReader["createdDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["createdDate"]) : null,
                        CurrentStage = dataReader["CurrentStage"].ToString(),
                        TotalScore = dataReader["TotalScore"] != DBNull.Value ? Convert.ToDecimal(dataReader["TotalScore"]) : null,
                        SubmissionDate = dataReader["SubmissionDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["SubmissionDate"]) : null,
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        Comments = dataReader["Comments"].ToString()
                    };
                    instanceList.Add(instance);
                }

                return instanceList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FormInstance> GetInstancesByStage(string stage)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@Stage", stage }
            };

            List<FormInstance> instanceList = new List<FormInstance>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetInstancesByStage", paramDic);

                while (dataReader.Read())
                {
                    FormInstance instance = new FormInstance
                    {
                        InstanceId = Convert.ToInt32(dataReader["InstanceId"]),
                        FormId = Convert.ToInt32(dataReader["FormId"]),
                        UserID = dataReader["UserID"].ToString(),
                        CreatedDate = dataReader["createdDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["createdDate"]) : null,
                        CurrentStage = dataReader["CurrentStage"].ToString(),
                        TotalScore = dataReader["TotalScore"] != DBNull.Value ? Convert.ToDecimal(dataReader["TotalScore"]) : null,
                        SubmissionDate = dataReader["SubmissionDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["SubmissionDate"]) : null,
                        LastModifiedDate = dataReader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["LastModifiedDate"]) : null,
                        Comments = dataReader["Comments"].ToString()
                    };
                    instanceList.Add(instance);
                }

                return instanceList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}