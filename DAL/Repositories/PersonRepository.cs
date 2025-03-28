using FinalProject.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalProject.DAL.Repositories
{
    public class PersonRepository : DBServices
    {
        public PersonRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Person> GetAllPersons()
        {
            List<Person> personList = new List<Person>();
            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetAllPersons", null);

                while (dataReader.Read())
                {
                    Person person = new Person
                    {
                        PersonId = dataReader["PersonId"].ToString(),
                        FirstName = dataReader["firstName"].ToString(),
                        LastName = dataReader["lastName"].ToString(),
                        Email = dataReader["email"].ToString(),
                        DepartmentID = dataReader["DepartmentID"] != DBNull.Value ? Convert.ToInt32(dataReader["DepartmentID"]) : null,
                        FolderPath = dataReader["folderPath"].ToString(),
                        Username = dataReader["Username"].ToString(),
                        Password = dataReader["password"].ToString(),
                        Position = dataReader["Position"].ToString(),
                        IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                        CreatedDate = dataReader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["CreatedDate"]) : null
                    };
                    personList.Add(person);
                }

                return personList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Person GetPersonById(string personId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PersonId", personId }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetPersonById", paramDic);
                Person person = null;

                if (dataReader.Read())
                {
                    person = new Person
                    {
                        PersonId = dataReader["PersonId"].ToString(),
                        FirstName = dataReader["firstName"].ToString(),
                        LastName = dataReader["lastName"].ToString(),
                        Email = dataReader["email"].ToString(),
                        DepartmentID = dataReader["DepartmentID"] != DBNull.Value ? Convert.ToInt32(dataReader["DepartmentID"]) : null,
                        FolderPath = dataReader["folderPath"].ToString(),
                        Username = dataReader["Username"].ToString(),
                        Password = dataReader["password"].ToString(),
                        Position = dataReader["Position"].ToString(),
                        IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                        CreatedDate = dataReader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["CreatedDate"]) : null
                    };
                }

                return person;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Person GetPersonByUsername(string username)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetPersonByUsername", paramDic);
                Person person = null;

                if (dataReader.Read())
                {
                    person = new Person
                    {
                        PersonId = dataReader["PersonId"].ToString(),
                        FirstName = dataReader["firstName"].ToString(),
                        LastName = dataReader["lastName"].ToString(),
                        Email = dataReader["email"].ToString(),
                        DepartmentID = dataReader["DepartmentID"] != DBNull.Value ? Convert.ToInt32(dataReader["DepartmentID"]) : null,
                        FolderPath = dataReader["folderPath"].ToString(),
                        Username = dataReader["Username"].ToString(),
                        Password = dataReader["password"].ToString(),
                        Position = dataReader["Position"].ToString(),
                        IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                        CreatedDate = dataReader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["CreatedDate"]) : null
                    };
                }

                return person;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddPerson(Person person)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PersonId", person.PersonId },
                { "@FirstName", person.FirstName },
                { "@LastName", person.LastName },
                { "@Email", person.Email },
                { "@DepartmentID", person.DepartmentID },
                { "@FolderPath", person.FolderPath },
                { "@Username", person.Username },
                { "@Password", person.Password },
                { "@Position", person.Position },
                { "@IsActive", person.IsActive }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spAddPerson", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int UpdatePerson(Person person)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PersonId", person.PersonId },
                { "@FirstName", person.FirstName },
                { "@LastName", person.LastName },
                { "@Email", person.Email },
                { "@DepartmentID", person.DepartmentID },
                { "@FolderPath", person.FolderPath },
                { "@Username", person.Username },
                { "@Password", person.Password },
                { "@Position", person.Position },
                { "@IsActive", person.IsActive }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spUpdatePerson", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Role> GetPersonRoles(string personId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PersonId", personId }
            };

            List<Role> roles = new List<Role>();

            try
            {
                SqlDataReader dataReader = ExecuteReader("spGetPersonRoles", paramDic);

                while (dataReader.Read())
                {
                    Role role = new Role
                    {
                        RoleID = Convert.ToInt32(dataReader["RoleID"]),
                        RoleName = dataReader["RoleName"].ToString(),
                        Description = dataReader["Description"].ToString()
                    };
                    roles.Add(role);
                }

                return roles;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AssignRoleToPerson(string personId, int roleId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PersonId", personId },
                { "@RoleId", roleId }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spAssignRoleToPerson", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int RemoveRoleFromPerson(string personId, int roleId)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@PersonId", personId },
                { "@RoleId", roleId }
            };

            try
            {
                int numAffected = ExecuteNonQuery("spRemoveRoleFromPerson", paramDic);
                return numAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}