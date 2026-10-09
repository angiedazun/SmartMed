using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;
using SmartMed.Utilities;
using CustomerModel = SmartMed.Models.Customer;

namespace SmartMed.Forms.Admin
{
    public class CustomerForm : Form
    {
        private readonly CustomerRepository _repo = new CustomerRepository();
        private readonly CustomerModel _customer;
        private TextBox txtFirst, txtLast, txtEmail, txtPhone, txtAddress, txtPassword;
        private Label lblEmailErr, lblPassErr;

        public CustomerForm(CustomerModel customer)
        {
            _customer = customer;
            InitForm();
            BuildLayout();
            if (customer != null) Populate();
        }

        private void InitForm()
        {
            Text = _customer == null ? "Add Customer" : "Edit Customer";
            Size = new Size(480, 580);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
        }

        private void BuildLayout()
        {
            var lblTitle = new Label { Text = Text, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(25, 20) };
            Controls.Add(lblTitle);

            int y = 62;
            txtFirst   = AddField("First Name *", 25, ref y);
            txtLast    = AddField("Last Name *",  25, ref y);
            txtEmail   = AddField("Email *",      25, ref y);
            lblEmailErr = AddErrLabel(25, ref y);
            txtPhone   = AddField("Phone",        25, ref y);
            txtAddress = AddField("Address",      25, ref y);

            string passLabel = _customer == null ? "Password *" : "New Password (optional)";
            txtPassword = AddPasswordField(passLabel, 25, ref y);
            lblPassErr = AddErrLabel(25, ref y);

            var btnSave = AppTheme.CreateButton("💾 Save", AppTheme.Primary, 150, 40);
            btnSave.Location = new Point(25, y + 25);
            btnSave.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnSave.Click += Save_Click;

            var btnCancel = AppTheme.CreateButton("Cancel", AppTheme.Surface, 120, 40);
            btnCancel.Location = new Point(185, y + 25);
            btnCancel.ForeColor = AppTheme.TextMuted;
            btnCancel.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private TextBox AddField(string label, int x, ref int y)
        {
            var lbl = new Label { Text = label, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(x, y) };
            y += 18;
            var pnl = new Panel { Location = new Point(x, y), Size = new Size(400, 30), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 4), Width = 390, BorderStyle = BorderStyle.None };
            pnl.Controls.Add(txt);
            Controls.AddRange(new Control[] { lbl, pnl });
            y += 42;
            return txt;
        }

        private TextBox AddPasswordField(string label, int x, ref int y)
        {
            var lbl = new Label { Text = label, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(x, y) };
            y += 18;
            var pnl = new Panel { Location = new Point(x, y), Size = new Size(400, 30), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 4), Width = 356, BorderStyle = BorderStyle.None, UseSystemPasswordChar = true };
            var btnShow = new Button { Text = "👁", Location = new Point(362, 2), Size = new Size(30, 24), BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextMuted, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, FlatAppearance = { BorderSize = 0 }, Font = new Font("Segoe UI Emoji", 10f) };
            btnShow.Click += (s, e) => { txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar; btnShow.Text = txt.UseSystemPasswordChar ? "👁" : "🙈"; };
            pnl.Controls.AddRange(new Control[] { txt, btnShow });
            Controls.AddRange(new Control[] { lbl, pnl });
            y += 42;
            return txt;
        }

        private Label AddErrLabel(int x, ref int y)
        {
            var lbl = new Label { Location = new Point(x, y - 14), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, Visible = false };
            Controls.Add(lbl);
            y += 2;
            return lbl;
        }

        private void Populate()
        {
            txtFirst.Text   = _customer.FirstName;
            txtLast.Text    = _customer.LastName;
            txtEmail.Text   = _customer.Email;
            txtPhone.Text   = _customer.Phone;
            txtAddress.Text = _customer.Address;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            lblEmailErr.Visible = false;
            if (!Validation.IsValidEmail(txtEmail.Text))
            {
                lblEmailErr.Text = "⚠ Invalid email";
                lblEmailErr.Visible = true;
                return;
            }

            lblPassErr.Visible = false;
            bool changingPassword = _customer == null || !string.IsNullOrEmpty(txtPassword.Text);
            if (changingPassword && !Validation.IsStrongPassword(txtPassword.Text))
            {
                lblPassErr.Text = "⚠ Password must be 8+ chars with upper, lower, number & special char";
                lblPassErr.Visible = true;
                return;
            }

            var c = new CustomerModel
            {
                CustomerID = _customer?.CustomerID ?? 0,
                FirstName  = txtFirst.Text.Trim(),
                LastName   = txtLast.Text.Trim(),
                Email      = txtEmail.Text.Trim(),
                Phone      = txtPhone.Text.Trim(),
                Address    = txtAddress.Text.Trim()
            };

            bool ok;
            if (_customer == null)
            {
                c.Password = PasswordHasher.Hash(txtPassword.Text);
                ok = _repo.Create(c) > 0;
            }
            else
            {
                c.ProfileImage = _customer.ProfileImage;
                ok = _repo.Update(c);
                if (ok && changingPassword)
                    ok = _repo.UpdatePassword(_customer.CustomerID, PasswordHasher.Hash(txtPassword.Text));
            }

            MessageBox.Show(ok ? "Saved successfully!" : "Save failed.",
                ok ? "Success" : "Error", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (ok) DialogResult = DialogResult.OK;
        }
    }
}
