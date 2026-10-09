using System;

namespace SmartMed.Models
{
    public class Medicine
    {
        public int MedicineID { get; set; }
        public string MedicineCode { get; set; }
        public string MedicineName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string Dosage { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Description { get; set; }
        public bool PrescriptionRequired { get; set; }
        public string Image { get; set; }
        public int? DiscountID { get; set; }
        public string DiscountName { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public decimal FinalPrice => DiscountPercentage > 0
            ? Price - (Price * DiscountPercentage / 100)
            : Price;

        public bool IsLowStock => Stock <= MinimumStock;
        public bool IsExpired => ExpiryDate < DateTime.Today;
        public bool IsExpiringSoon => ExpiryDate >= DateTime.Today && ExpiryDate <= DateTime.Today.AddDays(30);
    }
}
