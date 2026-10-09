using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;
using SmartMed.Utilities;

namespace SmartMed.Forms.Admin
{
    public class SettingsPanel : UserControl
    {
        private readonly AdminRepository _adminRepo = new AdminRepository();
        private TextBox txtFullName, txtEmail, txtOldPass, txtNewPass, txtConfirmPass;
        private Label lblPassMsg;

        public SettingsPanel()
        {
            BackColor  = AppTheme.Background;
            Dock       = DockStyle.Fill;
            AutoScroll = true;
            Build();
        }

        private void Build()
        {
            var lblTitle = new Label { Text = "Settings & Profile", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(30, 25) };
            Controls.Add(lblTitle);

            // Profile section
            var pnlProfile = new Panel { Location = new Point(30, 75), Size = new Size(500, 250), BackColor = AppTheme.Surface };
            var lblProfile = new Label { Text = "Profile Information", Font = AppTheme.FontHeader, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(20, 16) };

            int y = 55;
            txtFullName = AddTxt(pnlProfile, "Full Name", ref y);
            txtEmail    = AddTxt(pnlProfile, "Email",     ref y);

            txtFullName.Text = AppState.CurrentAdmin?.FullName ?? "";
            txtEmail.Text    = AppState.CurrentAdmin?.Email ?? "";

            var btnSaveProfile = AppTheme.CreateButton("💾 Save Profile", AppTheme.Primary, 150, 38);
            btnSaveProfile.Location = new Point(20, y + 10);
            btnSaveProfile.Click += (s, e) =>
            {
                if (AppState.CurrentAdmin == null) return;
                AppState.CurrentAdmin.FullName = txtFullName.Text.Trim();
                AppState.CurrentAdmin.Email    = txtEmail.Text.Trim();
                bool ok = _adminRepo.Update(AppState.CurrentAdmin);
                MessageBox.Show(ok ? "Profile updated!" : "Failed.", ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            };

            pnlProfile.Controls.AddRange(new Control[] { lblProfile, btnSaveProfile });
            Controls.Add(pnlProfile);

            // Change Password
            var pnlPwd = new Panel { Location = new Point(30, 340), Size = new Size(500, 325), BackColor = AppTheme.Surface };
            var lblPwd = new Label { Text = "Change Password", Font = AppTheme.FontHeader, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(20, 16) };

            int py = 55;
            txtOldPass     = AddTxt(pnlPwd, "Current Password",   ref py, isPassword: true);
            txtNewPass     = AddTxt(pnlPwd, "New Password",       ref py, isPassword: true);
            txtConfirmPass = AddTxt(pnlPwd, "Confirm New Password", ref py, isPassword: true);
            lblPassMsg = new Label { Location = new Point(20, py), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger };
            pnlPwd.Controls.Add(lblPassMsg);

            var btnChangePwd = AppTheme.CreateButton("🔒 Change Password", AppTheme.Danger, 180, 38);
            btnChangePwd.Location = new Point(20, py + 18);
            btnChangePwd.Click += ChangePwd_Click;

            pnlPwd.Controls.AddRange(new Control[] { lblPwd, btnChangePwd });
            Controls.Add(pnlPwd);

            // DB Connection Info
            var pnlDb = new Panel { Location = new Point(560, 75), Size = new Size(380, 160), BackColor = AppTheme.Surface };
            var lblDb = new Label { Text = "Database Info", Font = AppTheme.FontHeader, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(20, 16) };
            var lblDbInfo = new Label
            {
                Text = "Server: .\\SQLEXPRESS\nDatabase: SmartMedDB\nAuthentication: Windows",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted
            };
            var btnTestConn = AppTheme.CreateButton("🔌 Test Connection", AppTheme.Success, 180, 36);
            btnTestConn.Location = new Point(20, 110);
            btnTestConn.Click += (s, e) =>
            {
                bool ok = Data.DBConnection.TestConnection();
                MessageBox.Show(ok ? "Connection successful!" : "Connection failed. Check SQL Server.",
                    "Database", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            };
            pnlDb.Controls.AddRange(new Control[] { lblDb, lblDbInfo, btnTestConn });
            Controls.Add(pnlDb);

            // User Guide PDF — three separate guides
            var pnlGuide = new Panel { Location = new Point(560, 255), Size = new Size(380, 220), BackColor = AppTheme.Surface };
            pnlGuide.Controls.Add(new Label
            {
                Text      = "User Guides (PDF)",
                Font      = AppTheme.FontHeader,
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(20, 14)
            });
            pnlGuide.Controls.Add(new Label
            {
                Text      = "Generate a step-by-step guide for each user type:",
                Location  = new Point(20, 44),
                AutoSize  = true,
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted
            });

            var btnAdmin    = AppTheme.CreateButton("🛡  Admin Guide PDF",    AppTheme.Primary,  220, 34);
            var btnCustomer = AppTheme.CreateButton("👤  Customer Guide PDF", AppTheme.Success,  220, 34);
            var btnSupplier = AppTheme.CreateButton("🏭  Supplier Guide PDF", Color.FromArgb(13,148,136), 220, 34);

            btnAdmin.Location    = new Point(20, 72);
            btnCustomer.Location = new Point(20, 114);
            btnSupplier.Location = new Point(20, 156);

            btnAdmin.Click    += (s, e) => PDFExporter.ExportAdminGuide();
            btnCustomer.Click += (s, e) => PDFExporter.ExportCustomerGuide();
            btnSupplier.Click += (s, e) => PDFExporter.ExportSupplierGuide();

            pnlGuide.Controls.AddRange(new Control[] { btnAdmin, btnCustomer, btnSupplier });
            Controls.Add(pnlGuide);
        }

        private TextBox AddTxt(Panel panel, string label, ref int y, bool isPassword = false)
        {
            panel.Controls.Add(new Label { Text = label, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(20, y) });
            y += 22;
            var pnl = new Panel { Location = new Point(20, y), Size = new Size(450, 34), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 4), Width = 440, BorderStyle = BorderStyle.None, UseSystemPasswordChar = isPassword };
            pnl.Controls.Add(txt);
            panel.Controls.Add(pnl);
            y += 44;
            return txt;
        }

        private void ChangePwd_Click(object sender, EventArgs e)
        {
            lblPassMsg.Visible = false;
            if (AppState.CurrentAdmin == null) return;
            if (!PasswordHasher.Verify(txtOldPass.Text, AppState.CurrentAdmin.Password))
            {
                lblPassMsg.Text = "⚠ Current password is incorrect"; lblPassMsg.Visible = true; return;
            }
            if (!Validation.IsStrongPassword(txtNewPass.Text))
            {
                lblPassMsg.Text = "⚠ Password must be 8+ chars with upper, lower, number & special char"; lblPassMsg.Visible = true; return;
            }
            if (txtNewPass.Text != txtConfirmPass.Text)
            {
                lblPassMsg.Text = "⚠ Passwords do not match"; lblPassMsg.Visible = true; return;
            }
            string hashed = PasswordHasher.Hash(txtNewPass.Text);
            bool ok = _adminRepo.UpdatePassword(AppState.CurrentAdmin.AdminID, hashed);
            if (ok)
            {
                AppState.CurrentAdmin.Password = hashed;
                txtOldPass.Clear(); txtNewPass.Clear(); txtConfirmPass.Clear();
                lblPassMsg.ForeColor = AppTheme.Success;
                lblPassMsg.Text = "✓ Password changed successfully!";
                lblPassMsg.Visible = true;
            }
            else
            {
                lblPassMsg.Text = "⚠ Failed to change password"; lblPassMsg.Visible = true;
            }
        }
    }
}
