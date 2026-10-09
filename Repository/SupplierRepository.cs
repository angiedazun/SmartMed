using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class SupplierRepository
    {
        public List<Supplier> GetAll(string search = "")
        {
            var list = new List<Supplier>();
            string sql = @"SELECT * FROM Supplier WHERE IsActive=1
                AND (SupplierName LIKE @Search OR Company LIKE @Search OR Phone LIKE @Search OR Email LIKE @Search)
                ORDER BY SupplierName";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@Search", $"%{search}%") });
            foreach (DataRow row in dt.Rows)
                list.Add(Map(row));
            return list;
        }

        public int Create(Supplier s)
        {
            return DatabaseHelper.ExecuteInsertGetId(
                "INSERT INTO Supplier (SupplierName, Company, Phone, Email, Address) VALUES (@Name, @Company, @Phone, @Email, @Address)",
                new[]
                {
                    DatabaseHelper.Param("@Name", s.SupplierName),
                    DatabaseHelper.Param("@Company", s.Company ?? ""),
                    DatabaseHelper.Param("@Phone", s.Phone ?? ""),
                    DatabaseHelper.Param("@Email", s.Email ?? ""),
                    DatabaseHelper.Param("@Address", s.Address ?? "")
                });
        }

        public bool Update(Supplier s)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Supplier SET SupplierName=@Name, Company=@Company, Phone=@Phone, Email=@Email, Address=@Address WHERE SupplierID=@ID",
                new[]
                {
                    DatabaseHelper.Param("@Name", s.SupplierName),
                    DatabaseHelper.Param("@Company", s.Company ?? ""),
                    DatabaseHelper.Param("@Phone", s.Phone ?? ""),
                    DatabaseHelper.Param("@Email", s.Email ?? ""),
                    DatabaseHelper.Param("@Address", s.Address ?? ""),
                    DatabaseHelper.Param("@ID", s.SupplierID)
                }) > 0;
        }

        public bool Delete(int id)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Supplier SET IsActive=0 WHERE SupplierID=@ID",
                new[] { DatabaseHelper.Param("@ID", id) }) > 0;
        }

        private Supplier Map(DataRow row)
        {
            return new Supplier
            {
                SupplierID   = Convert.ToInt32(row["SupplierID"]),
                SupplierName = row["SupplierName"].ToString(),
                Company      = row["Company"].ToString(),
                Phone        = row["Phone"].ToString(),
                Email        = row["Email"].ToString(),
                Address      = row["Address"].ToString(),
                CreatedDate  = Convert.ToDateTime(row["CreatedDate"]),
                IsActive     = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
