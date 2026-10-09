using System;
using System.Collections.Generic;
using SmartMed.Models;
using SmartMed.Repository;

namespace SmartMed.Services
{
    public class MedicineService
    {
        private readonly MedicineRepository _repo = new MedicineRepository();

        public (bool success, string message, int id) AddMedicine(Medicine m)
        {
            if (string.IsNullOrWhiteSpace(m.MedicineName))
                return (false, "Medicine name is required.", 0);
            if (m.Price <= 0)
                return (false, "Price must be greater than 0.", 0);
            if (m.ExpiryDate <= DateTime.Today)
                return (false, "Expiry date must be in the future.", 0);
            if (_repo.CodeExists(m.MedicineCode))
                return (false, $"Medicine code '{m.MedicineCode}' already exists.", 0);

            int id = _repo.Create(m);
            return id > 0 ? (true, "Medicine added successfully.", id) : (false, "Failed to add medicine.", 0);
        }

        public (bool success, string message) UpdateMedicine(Medicine m)
        {
            if (string.IsNullOrWhiteSpace(m.MedicineName))
                return (false, "Medicine name is required.");
            if (m.Price <= 0)
                return (false, "Price must be greater than 0.");
            if (m.ExpiryDate <= DateTime.Today)
                return (false, "Expiry date must be in the future.");
            if (_repo.CodeExists(m.MedicineCode, m.MedicineID))
                return (false, $"Medicine code '{m.MedicineCode}' already exists.");

            bool ok = _repo.Update(m);
            return ok ? (true, "Medicine updated successfully.") : (false, "Failed to update medicine.");
        }

        public (bool success, string message) DeleteMedicine(int id)
        {
            if (_repo.HasOrders(id))
                return (false, "Cannot delete this medicine because it has existing order records.\nUse Edit → Status = Inactive instead.");
            bool ok = _repo.Delete(id);
            return ok ? (true, "Medicine deleted successfully.") : (false, "Failed to delete medicine.");
        }

        public string GenerateCode() => _repo.GenerateNextCode();
    }
}
