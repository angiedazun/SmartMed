using System;
using System.Text.RegularExpressions;

namespace SmartMed.Utilities
{
    public static class Validation
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone, @"^\+?[\d\s\-]{7,15}$");
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecial = Regex.IsMatch(password, @"[@$!%*?&]");
            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public static bool IsNumeric(string value) =>
            !string.IsNullOrWhiteSpace(value) && decimal.TryParse(value, out _);

        public static bool IsPositiveInteger(string value) =>
            int.TryParse(value, out int n) && n > 0;

        public static string GetPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";
            int score = 0;
            if (password.Length >= 8) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"\d")) score++;
            if (Regex.IsMatch(password, @"[@$!%*?&]")) score++;
            return score switch { 1 => "Very Weak", 2 => "Weak", 3 => "Fair", 4 => "Strong", 5 => "Very Strong", _ => "" };
        }
    }
}
