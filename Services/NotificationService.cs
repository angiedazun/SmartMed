using System.Collections.Generic;
using SmartMed.Data;
using System.Data;
using System;

namespace SmartMed.Services
{
    public class NotificationService
    {
        public List<string> GetAlerts()
        {
            var alerts = new List<string>();

            var lowStock = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Medicine WHERE Stock <= MinimumStock AND Status='Active'");
            if (Convert.ToInt32(lowStock) > 0)
                alerts.Add($"⚠ {lowStock} medicine(s) are low in stock!");

            var expired = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Medicine WHERE ExpiryDate < GETDATE()");
            if (Convert.ToInt32(expired) > 0)
                alerts.Add($"⚠ {expired} medicine(s) have expired!");

            var expiringSoon = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Medicine WHERE ExpiryDate BETWEEN GETDATE() AND DATEADD(DAY,30,GETDATE())");
            if (Convert.ToInt32(expiringSoon) > 0)
                alerts.Add($"⚠ {expiringSoon} medicine(s) expire within 30 days!");

            var pending = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM [Order] WHERE Status='Pending'");
            if (Convert.ToInt32(pending) > 0)
                alerts.Add($"📦 {pending} pending order(s) need attention!");

            return alerts;
        }
    }
}
