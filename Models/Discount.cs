using System;

namespace SmartMed.Models
{
    public class Discount
    {
        public int DiscountID { get; set; }
        public string DiscountName { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentlyActive => IsActive && DateTime.Today >= StartDate && DateTime.Today <= EndDate;
    }
}
