using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class DepartmentRepository : DBServices
    {
        public DepartmentRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Department> GetAllDepartments()
        {
            List<Department> departmentList = new List<Department>();
            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetAllDepartments", null);

                while (dataReader.Read())
                {
                    Department department = new Department
                    {
                        DepartmentID = Convert.ToInt32(dataReader["DepartmentID"]),
                        DepartmentName = dataReader["DepartmentName"].ToString(),
                        FacultyId = dataReader["FacultyId"] != DBNull.Value ? Convert.ToInt32(dataReader["FacultyId"]) : null
                    };
                    departmentList.Add(department);
                }

                return departmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Department GetDepartmentById(int departmentId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@DepartmentId", departmentId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetDepartmentById", paramDic);
                Department department = null;

                if (dataReader.Read())
                {
                    department = new Department
                    {
                        DepartmentID = Convert.ToInt32(dataReader["DepartmentID"]),
                        DepartmentName = dataReader["DepartmentName"].ToString(),
                        FacultyId = dataReader["FacultyId"] != DBNull.Value ? Convert.ToInt32(dataReader["FacultyId"]) : null
                    };
                }

                return department;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Department> GetDepartmentsByFacultyId(int facultyId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FacultyId", facultyId }
            };

            List<Department> departmentList = new List<Department>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetDepartmentsByFacultyId", paramDic);

                while (dataReader.Read())
                {
                    Department department = new Department
                    {
                        DepartmentID = Convert.ToInt32(dataReader["DepartmentID"]),
                        DepartmentName = dataReader["DepartmentName"].ToString(),
                        FacultyId = dataReader["FacultyId"] != DBNull.Value ? Convert.ToInt32(dataReader["FacultyId"]) : null
                    };
                    departmentList.Add(department);
                }

                return departmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Faculty> GetAllFaculties()
        {
            List<Faculty> facultyList = new List<Faculty>();
            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetAllFaculties", null);

                while (dataReader.Read())
                {
                    Faculty faculty = new Faculty
                    {
                        FacultyID = Convert.ToInt32(dataReader["FacultyID"]),
                        FacultyName = dataReader["FacultyName"].ToString()
                    };
                    facultyList.Add(faculty);
                }

                return facultyList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Faculty GetFacultyById(int facultyId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@FacultyId", facultyId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetFacultyById", paramDic);
                Faculty faculty = null;

                if (dataReader.Read())
                {
                    faculty = new Faculty
                    {
                        FacultyID = Convert.ToInt32(dataReader["FacultyID"]),
                        FacultyName = dataReader["FacultyName"].ToString()
                    };
                }

                return faculty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}