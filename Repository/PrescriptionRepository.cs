using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Data;
using SmartMed.Models;

namespace SmartMed.Repository
{
    public class PrescriptionRepository
    {
        public int Create(Prescription p)
        {
            return DatabaseHelper.ExecuteInsertGetId(
                "INSERT INTO Prescription (CustomerID, OrderID, Image, DoctorName, IssueDate, Status) VALUES (@CustID, @OrderID, @Image, @Doctor, @Issue, 'Pending')",
                new[]
                {
                    DatabaseHelper.Param("@CustID",  p.CustomerID),
                    DatabaseHelper.ParamNullable("@OrderID", p.OrderID),
                    DatabaseHelper.Param("@Image",   p.Image ?? ""),
                    DatabaseHelper.Param("@Doctor",  p.DoctorName ?? ""),
                    DatabaseHelper.ParamNullable("@Issue", p.IssueDate)
                });
        }

        public List<Prescription> GetByCustomer(int customerId)
        {
            var list = new List<Prescription>();
            string sql = @"SELECT p.*, c.FirstName + ' ' + c.LastName AS CustomerName
                FROM Prescription p JOIN Customer c ON p.CustomerID = c.CustomerID
                WHERE p.CustomerID = @ID ORDER BY p.UploadDate DESC";
            var dt = DatabaseHelper.ExecuteQuery(sql, new[] { DatabaseHelper.Param("@ID", customerId) });
            foreach (DataRow row in dt.Rows)
                list.Add(Map(row));
            return list;
        }

        public bool UpdateStatus(int id, string status)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Prescription SET Status=@Status WHERE PrescriptionID=@ID",
                new[] { DatabaseHelper.Param("@Status", status), DatabaseHelper.Param("@ID", id) }) > 0;
        }

        private Prescription Map(DataRow row)
        {
            return new Prescription
            {
                PrescriptionID = Convert.ToInt32(row["PrescriptionID"]),
                CustomerID     = Convert.ToInt32(row["CustomerID"]),
                CustomerName   = row["CustomerName"].ToString(),
                OrderID        = row["OrderID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["OrderID"]),
                Image          = row["Image"].ToString(),
                DoctorName     = row["DoctorName"].ToString(),
                IssueDate      = row["IssueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["IssueDate"]),
                Status         = row["Status"].ToString(),
                UploadDate     = Convert.ToDateTime(row["UploadDate"])
            };
        }
    }
}
