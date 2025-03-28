using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class SectionFieldRepository : DBServices
    {
        public SectionFieldRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<SectionField> GetFieldsBySectionId(int sectionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", sectionId }
            };

            List<SectionField> fieldList = new List<SectionField>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFieldsBySectionId", paramDic);

                while (dataReader.Read())
                {
                    SectionField field = new SectionField
                    {
                        FieldID = Convert.ToInt32(dataReader["FieldID"]),
                        SectionID = dataReader["SectionID"] != DBNull.Value ? Convert.ToInt32(dataReader["SectionID"]) : null,
                        FieldName = dataReader["FieldName"].ToString(),
                        FieldLabel = dataReader["FieldLabel"].ToString(),
                        FieldType = dataReader["FieldType"].ToString(),
                        IsRequired = Convert.ToBoolean(dataReader["IsRequired"]),
                        DefaultValue = dataReader["DefaultValue"].ToString(),
                        Placeholder = dataReader["Placeholder"].ToString(),
                        HelpText = dataReader["HelpText"].ToString(),
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        IsVisible = Convert.ToBoolean(dataReader["IsVisible"]),
                        MaxLength = dataReader["MaxLength"] != DBNull.Value ? Convert.ToInt32(dataReader["MaxLength"]) : null,
                        MinValue = dataReader["MinValue"] != DBNull.Value ? Convert.ToDecimal(dataReader["MinValue"]) : null,
                        MaxValue = dataReader["MaxValue"] != DBNull.Value ? Convert.ToDecimal(dataReader["MaxValue"]) : null,
                        ScoreCalculationRule = dataReader["ScoreCalculationRule"].ToString(),
                        IsActive = Convert.ToBoolean(dataReader["IsActive"])
                    };
                    fieldList.Add(field);
                }

                return fieldList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SectionField GetFieldById(int fieldId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FieldId", fieldId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFieldById", paramDic);
                SectionField field = null;

                if (dataReader.Read())
                {
                    field = new SectionField
                    {
                        FieldID = Convert.ToInt32(dataReader["FieldID"]),
                        SectionID = dataReader["SectionID"] != DBNull.Value ? Convert.ToInt32(dataReader["SectionID"]) : null,
                        FieldName = dataReader["FieldName"].ToString(),
                        FieldLabel = dataReader["FieldLabel"].ToString(),
                        FieldType = dataReader["FieldType"].ToString(),
                        IsRequired = Convert.ToBoolean(dataReader["IsRequired"]),
                        DefaultValue = dataReader["DefaultValue"].ToString(),
                        Placeholder = dataReader["Placeholder"].ToString(),
                        HelpText = dataReader["HelpText"].ToString(),
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        IsVisible = Convert.ToBoolean(dataReader["IsVisible"]),
                        MaxLength = dataReader["MaxLength"] != DBNull.Value ? Convert.ToInt32(dataReader["MaxLength"]) : null,
                        MinValue = dataReader["MinValue"] != DBNull.Value ? Convert.ToDecimal(dataReader["MinValue"]) : null,
                        MaxValue = dataReader["MaxValue"] != DBNull.Value ? Convert.ToDecimal(dataReader["MaxValue"]) : null,
                        ScoreCalculationRule = dataReader["ScoreCalculationRule"].ToString(),
                        IsActive = Convert.ToBoolean(dataReader["IsActive"])
                    };
                }

                return field;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddField(SectionField field)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", field.SectionID },
                { "@FieldName", field.FieldName },
                { "@FieldLabel", field.FieldLabel },
                { "@FieldType", field.FieldType },
                { "@IsRequired", field.IsRequired },
                { "@DefaultValue", field.DefaultValue },
                { "@Placeholder", field.Placeholder },
                { "@HelpText", field.HelpText },
                { "@OrderIndex", field.OrderIndex },
                { "@IsVisible", field.IsVisible },
                { "@MaxLength", field.MaxLength },
                { "@MinValue", field.MinValue },
                { "@MaxValue", field.MaxValue },
                { "@ScoreCalculationRule", field.ScoreCalculationRule },
                { "@IsActive", field.IsActive }
            };

            try
            {
                int fieldId = Convert.ToInt32(ExecuteScalar("spAddField", paramDic));
                return fieldId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateField(SectionField field)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FieldId", field.FieldID },
                { "@FieldName", field.FieldName },
                { "@FieldLabel", field.FieldLabel },
                { "@FieldType", field.FieldType },
                { "@IsRequired", field.IsRequired },
                { "@DefaultValue", field.DefaultValue },
                { "@Placeholder", field.Placeholder },
                { "@HelpText", field.HelpText },
                { "@OrderIndex", field.OrderIndex },
                { "@IsVisible", field.IsVisible },
                { "@MaxLength", field.MaxLength },
                { "@MinValue", field.MinValue },
                { "@MaxValue", field.MaxValue },
                { "@ScoreCalculationRule", field.ScoreCalculationRule },
                { "@IsActive", field.IsActive }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdateField", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DeleteField(int fieldId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FieldId", fieldId }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spDeleteField", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FieldOption> GetFieldOptions(int fieldId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FieldId", fieldId }
            };

            List<FieldOption> optionList = new List<FieldOption>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFieldOptions", paramDic);

                while (dataReader.Read())
                {
                    FieldOption option = new FieldOption
                    {
                        OptionID = Convert.ToInt32(dataReader["OptionID"]),
                        FieldID = Convert.ToInt32(dataReader["FieldID"]),
                        OptionValue = dataReader["OptionValue"].ToString(),
                        OptionLabel = dataReader["OptionLabel"].ToString(),
                        ScoreValue = dataReader["ScoreValue"] != DBNull.Value ? Convert.ToDecimal(dataReader["ScoreValue"]) : null,
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        IsDefault = Convert.ToBoolean(dataReader["IsDefault"])
                    };
                    optionList.Add(option);
                }

                return optionList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddFieldOption(FieldOption option)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FieldId", option.FieldID },
                { "@OptionValue", option.OptionValue },
                { "@OptionLabel", option.OptionLabel },
                { "@ScoreValue", option.ScoreValue },
                { "@OrderIndex", option.OrderIndex },
                { "@IsDefault", option.IsDefault }
            };

            try
            {
                int optionId = Convert.ToInt32(ExecuteScalar("spAddFieldOption", paramDic));
                return optionId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateFieldOption(FieldOption option)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@OptionId", option.OptionID },
                { "@OptionValue", option.OptionValue },
                { "@OptionLabel", option.OptionLabel },
                { "@ScoreValue", option.ScoreValue },
                { "@OrderIndex", option.OrderIndex },
                { "@IsDefault", option.IsDefault }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdateFieldOption", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public FieldOption GetFieldOptionById(int optionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
    {
        { "@OptionId", optionId }
    };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFieldOptionById", paramDic);
                FieldOption option = null;

                if (dataReader.Read())
                {
                    option = new FieldOption
                    {
                        OptionID = Convert.ToInt32(dataReader["OptionID"]),
                        FieldID = Convert.ToInt32(dataReader["FieldID"]),
                        OptionValue = dataReader["OptionValue"].ToString(),
                        OptionLabel = dataReader["OptionLabel"].ToString(),
                        ScoreValue = dataReader["ScoreValue"] != DBNull.Value ? Convert.ToDecimal(dataReader["ScoreValue"]) : null,
                        OrderIndex = Convert.ToInt32(dataReader["OrderIndex"]),
                        IsDefault = Convert.ToBoolean(dataReader["IsDefault"])
                    };
                }

                return option;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DeleteFieldOption(int optionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@OptionId", optionId }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spDeleteFieldOption", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}