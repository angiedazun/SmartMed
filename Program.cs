using System;
using System.Windows.Forms;
using SmartMed.Data;
using SmartMed.Forms.Login;
using SmartMed.Utilities;

namespace SmartMed
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!DBConnection.TestConnection())
            {
                var result = MessageBox.Show(
                    "Cannot connect to the database.\n\n" +
                    "Please ensure SQL Server Express is running\n" +
                    "and the SmartMedDB database has been created.\n\n" +
                    "Connection: .\\SQLEXPRESS\n\n" +
                    "Run the script: Database\\SmartMed.sql\n\n" +
                    "Continue anyway?",
                    "Database Connection Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.No)
                    return;
            }

            try { SeedAdminPassword(); } catch { }

            Application.Run(new LoginForm());
        }

        private static void SeedAdminPassword()
        {
            var adminRepo = new Repository.AdminRepository();
            var admin     = adminRepo.GetByUsername("admin");
            if (admin != null && !admin.Password.StartsWith("$2"))
                adminRepo.UpdatePassword(admin.AdminID, PasswordHasher.Hash("Admin@123"));
        }
    }
}
