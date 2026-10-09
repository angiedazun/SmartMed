using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class OrderRepository
    {
        private const string SelectBase = @"
            SELECT o.*, c.FirstName + ' ' + c.LastName AS CustomerName,
                   c.Phone AS CustomerPhone, c.Address AS CustomerAddress,
                   (SELECT COUNT(*) FROM OrderDetail od WHERE od.OrderID = o.OrderID) AS ItemCount
            FROM [Order] o
            LEFT JOIN Customer c ON o.CustomerID = c.CustomerID";

        public List<Order> GetAll(string status = "", int customerId = 0)
        {
            var list = new List<Order>();
            string sql = SelectBase + @" WHERE 1=1
                AND (@Status='' OR o.Status=@Status)
                AND (@CustomerID=0 OR o.CustomerID=@CustomerID)
                ORDER BY o.OrderDate DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql, new[]
            {
                DatabaseHelper.Param("@Status", status),
                DatabaseHelper.Param("@CustomerID", customerId)
            });
            foreach (DataRow row in dt.Rows)
                list.Add(MapOrder(row));
            return list;
        }

        public Order GetByID(int id)
        {
            string sql = SelectBase + " WHERE o.OrderID = @ID";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@ID", id) });
            if (dt.Rows.Count == 0) return null;
            var order = MapOrder(dt.Rows[0]);
            order.Items = GetOrderItems(id);
            return order;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            var list = new List<OrderItem>();
            string sql = @"SELECT od.*, m.MedicineName, m.MedicineCode
                FROM OrderDetail od JOIN Medicine m ON od.MedicineID = m.MedicineID
                WHERE od.OrderID = @OrderID";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@OrderID", orderId) });
            foreach (DataRow row in dt.Rows)
                list.Add(MapItem(row));
            return list;
        }

        public int Create(Order order)
        {
            string sql = @"INSERT INTO [Order] (CustomerID, Total, Discount, FinalAmount, Status, PaymentMethod, PickupDate, Notes)
                           VALUES (@CustID, @Total, @Discount, @Final, @Status, @Payment, @Pickup, @Notes)";
            int id = DatabaseHelper.ExecuteInsertGetId(sql, new[]
            {
                DatabaseHelper.Param("@CustID", order.CustomerID),
                DatabaseHelper.Param("@Total", order.Total),
                DatabaseHelper.Param("@Discount", order.Discount),
                DatabaseHelper.Param("@Final", order.FinalAmount),
                DatabaseHelper.Param("@Status", order.Status ?? "Pending"),
                DatabaseHelper.Param("@Payment", order.PaymentMethod ?? "Cash"),
                DatabaseHelper.ParamNullable("@Pickup", order.PickupDate),
                DatabaseHelper.Param("@Notes", order.Notes ?? "")
            });

            foreach (var item in order.Items)
            {
                AddOrderItem(id, item);
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Medicine SET Stock = Stock - @Qty WHERE MedicineID = @ID",
                    new[] { DatabaseHelper.Param("@Qty", item.Quantity), DatabaseHelper.Param("@ID", item.MedicineID) });
            }
            return id;
        }

        private void AddOrderItem(int orderId, OrderItem item)
        {
            string sql = @"INSERT INTO OrderDetail (OrderID, MedicineID, Quantity, Price, Subtotal)
                           VALUES (@OrderID, @MedID, @Qty, @Price, @Subtotal)";
            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                DatabaseHelper.Param("@OrderID", orderId),
                DatabaseHelper.Param("@MedID", item.MedicineID),
                DatabaseHelper.Param("@Qty", item.Quantity),
                DatabaseHelper.Param("@Price", item.Price),
                DatabaseHelper.Param("@Subtotal", item.Subtotal)
            });
        }

        public bool UpdateStatus(int orderId, string status)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE [Order] SET Status=@Status WHERE OrderID=@ID",
                new[] { DatabaseHelper.Param("@Status", status), DatabaseHelper.Param("@ID", orderId) }) > 0;
        }

        public void RestoreStock(int medicineId, int quantity)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Medicine SET Stock = Stock + @Qty WHERE MedicineID = @ID",
                new[] { DatabaseHelper.Param("@Qty", quantity), DatabaseHelper.Param("@ID", medicineId) });
        }

        public int GetPendingCount() =>
            Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Order] WHERE Status='Pending'"));

        public int GetCompletedCount() =>
            Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Order] WHERE Status='Delivered'"));

        public decimal GetTotalSales() =>
            Convert.ToDecimal(DatabaseHelper.ExecuteScalar("SELECT ISNULL(SUM(FinalAmount),0) FROM [Order] WHERE Status='Delivered'"));

        public decimal GetTodaySales() =>
            Convert.ToDecimal(DatabaseHelper.ExecuteScalar(
                "SELECT ISNULL(SUM(FinalAmount),0) FROM [Order] WHERE Status='Delivered' AND CAST(OrderDate AS DATE)=CAST(GETDATE() AS DATE)"));

        public decimal GetMonthlyRevenue() =>
            Convert.ToDecimal(DatabaseHelper.ExecuteScalar(
                "SELECT ISNULL(SUM(FinalAmount),0) FROM [Order] WHERE Status='Delivered' AND MONTH(OrderDate)=MONTH(GETDATE()) AND YEAR(OrderDate)=YEAR(GETDATE())"));

        public DataTable GetMonthlySalesChart()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT MONTH(OrderDate) AS Month, YEAR(OrderDate) AS Year,
                       SUM(FinalAmount) AS Revenue, COUNT(*) AS Orders
                FROM [Order] WHERE Status='Delivered' AND YEAR(OrderDate)=YEAR(GETDATE())
                GROUP BY YEAR(OrderDate), MONTH(OrderDate)
                ORDER BY Month");
        }

        public DataTable GetSalesReport(DateTime from, DateTime to)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT CAST(o.OrderDate AS DATE) AS [Date],
                       COUNT(*) AS TotalOrders,
                       SUM(o.Total) AS TotalSales,
                       SUM(o.Discount) AS TotalDiscount,
                       SUM(o.FinalAmount) AS NetRevenue
                FROM [Order] o
                WHERE o.OrderDate >= @From AND o.OrderDate <= @To AND o.Status='Delivered'
                GROUP BY CAST(o.OrderDate AS DATE)
                ORDER BY [Date]",
                new[]
                {
                    DatabaseHelper.Param("@From", from),
                    DatabaseHelper.Param("@To", to.AddDays(1))
                });
        }

        private Order MapOrder(DataRow row)
        {
            return new Order
            {
                OrderID = Convert.ToInt32(row["OrderID"]),
                CustomerID = Convert.ToInt32(row["CustomerID"]),
                CustomerName = row["CustomerName"].ToString(),
                CustomerPhone   = row["CustomerPhone"]   == DBNull.Value ? "" : row["CustomerPhone"].ToString(),
                CustomerAddress = row["CustomerAddress"] == DBNull.Value ? "" : row["CustomerAddress"].ToString(),
                OrderDate = Convert.ToDateTime(row["OrderDate"]),
                Total = Convert.ToDecimal(row["Total"]),
                Discount = Convert.ToDecimal(row["Discount"]),
                FinalAmount = Convert.ToDecimal(row["FinalAmount"]),
                Status = row["Status"].ToString(),
                PaymentMethod = row["PaymentMethod"].ToString(),
                PickupDate = row["PickupDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["PickupDate"]),
                Notes      = row["Notes"].ToString(),
                ItemCount  = row.Table.Columns.Contains("ItemCount") && row["ItemCount"] != DBNull.Value
                             ? Convert.ToInt32(row["ItemCount"]) : 0
            };
        }

        private OrderItem MapItem(DataRow row)
        {
            return new OrderItem
            {
                OrderDetailID = Convert.ToInt32(row["OrderDetailID"]),
                OrderID = Convert.ToInt32(row["OrderID"]),
                MedicineID = Convert.ToInt32(row["MedicineID"]),
                MedicineName = row["MedicineName"].ToString(),
                MedicineCode = row["MedicineCode"].ToString(),
                Quantity = Convert.ToInt32(row["Quantity"]),
                Price = Convert.ToDecimal(row["Price"]),
                Subtotal = Convert.ToDecimal(row["Subtotal"])
            };
        }
    }
}
