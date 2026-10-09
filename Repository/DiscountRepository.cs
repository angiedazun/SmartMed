using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class DiscountRepository
    {
        public List<Discount> GetAll()
        {
            var list = new List<Discount>();
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Discount ORDER BY DiscountName");
            foreach (DataRow row in dt.Rows)
                list.Add(Map(row));
            return list;
        }

        public int Create(Discount d)
        {
            return DatabaseHelper.ExecuteInsertGetId(
                "INSERT INTO Discount (DiscountName, Percentage, StartDate, EndDate, IsActive) VALUES (@Name, @Pct, @Start, @End, @Active)",
                new[]
                {
                    DatabaseHelper.Param("@Name",   d.DiscountName),
                    DatabaseHelper.Param("@Pct",    d.Percentage),
                    DatabaseHelper.Param("@Start",  d.StartDate),
                    DatabaseHelper.Param("@End",    d.EndDate),
                    DatabaseHelper.Param("@Active", d.IsActive)
                });
        }

        public bool Update(Discount d)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Discount SET DiscountName=@Name, Percentage=@Pct, StartDate=@Start, EndDate=@End, IsActive=@Active WHERE DiscountID=@ID",
                new[]
                {
                    DatabaseHelper.Param("@Name",   d.DiscountName),
                    DatabaseHelper.Param("@Pct",    d.Percentage),
                    DatabaseHelper.Param("@Start",  d.StartDate),
                    DatabaseHelper.Param("@End",    d.EndDate),
                    DatabaseHelper.Param("@Active", d.IsActive),
                    DatabaseHelper.Param("@ID",     d.DiscountID)
                }) > 0;
        }

        public bool Delete(int id) =>
            DatabaseHelper.ExecuteNonQuery("DELETE FROM Discount WHERE DiscountID=@ID",
                new[] { DatabaseHelper.Param("@ID", id) }) > 0;

        private Discount Map(DataRow row)
        {
            return new Discount
            {
                DiscountID   = Convert.ToInt32(row["DiscountID"]),
                DiscountName = row["DiscountName"].ToString(),
                Percentage   = Convert.ToDecimal(row["Percentage"]),
                StartDate    = Convert.ToDateTime(row["StartDate"]),
                EndDate      = Convert.ToDateTime(row["EndDate"]),
                IsActive     = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
