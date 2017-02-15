using AJG.DW.SecurityUtility.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AJG.DW.SecurityUtility.Web.Helpers
{
    public static class dbcontext
    {
        public static List<BusinessUnitDWAccess> DefaultSearchBusinessUnit(string searchText)
        {
            var dwconnectionString = ConfigurationManager.ConnectionStrings["dwdb"].ConnectionString;
            var command = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_utl_security_PnLAccess_Get"
            };

            var unitCodeParam = command.Parameters.Add("@UserName", SqlDbType.NVarChar);
            unitCodeParam.Value = searchText;
            return GetUsers(command, dwconnectionString);
        }

        public static List<BranchDWAccess> DefaultSearchBranch(string searchText)
        {
            var dwconnectionString = ConfigurationManager.ConnectionStrings["dwdb"].ConnectionString;
            var command = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_utl_security_BranchAccess_Get"
            };

            var unitCodeParam = command.Parameters.Add("@UserName", SqlDbType.NVarChar);
            unitCodeParam.Value = searchText;
            return GetUsersBranchAccess(command, dwconnectionString);
        }

        public static int UpdateBranch(string username, string[] branchCodes)
        {
            var dwconnectionString = ConfigurationManager.ConnectionStrings["dwdb"].ConnectionString;
            var command = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_utl_security_BranchAccess_Set"
            };

            var usernameParam = command.Parameters.Add("@UserName", SqlDbType.NVarChar);
            var unitCodeParam = command.Parameters.Add("@BranchCode", SqlDbType.NVarChar);

            if (branchCodes != null)
                unitCodeParam.Value = string.Join(",", branchCodes);
            else
                unitCodeParam.Value = "";

            usernameParam.Value = username;

            using (SqlConnection cn = new SqlConnection(dwconnectionString))
            {
                command.Connection = cn;
                cn.Open();
                return command.ExecuteNonQuery();
            }
        }


     
        public static int UpdateBusinessUnit(string username, string[] buCodes)
        {
            var dwconnectionString = ConfigurationManager.ConnectionStrings["dwdb"].ConnectionString;
            var command = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_utl_security_PnLAccess_Set"
            };

            var usernameParam = command.Parameters.Add("@username", SqlDbType.NVarChar);
            var unitCodeParam = command.Parameters.Add("@bucodes", SqlDbType.NVarChar);

            if (buCodes != null)
                unitCodeParam.Value = string.Join(",", buCodes);
            else
                unitCodeParam.Value = "";

            usernameParam.Value = username;

            using (SqlConnection cn = new SqlConnection(dwconnectionString))
            {
                command.Connection = cn;
                cn.Open();
                return command.ExecuteNonQuery();
            }
        }
        private static List<BranchDWAccess> GetUsersBranchAccess(SqlCommand cmd, string connectionString)
        {
            var permissions = new List<BranchDWAccess>();


            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cmd.Connection = cn;
                cn.Open();

                DataSet ds = new DataSet();
                DataTable table = new DataTable();
                table.Load(cmd.ExecuteReader());
                ds.Tables.Add(table);

                foreach (DataRow dr in table.Rows)
                {
                    var user = new BranchDWAccess
                    {
                        RegionName = dr["RegionName"].ToString(),
                        BranchName = dr["BranchName"].ToString(),
                        BranchCode = dr["BranchCode"].ToString(),
                        HasAccess = ((dr["access"].ToString() == "1"))
                    };
                    permissions.Add(user);
                }
                cn.Close();
            }
            return permissions;
        }

        private static List<BusinessUnitDWAccess> GetUsers(SqlCommand cmd, string connectionString)
        {
            var permissions = new List<BusinessUnitDWAccess>();


            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cmd.Connection = cn;
                cn.Open();

                DataSet ds = new DataSet();
                DataTable table = new DataTable();
                table.Load(cmd.ExecuteReader());
                ds.Tables.Add(table);

                foreach (DataRow dr in table.Rows)
                {
                    var user = new BusinessUnitDWAccess
                    {
                        Region = dr["RegionFullDesc"].ToString(),
                        BusinessUnitDescription = dr["bufulldesc"].ToString(),
                        BusinessUnitCode = int.Parse(dr["bucode"].ToString()),
                        HasAccess = ((dr["access"].ToString() == "1"))

                    };

                    permissions.Add(user);
                }

                cn.Close();
            }
            return permissions;
        }
    }
}