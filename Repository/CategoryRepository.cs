using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class CategoryRepository
    {
        public List<Category> GetAll(string search = "")
        {
            var list = new List<Category>();
            string sql = @"SELECT c.*, COUNT(m.MedicineID) AS MedicineCount
                FROM Category c LEFT JOIN Medicine m ON c.CategoryID = m.CategoryID
                WHERE c.CategoryName LIKE @Search
                GROUP BY c.CategoryID, c.CategoryName, c.Description
                ORDER BY c.CategoryName";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@Search", $"%{search}%") });
            foreach (DataRow row in dt.Rows)
                list.Add(new Category
                {
                    CategoryID    = Convert.ToInt32(row["CategoryID"]),
                    CategoryName  = row["CategoryName"].ToString(),
                    Description   = row["Description"].ToString(),
                    MedicineCount = Convert.ToInt32(row["MedicineCount"])
                });
            return list;
        }

        public int Create(Category c)
        {
            return DatabaseHelper.ExecuteInsertGetId(
                "INSERT INTO Category (CategoryName, Description) VALUES (@Name, @Desc)",
                new[] { DatabaseHelper.Param("@Name", c.CategoryName), DatabaseHelper.Param("@Desc", c.Description ?? "") });
        }

        public bool Update(Category c)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Category SET CategoryName=@Name, Description=@Desc WHERE CategoryID=@ID",
                new[] { DatabaseHelper.Param("@Name", c.CategoryName), DatabaseHelper.Param("@Desc", c.Description ?? ""), DatabaseHelper.Param("@ID", c.CategoryID) }) > 0;
        }

        public bool Delete(int id)
        {
            var count = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Medicine WHERE CategoryID=@ID", new[] { DatabaseHelper.Param("@ID", id) });
            if (Convert.ToInt32(count) > 0) return false;
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM Category WHERE CategoryID=@ID", new[] { DatabaseHelper.Param("@ID", id) }) > 0;
        }

        public bool NameExists(string name, int excludeId = 0)
        {
            var r = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Category WHERE CategoryName=@Name AND CategoryID<>@ID",
                new[] { DatabaseHelper.Param("@Name", name), DatabaseHelper.Param("@ID", excludeId) });
            return Convert.ToInt32(r) > 0;
        }
    }
}
