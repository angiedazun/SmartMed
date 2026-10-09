using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class CustomerRepository
    {
        public Customer GetByEmail(string email)
        {
            string sql = "SELECT * FROM Customer WHERE Email = @Email";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@Email", email) });
            return dt.Rows.Count > 0 ? MapCustomer(dt.Rows[0]) : null;
        }

        public Customer GetByID(int id)
        {
            string sql = "SELECT * FROM Customer WHERE CustomerID = @ID";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@ID", id) });
            return dt.Rows.Count > 0 ? MapCustomer(dt.Rows[0]) : null;
        }

        public List<Customer> GetAll(string search = "")
        {
            var list = new List<Customer>();
            string sql = @"SELECT * FROM Customer WHERE
                (FirstName + ' ' + LastName LIKE @Search OR Email LIKE @Search OR Phone LIKE @Search)
                ORDER BY RegisteredDate DESC";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[]
            {
                DatabaseHelper.Param("@Search", $"%{search}%")
            });
            foreach (DataRow row in dt.Rows)
                list.Add(MapCustomer(row));
            return list;
        }

        public int Create(Customer c)
        {
            string sql = @"INSERT INTO Customer (FirstName, LastName, Email, Phone, Address, Password, ProfileImage, Status)
                           VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @Password, @ProfileImage, 'Active')";
            return DatabaseHelper.ExecuteInsertGetId(sql, new[]
            {
                DatabaseHelper.Param("@FirstName", c.FirstName),
                DatabaseHelper.Param("@LastName", c.LastName),
                DatabaseHelper.Param("@Email", c.Email),
                DatabaseHelper.Param("@Phone", c.Phone ?? ""),
                DatabaseHelper.Param("@Address", c.Address ?? ""),
                DatabaseHelper.Param("@Password", c.Password),
                DatabaseHelper.ParamNullable("@ProfileImage", c.ProfileImage)
            });
        }

        public bool Update(Customer c)
        {
            string sql = @"UPDATE Customer SET FirstName=@FirstName, LastName=@LastName,
                Email=@Email, Phone=@Phone, Address=@Address, ProfileImage=@ProfileImage
                WHERE CustomerID=@ID";
            return DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                DatabaseHelper.Param("@FirstName", c.FirstName),
                DatabaseHelper.Param("@LastName", c.LastName),
                DatabaseHelper.Param("@Email", c.Email),
                DatabaseHelper.Param("@Phone", c.Phone ?? ""),
                DatabaseHelper.Param("@Address", c.Address ?? ""),
                DatabaseHelper.ParamNullable("@ProfileImage", c.ProfileImage),
                DatabaseHelper.Param("@ID", c.CustomerID)
            }) > 0;
        }

        public bool UpdatePassword(int id, string hashedPwd)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Customer SET Password=@Pwd WHERE CustomerID=@ID",
                new[] { DatabaseHelper.Param("@Pwd", hashedPwd), DatabaseHelper.Param("@ID", id) }) > 0;
        }

        public bool SetStatus(int id, string status)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Customer SET Status=@Status WHERE CustomerID=@ID",
                new[] { DatabaseHelper.Param("@Status", status), DatabaseHelper.Param("@ID", id) }) > 0;
        }

        public int GetTotalCount()
        {
            var result = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Customer");
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private Customer MapCustomer(DataRow row)
        {
            return new Customer
            {
                CustomerID = Convert.ToInt32(row["CustomerID"]),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Email = row["Email"].ToString(),
                Phone = row["Phone"].ToString(),
                Address = row["Address"].ToString(),
                Password = row["Password"].ToString(),
                ProfileImage = row["ProfileImage"] == DBNull.Value ? null : row["ProfileImage"].ToString(),
                RegisteredDate = Convert.ToDateTime(row["RegisteredDate"]),
                Status = row["Status"].ToString()
            };
        }
    }
}
