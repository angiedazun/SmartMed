using System;
using SmartMed.Models;
using SmartMed.Repository;
using SmartMed.Utilities;

namespace SmartMed.Services
{
    public class LoginService
    {
        private readonly AdminRepository _adminRepo = new AdminRepository();
        private readonly CustomerRepository _customerRepo = new CustomerRepository();

        private int _adminAttempts = 0;
        private int _customerAttempts = 0;
        private const int MaxAttempts = 5;

        public (bool success, string message, Admin admin) AdminLogin(string username, string password)
        {
            if (_adminAttempts >= MaxAttempts)
                return (false, "Account locked. Too many failed attempts.", null);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password are required.", null);

            var admin = _adminRepo.GetByUsername(username);
            if (admin == null)
            {
                _adminAttempts++;
                return (false, $"Invalid credentials. {MaxAttempts - _adminAttempts} attempts remaining.", null);
            }

            if (!PasswordHasher.Verify(password, admin.Password))
            {
                _adminAttempts++;
                return (false, $"Invalid credentials. {MaxAttempts - _adminAttempts} attempts remaining.", null);
            }

            _adminAttempts = 0;
            _adminRepo.LogActivity("Admin", admin.AdminID, $"Admin '{admin.Username}' logged in.");
            return (true, "Login successful.", admin);
        }

        public (bool success, string message, Customer customer) CustomerLogin(string email, string password)
        {
            if (_customerAttempts >= MaxAttempts)
                return (false, "Account locked. Too many failed attempts.", null);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email and password are required.", null);

            var customer = _customerRepo.GetByEmail(email);
            if (customer == null)
            {
                _customerAttempts++;
                return (false, $"Invalid credentials. {MaxAttempts - _customerAttempts} attempts remaining.", null);
            }

            if (customer.Status == "Disabled")
                return (false, "Your account has been disabled. Contact admin.", null);

            if (!PasswordHasher.Verify(password, customer.Password))
            {
                _customerAttempts++;
                return (false, $"Invalid credentials. {MaxAttempts - _customerAttempts} attempts remaining.", null);
            }

            _customerAttempts = 0;
            return (true, "Login successful.", customer);
        }

        public void ResetAttempts()
        {
            _adminAttempts = 0;
            _customerAttempts = 0;
        }
    }
}