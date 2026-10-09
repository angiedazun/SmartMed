using System;

namespace SmartMed.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public int? OrderID { get; set; }
        public string Image { get; set; }
        public string DoctorName { get; set; }
        public DateTime? IssueDate { get; set; }
        public string Status { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
