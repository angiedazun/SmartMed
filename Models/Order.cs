using System;
using System.Collections.Generic;

namespace SmartMed.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PickupDate { get; set; }
        public string Notes { get; set; }
        public int ItemCount { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int MedicineID { get; set; }
        public string MedicineName { get; set; }
        public string MedicineCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
}
