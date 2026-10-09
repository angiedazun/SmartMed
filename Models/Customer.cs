using System;

namespace SmartMed.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string ProfileImage { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string Status { get; set; }
    }
}
