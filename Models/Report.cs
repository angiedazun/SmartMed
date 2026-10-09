using System;

namespace SmartMed.Models
{
    public class SalesReport
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetRevenue { get; set; }
    }

    public class StockReport
    {
        public string MedicineCode { get; set; }
        public string MedicineName { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
    }

    public class TopMedicine
    {
        public string MedicineName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DashboardStats
    {
        public decimal TotalSales { get; set; }
        public decimal TodaySales { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalMedicines { get; set; }
        public int ExpiredMedicines { get; set; }
        public int LowStockMedicines { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}
