using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class MedicineRepository
    {
        private const string SelectBase = @"
            SELECT m.*, c.CategoryName, s.SupplierName,
                   d.DiscountName, ISNULL(d.Percentage, 0) AS DiscountPercentage
            FROM Medicine m
            LEFT JOIN Category c ON m.CategoryID = c.CategoryID
            LEFT JOIN Supplier s ON m.SupplierID = s.SupplierID
            LEFT JOIN Discount d ON m.DiscountID = d.DiscountID";

        public List<Medicine> GetAll(string search = "", int categoryId = 0, string status = "")
        {
            var list = new List<Medicine>();
            string sql = SelectBase + @" WHERE 1=1
                AND (m.MedicineName LIKE @Search OR m.MedicineCode LIKE @Search OR c.CategoryName LIKE @Search)
                AND (@CategoryID = 0 OR m.CategoryID = @CategoryID)
                AND (@Status = '' OR m.Status = @Status)
                ORDER BY m.MedicineName";

            var dt = DatabaseHelper.ExecuteQuery(sql, new[]
            {
                DatabaseHelper.Param("@Search", $"%{search}%"),
                DatabaseHelper.Param("@CategoryID", categoryId),
                DatabaseHelper.Param("@Status", status)
            });
            foreach (DataRow row in dt.Rows)
                list.Add(MapMedicine(row));
            return list;
        }

        public Medicine GetByID(int id)
        {
            string sql = SelectBase + " WHERE m.MedicineID = @ID";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@ID", id) });
            return dt.Rows.Count > 0 ? MapMedicine(dt.Rows[0]) : null;
        }

        public Medicine GetByCode(string code)
        {
            string sql = SelectBase + " WHERE m.MedicineCode = @Code";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@Code", code) });
            return dt.Rows.Count > 0 ? MapMedicine(dt.Rows[0]) : null;
        }

        public int Create(Medicine m)
        {
            string sql = @"INSERT INTO Medicine
                (MedicineCode, MedicineName, CategoryID, SupplierID, Dosage, Unit, Price, Stock,
                 MinimumStock, ExpiryDate, Description, PrescriptionRequired, Image, DiscountID, Status)
                VALUES (@Code, @Name, @CatID, @SupID, @Dosage, @Unit, @Price, @Stock,
                        @MinStock, @Expiry, @Desc, @PrescReq, @Image, @DiscID, @Status)";
            return DatabaseHelper.ExecuteInsertGetId(sql, new[]
            {
                DatabaseHelper.Param("@Code", m.MedicineCode),
                DatabaseHelper.Param("@Name", m.MedicineName),
                DatabaseHelper.Param("@CatID", m.CategoryID),
                DatabaseHelper.Param("@SupID", m.SupplierID),
                DatabaseHelper.Param("@Dosage", m.Dosage ?? ""),
                DatabaseHelper.Param("@Unit", m.Unit ?? ""),
                DatabaseHelper.Param("@Price", m.Price),
                DatabaseHelper.Param("@Stock", m.Stock),
                DatabaseHelper.Param("@MinStock", m.MinimumStock),
                DatabaseHelper.Param("@Expiry", m.ExpiryDate),
                DatabaseHelper.Param("@Desc", m.Description ?? ""),
                DatabaseHelper.Param("@PrescReq", m.PrescriptionRequired),
                DatabaseHelper.ParamNullable("@Image", m.Image),
                DatabaseHelper.ParamNullable("@DiscID", m.DiscountID),
                DatabaseHelper.Param("@Status", m.Status ?? "Active")
            });
        }

        public bool Update(Medicine m)
        {
            string sql = @"UPDATE Medicine SET MedicineName=@Name, CategoryID=@CatID, SupplierID=@SupID,
                Dosage=@Dosage, Unit=@Unit, Price=@Price, Stock=@Stock, MinimumStock=@MinStock,
                ExpiryDate=@Expiry, Description=@Desc, PrescriptionRequired=@PrescReq,
                Image=@Image, DiscountID=@DiscID, Status=@Status WHERE MedicineID=@ID";
            return DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                DatabaseHelper.Param("@Name", m.MedicineName),
                DatabaseHelper.Param("@CatID", m.CategoryID),
                DatabaseHelper.Param("@SupID", m.SupplierID),
                DatabaseHelper.Param("@Dosage", m.Dosage ?? ""),
                DatabaseHelper.Param("@Unit", m.Unit ?? ""),
                DatabaseHelper.Param("@Price", m.Price),
                DatabaseHelper.Param("@Stock", m.Stock),
                DatabaseHelper.Param("@MinStock", m.MinimumStock),
                DatabaseHelper.Param("@Expiry", m.ExpiryDate),
                DatabaseHelper.Param("@Desc", m.Description ?? ""),
                DatabaseHelper.Param("@PrescReq", m.PrescriptionRequired),
                DatabaseHelper.ParamNullable("@Image", m.Image),
                DatabaseHelper.ParamNullable("@DiscID", m.DiscountID),
                DatabaseHelper.Param("@Status", m.Status ?? "Active"),
                DatabaseHelper.Param("@ID", m.MedicineID)
            }) > 0;
        }

        public bool HasOrders(int id)
        {
            var result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM OrderDetail WHERE MedicineID=@ID",
                new[] { DatabaseHelper.Param("@ID", id) });
            return Convert.ToInt32(result) > 0;
        }

        public bool Delete(int id)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM Medicine WHERE MedicineID=@ID",
                new[] { DatabaseHelper.Param("@ID", id) }) > 0;
        }

        public bool UpdateStock(int id, int newStock)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Medicine SET Stock=@Stock WHERE MedicineID=@ID",
                new[] { DatabaseHelper.Param("@Stock", newStock), DatabaseHelper.Param("@ID", id) }) > 0;
        }

        public bool CodeExists(string code, int excludeId = 0)
        {
            var result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Medicine WHERE MedicineCode=@Code AND MedicineID<>@ID",
                new[] { DatabaseHelper.Param("@Code", code), DatabaseHelper.Param("@ID", excludeId) });
            return Convert.ToInt32(result) > 0;
        }

        public int GetTotalCount() => Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Medicine WHERE Status='Active'"));
        public int GetExpiredCount() => Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Medicine WHERE ExpiryDate < GETDATE()"));
        public int GetLowStockCount() => Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Medicine WHERE Stock <= MinimumStock AND Status='Active'"));

        public DataTable GetExpired(int top = 8)
        {
            return DatabaseHelper.ExecuteQuery($@"
                SELECT TOP {top} m.MedicineCode, m.MedicineName, c.CategoryName, m.Stock, m.ExpiryDate
                FROM Medicine m
                LEFT JOIN Category c ON m.CategoryID = c.CategoryID
                WHERE m.ExpiryDate < GETDATE()
                ORDER BY m.ExpiryDate ASC");
        }

        public DataTable GetTopSelling(int top = 5)
        {
            return DatabaseHelper.ExecuteQuery($@"
                SELECT TOP {top} m.MedicineName, SUM(od.Quantity) AS TotalSold, SUM(od.Subtotal) AS TotalRevenue
                FROM OrderDetail od JOIN Medicine m ON od.MedicineID = m.MedicineID
                GROUP BY m.MedicineName ORDER BY TotalSold DESC");
        }

        public string GenerateNextCode()
        {
            var result = DatabaseHelper.ExecuteScalar(
                "SELECT TOP 1 MedicineCode FROM Medicine ORDER BY MedicineID DESC");
            if (result == null || result == DBNull.Value) return "MED-001";
            string last = result.ToString();
            if (last.StartsWith("MED-") && int.TryParse(last.Substring(4), out int num))
                return $"MED-{(num + 1):D3}";
            return "MED-001";
        }

        private Medicine MapMedicine(DataRow row)
        {
            return new Medicine
            {
                MedicineID = Convert.ToInt32(row["MedicineID"]),
                MedicineCode = row["MedicineCode"].ToString(),
                MedicineName = row["MedicineName"].ToString(),
                CategoryID = Convert.ToInt32(row["CategoryID"]),
                CategoryName = row["CategoryName"].ToString(),
                SupplierID = Convert.ToInt32(row["SupplierID"]),
                SupplierName = row["SupplierName"].ToString(),
                Dosage = row["Dosage"].ToString(),
                Unit = row["Unit"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                Stock = Convert.ToInt32(row["Stock"]),
                MinimumStock = Convert.ToInt32(row["MinimumStock"]),
                ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                Description = row["Description"].ToString(),
                PrescriptionRequired = Convert.ToBoolean(row["PrescriptionRequired"]),
                Image = row["Image"] == DBNull.Value ? null : row["Image"].ToString(),
                DiscountID = row["DiscountID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["DiscountID"]),
                DiscountName = row["DiscountName"] == DBNull.Value ? "" : row["DiscountName"].ToString(),
                DiscountPercentage = Convert.ToDecimal(row["DiscountPercentage"]),
                Status = row["Status"].ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }
    }
}
