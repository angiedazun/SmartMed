using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SmartMed.Data
{
    public static class DBConnection
    {
        private static string _connectionString = string.Empty;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                    _connectionString = BuildConnectionString();
                return _connectionString;
            }
        }

        private static string BuildConnectionString()
        {
            return "Server=.\\SQLEXPRESS;Database=SmartMedDB;Integrated Security=True;MultipleActiveResultSets=True;";
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void OpenConnection(SqlConnection conn)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
        }
    }
}
