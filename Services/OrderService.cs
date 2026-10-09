using System;
using System.Collections.Generic;
using SmartMed.Models;
using SmartMed.Repository;

namespace SmartMed.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepo = new OrderRepository();
        private readonly MedicineRepository _medRepo = new MedicineRepository();

        public (bool success, string message, int orderId) PlaceOrder(Order order)
        {
            if (order.Items == null || order.Items.Count == 0)
                return (false, "Cart is empty.", 0);

            foreach (var item in order.Items)
            {
                var med = _medRepo.GetByID(item.MedicineID);
                if (med == null)
                    return (false, $"Medicine not found.", 0);
                if (med.Stock < item.Quantity)
                    return (false, $"Insufficient stock for '{med.MedicineName}'. Available: {med.Stock}", 0);
                if (med.IsExpired)
                    return (false, $"'{med.MedicineName}' is expired and cannot be ordered.", 0);
            }

            int id = _orderRepo.Create(order);
            return id > 0
                ? (true, "Order placed successfully.", id)
                : (false, "Failed to place order.", 0);
        }

        public (bool success, string message) UpdateStatus(int orderId, string newStatus)
        {
            var valid = new HashSet<string> { "Pending", "Approved", "Ready", "Delivered", "Cancelled", "Rejected" };
            if (!valid.Contains(newStatus))
                return (false, "Invalid status.");

            // Restore stock when an active order is cancelled or rejected
            if (newStatus == "Cancelled" || newStatus == "Rejected")
            {
                var order = _orderRepo.GetByID(orderId);
                // Only restore if the order was in an active (non-terminal) state
                if (order != null &&
                    order.Status != "Cancelled" &&
                    order.Status != "Rejected" &&
                    order.Status != "Delivered" &&
                    order.Items != null)
                {
                    foreach (var item in order.Items)
                        _orderRepo.RestoreStock(item.MedicineID, item.Quantity);
                }
            }

            bool ok = _orderRepo.UpdateStatus(orderId, newStatus);
            return ok ? (true, $"Order status updated to '{newStatus}'.") : (false, "Failed to update status.");
        }
    }
}