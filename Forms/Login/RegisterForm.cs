using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;
using SmartMed.Utilities;
using CustomerModel = SmartMed.Models.Customer;

namespace SmartMed.Forms.Login
{
    public class RegisterForm : Form
    {
        private readonly CustomerRepository _custRepo = new CustomerRepository();

        private TextBox txtFirst, txtLast, txtEmail, txtPhone, txtAddress, txtPass, txtConfirm;
        private Label lblPassStrength, lblEmailErr, lblPassErr, lblConfirmErr;
        private Button btnRegister, btnCancel;

        public RegisterForm()
        {
            InitForm();
            BuildLayout();
        }

        private void InitForm()
        {
            Text = "SmartMed - Customer Registration";
            Size = new Size(540, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = AppTheme.FontLabel;
        }

        private void BuildLayout()
        {
            var lblTitle = new Label
            {
                Text = "Create Account",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(30, 25)
            };
            var lblSub = new Label
            {
                Text = "Fill in your details to register as a customer",
                Font = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(30, 60)
            };

            int y = 88;

            txtFirst = AddField("First Name *", ref y);
            txtLast = AddField("Last Name *", ref y);
            txtEmail = AddField("Email Address *", ref y);
            lblEmailErr = AddErrLabel(ref y, -10);
            txtPhone = AddField("Phone Number", ref y);
            txtAddress = AddField("Address", ref y);

            var lblPass = CreateLabel("Password *", y);
            y += 18;
            var pnlPass = new Panel { Location = new Point(30, y), Size = new Size(460, 34), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            txtPass = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Width = 410, Height = 22, Location = new Point(4, 6), BorderStyle = BorderStyle.None, UseSystemPasswordChar = true };
            var btnShowPass = CreateEyeBtn(416, 6);
            btnShowPass.Click += (s, e) => { txtPass.UseSystemPasswordChar = !txtPass.UseSystemPasswordChar; btnShowPass.Text = txtPass.UseSystemPasswordChar ? "👁" : "🙈"; };
            pnlPass.Controls.AddRange(new Control[] { txtPass, btnShowPass });
            txtPass.TextChanged += (s, e) =>
            {
                var strength = Validation.GetPasswordStrength(txtPass.Text);
                lblPassStrength.Text = $"Strength: {strength}";
                lblPassStrength.ForeColor = strength switch
                {
                    "Very Strong" or "Strong" => AppTheme.Success,
                    "Fair" => AppTheme.Warning,
                    _ => AppTheme.Danger
                };
            };
            y += 36;

            lblPassStrength = new Label { Location = new Point(30, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMuted };
            y += 15;
            lblPassErr = AddErrLabel(ref y, 0);

            var lblConfirm = CreateLabel("Confirm Password *", y);
            y += 18;
            var pnlConfirm = new Panel { Location = new Point(30, y), Size = new Size(460, 34), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            txtConfirm = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Width = 410, Height = 22, Location = new Point(4, 6), BorderStyle = BorderStyle.None, UseSystemPasswordChar = true };
            pnlConfirm.Controls.Add(txtConfirm);
            y += 36;
            lblConfirmErr = AddErrLabel(ref y, 0);

            btnRegister = AppTheme.CreateButton("Create Account", AppTheme.Primary, 220, 44);
            btnRegister.Location = new Point(30, y + 8);
            btnRegister.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnRegister.Click += Register_Click;

            btnCancel = AppTheme.CreateButton("Cancel", AppTheme.Surface, 220, 44);
            btnCancel.Location = new Point(260, y + 8);
            btnCancel.ForeColor = AppTheme.TextMuted;
            btnCancel.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                lblTitle, lblSub,
                lblPass, pnlPass, lblPassStrength, lblPassErr,
                lblConfirm, pnlConfirm, lblConfirmErr,
                btnRegister, btnCancel
            });
        }

        private TextBox AddField(string label, ref int y)
        {
            var lbl = CreateLabel(label, y);
            y += 18;
            var pnl = new Panel { Location = new Point(30, y), Size = new Size(460, 34), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Width = 450, Height = 22, Location = new Point(4, 6), BorderStyle = BorderStyle.None };
            pnl.Controls.Add(txt);
            Controls.AddRange(new Control[] { lbl, pnl });
            y += 38;
            return txt;
        }

        private Label AddErrLabel(ref int y, int offset)
        {
            var lbl = new Label { Location = new Point(30, y + offset), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, Visible = false };
            Controls.Add(lbl);
            y += 14;
            return lbl;
        }

        private Label CreateLabel(string text, int y)
        {
            return new Label { Text = text, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(30, y) };
        }

        private Button CreateEyeBtn(int x, int y)
        {
            return new Button { Text = "👁", Location = new Point(x, y), Size = new Size(30, 22), BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextMuted, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, FlatAppearance = { BorderSize = 0 }, Font = new Font("Segoe UI Emoji", 11f) };
        }

        private void Register_Click(object sender, EventArgs e)
        {
            ClearErrors();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtFirst.Text)) { valid = false; }
            if (string.IsNullOrWhiteSpace(txtLast.Text)) { valid = false; }

            if (!Validation.IsValidEmail(txtEmail.Text))
            {
                lblEmailErr.Text = "⚠ Enter a valid email address";
                lblEmailErr.Visible = true;
                valid = false;
            }
            else if (_custRepo.GetByEmail(txtEmail.Text.Trim()) != null)
            {
                lblEmailErr.Text = "⚠ This email is already registered";
                lblEmailErr.Visible = true;
                valid = false;
            }

            if (!Validation.IsStrongPassword(txtPass.Text))
            {
                lblPassErr.Text = "⚠ Password must be 8+ chars with upper, lower, number & special char";
                lblPassErr.Visible = true;
                valid = false;
            }

            if (txtPass.Text != txtConfirm.Text)
            {
                lblConfirmErr.Text = "⚠ Passwords do not match";
                lblConfirmErr.Visible = true;
                valid = false;
            }

            if (!valid) return;

            var customer = new CustomerModel
            {
                FirstName = txtFirst.Text.Trim(),
                LastName  = txtLast.Text.Trim(),
                Email     = txtEmail.Text.Trim(),
                Phone     = txtPhone.Text.Trim(),
                Address   = txtAddress.Text.Trim(),
                Password  = PasswordHasher.Hash(txtPass.Text)
            };

            int id = _custRepo.Create(customer);
            if (id > 0)
            {
                MessageBox.Show("Account created successfully! You can now log in.",
                    "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            else
            {
                MessageBox.Show("Registration failed. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearErrors()
        {
            lblEmailErr.Visible = false;
            lblPassErr.Visible = false;
            lblConfirmErr.Visible = false;
        }
    }
}
