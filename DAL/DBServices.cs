using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FinalProject.DAL
{
    public class DBServices
    {
        private readonly IConfiguration _configuration;

        public DBServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection Connect(string connectionStringName = "myProjDB")
        {
            string connectionString = _configuration.GetConnectionString(connectionStringName);
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            return con;
        }

        public SqlCommand CreateCommandWithStoredProcedure(string spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = spName;
            cmd.CommandTimeout = 10;
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramDic != null)
            {
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return cmd;
        }

        public int ExecuteNonQuery(string spName, Dictionary<string, object> paramDic)
        {
            SqlConnection con = null;
            try
            {
                con = Connect("myProjDB");
                SqlCommand cmd = CreateCommandWithStoredProcedure(spName, con, paramDic);
                int numAffected = cmd.ExecuteNonQuery();
                return numAffected;
            }
            catch (Exception ex)
            {
                // לוג שגיאות
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
        }

        public object ExecuteScalar(string spName, Dictionary<string, object> paramDic)
        {
            SqlConnection con = null;
            try
            {
                con = Connect("myProjDB");
                SqlCommand cmd = CreateCommandWithStoredProcedure(spName, con, paramDic);
                object scalar = cmd.ExecuteScalar();
                return scalar;
            }
            catch (Exception ex)
            {
                // לוג שגיאות
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
        }

        public SqlDataReader ExecuteReader(string spName, Dictionary<string, object> paramDic)
        {
            SqlConnection con = null;
            try
            {
                con = Connect("myProjDB");
                SqlCommand cmd = CreateCommandWithStoredProcedure(spName, con, paramDic);
                SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception ex)
            {
                if (con != null)
                {
                    con.Close();
                }
                // לוג שגיאות
                throw ex;
            }
        }
    }
}