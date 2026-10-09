using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class AdminRepository
    {
        public Admin GetByUsername(string username)
        {
            string sql = "SELECT * FROM Admin WHERE Username = @Username AND IsActive = 1";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[]
            {
                DatabaseHelper.Param("@Username", username)
            });
            if (dt.Rows.Count == 0) return null;
            return MapAdmin(dt.Rows[0]);
        }

        public Admin GetByID(int id)
        {
            string sql = "SELECT * FROM Admin WHERE AdminID = @ID";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[]
            {
                DatabaseHelper.Param("@ID", id)
            });
            if (dt.Rows.Count == 0) return null;
            return MapAdmin(dt.Rows[0]);
        }

        public List<Admin> GetAll()
        {
            var list = new List<Admin>();
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Admin ORDER BY CreatedDate DESC");
            foreach (DataRow row in dt.Rows)
                list.Add(MapAdmin(row));
            return list;
        }

        public bool Update(Admin admin)
        {
            string sql = @"UPDATE Admin SET FullName=@FullName, Email=@Email WHERE AdminID=@AdminID";
            int rows = DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                DatabaseHelper.Param("@FullName", admin.FullName),
                DatabaseHelper.Param("@Email", admin.Email),
                DatabaseHelper.Param("@AdminID", admin.AdminID)
            });
            return rows > 0;
        }

        public bool UpdatePassword(int adminId, string hashedPassword)
        {
            string sql = "UPDATE Admin SET Password=@Password WHERE AdminID=@AdminID";
            return DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                DatabaseHelper.Param("@Password", hashedPassword),
                DatabaseHelper.Param("@AdminID", adminId)
            }) > 0;
        }

        public void LogActivity(string userType, int userId, string action)
        {
            string sql = "INSERT INTO ActivityLog (UserType, UserID, Action) VALUES (@UserType, @UserID, @Action)";
            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                DatabaseHelper.Param("@UserType", userType),
                DatabaseHelper.Param("@UserID", userId),
                DatabaseHelper.Param("@Action", action)
            });
        }

        public DataTable GetActivityLogs(int top = 50)
        {
            return DatabaseHelper.ExecuteQuery($"SELECT TOP {top} * FROM ActivityLog ORDER BY LogDate DESC");
        }

        private Admin MapAdmin(DataRow row)
        {
            return new Admin
            {
                AdminID = Convert.ToInt32(row["AdminID"]),
                Username = row["Username"].ToString(),
                Password = row["Password"].ToString(),
                FullName = row["FullName"].ToString(),
                Email = row["Email"].ToString(),
                Role = row["Role"].ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
