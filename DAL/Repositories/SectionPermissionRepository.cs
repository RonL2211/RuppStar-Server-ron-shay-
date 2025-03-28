using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class SectionPermissionRepository : DBServices
    {
        public SectionPermissionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<SectionPermission> GetSectionPermissions(int sectionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", sectionId }
            };

            List<SectionPermission> permissionList = new List<SectionPermission>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetSectionPermissions", paramDic);

                while (dataReader.Read())
                {
                    SectionPermission permission = new SectionPermission
                    {
                        PermissionId = Convert.ToInt32(dataReader["PremisionId"]),
                        SectionID = Convert.ToInt32(dataReader["SectionID"]),
                        ResponsiblePerson = dataReader["ResposiblePerson"].ToString(),
                        CanView = Convert.ToBoolean(dataReader["CanView"]),
                        CanEdit = Convert.ToBoolean(dataReader["CanEdit"]),
                        CanEvaluate = Convert.ToBoolean(dataReader["CanEvaluate"])
                    };
                    permissionList.Add(permission);
                }

                return permissionList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SectionPermission GetPermissionById(int permissionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PermissionId", permissionId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetPermissionById", paramDic);
                SectionPermission permission = null;

                if (dataReader.Read())
                {
                    permission = new SectionPermission
                    {
                        PermissionId = Convert.ToInt32(dataReader["PremisionId"]),
                        SectionID = Convert.ToInt32(dataReader["SectionID"]),
                        ResponsiblePerson = dataReader["ResposiblePerson"].ToString(),
                        CanView = Convert.ToBoolean(dataReader["CanView"]),
                        CanEdit = Convert.ToBoolean(dataReader["CanEdit"]),
                        CanEvaluate = Convert.ToBoolean(dataReader["CanEvaluate"])
                    };
                }

                return permission;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddSectionPermission(SectionPermission permission)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@SectionId", permission.SectionID },
                { "@ResponsiblePerson", permission.ResponsiblePerson },
                { "@CanView", permission.CanView },
                { "@CanEdit", permission.CanEdit },
                { "@CanEvaluate", permission.CanEvaluate }
            };

            try
            {
                int permissionId = Convert.ToInt32(ExecuteScalar("spAddSectionPermission", paramDic));
                return permissionId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdateSectionPermission(SectionPermission permission)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PermissionId", permission.PermissionId },
                { "@CanView", permission.CanView },
                { "@CanEdit", permission.CanEdit },
                { "@CanEvaluate", permission.CanEvaluate }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdateSectionPermission", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DeleteSectionPermission(int permissionId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PermissionId", permissionId }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spDeleteSectionPermission", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}