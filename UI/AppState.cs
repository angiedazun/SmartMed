using System.Collections.Generic;
using SmartMed.Models;

namespace SmartMed.UI
{
    public static class AppState
    {
        public static Admin CurrentAdmin { get; set; }
        public static Customer CurrentCustomer { get; set; }

        public static List<OrderItem> Cart { get; set; } = new List<OrderItem>();

        public static void ClearAdmin() { CurrentAdmin = null; }
        public static void ClearCustomer() { CurrentCustomer = null; Cart.Clear(); }

        public static decimal CartTotal()
        {
            decimal total = 0;
            foreach (var item in Cart) total += item.Subtotal;
            return total;
        }

        public static int CartCount()
        {
            int count = 0;
            foreach (var item in Cart) count += item.Quantity;
            return count;
        }
    }
}
