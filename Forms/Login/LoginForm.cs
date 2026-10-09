using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartMed.Services;
using SmartMed.UI;

namespace SmartMed.Forms.Login
{
    public class LoginForm : Form
    {
        private readonly LoginService _loginService = new LoginService();
        private bool _isAdminTab = true;

        private TextBox  txtAdminUser, txtAdminPass, txtCustEmail, txtCustPass;
        private Button   btnAdminLogin, btnCustLogin, btnAdminShowPwd, btnCustShowPwd;
        private CheckBox chkRemember;
        private LinkLabel lnkForgot, lnkRegister;
        private Label    lblAdminUserErr, lblAdminPassErr, lblCustEmailErr, lblCustPassErr;
        private Panel    pnlAdminFields, pnlCustFields;
        private Button   btnAdminTab, btnCustTab;

        public LoginForm()
        {
            InitForm();
            BuildLayout();
            LoadSavedCredentials();
        }

        private void InitForm()
        {
            Text            = "SmartMed Pharmacy - Login";
            Size            = new Size(960, 580);
            MinimumSize     = new Size(960, 580);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            Font            = AppTheme.FontLabel;
        }

        private void BuildLayout()
        {
            // ══════════════════════════════════════════════════════
            // LEFT PANEL — Solid blue (matches app sidebar brand)
            // Everything drawn via Paint for reliable rendering
            // ══════════════════════════════════════════════════════
            var pnlLeft = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(370, 580),
                BackColor = Color.FromArgb(37, 99, 235)   // same as sidebar
            };
            pnlLeft.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int W = pnlLeft.Width, H = pnlLeft.Height;

                // ── 1. Blue gradient background ──────────────────────────
                using var bgBr = new LinearGradientBrush(
                    new Point(0, 0), new Point(0, H),
                    Color.FromArgb(37,  99, 235),   // blue-600
                    Color.FromArgb(29,  78, 216));  // blue-700
                g.FillRectangle(bgBr, pnlLeft.ClientRectangle);

                // ── 2. Decorative large circles (depth) ──────────────────
                using var c1 = new SolidBrush(Color.FromArgb(18, 255, 255, 255));
                g.FillEllipse(c1, -90, -90, 280, 280);   // top-left large
                g.FillEllipse(c1, 170, 380, 260, 260);   // bottom-right large
                using var c2 = new Pen(Color.FromArgb(30, 255, 255, 255), 1.2f);
                g.DrawEllipse(c2, -60, -60, 200, 200);
                g.DrawEllipse(c2, 200, 410, 210, 210);
                g.DrawEllipse(c2, 120, 210,  80,  80);   // small mid-right

                // ── 3. Logo: white rounded square + blue cross ────────────
                int lx = 148, ly = 58, lw = 74, lh = 74, lr = 14;
                using var lPath = new System.Drawing.Drawing2D.GraphicsPath();
                lPath.AddArc(lx,           ly,           lr * 2, lr * 2, 180, 90);
                lPath.AddArc(lx + lw - lr * 2, ly,           lr * 2, lr * 2, 270, 90);
                lPath.AddArc(lx + lw - lr * 2, ly + lh - lr * 2, lr * 2, lr * 2,   0, 90);
                lPath.AddArc(lx,           ly + lh - lr * 2, lr * 2, lr * 2,  90, 90);
                lPath.CloseFigure();
                using var logoBg = new SolidBrush(Color.White);
                g.FillPath(logoBg, lPath);
                // Cross in sidebar blue
                using var crossPen = new Pen(Color.FromArgb(37, 99, 235), 5f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round };
                int lcx = lx + lw / 2, lcy = ly + lh / 2;
                g.DrawLine(crossPen, lcx, lcy - 16, lcx, lcy + 16);
                g.DrawLine(crossPen, lcx - 16, lcy, lcx + 16, lcy);

                // ── 4. Brand name ─────────────────────────────────────────
                using var fBrand = new Font("Segoe UI", 26f, FontStyle.Bold);
                using var brW    = new SolidBrush(Color.White);
                var sfC = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("SmartMed", fBrand, brW, new RectangleF(0, 146, W, 38), sfC);

                // ── 5. Subtitle ───────────────────────────────────────────
                using var fSub = new Font("Segoe UI", 10f);
                using var brL  = new SolidBrush(Color.FromArgb(200, 219, 234, 254));
                g.DrawString("Pharmacy Management System", fSub, brL,
                    new RectangleF(0, 190, W, 22), sfC);

                // ── 6. Divider ────────────────────────────────────────────
                using var divPen = new Pen(Color.FromArgb(55, 255, 255, 255), 1f);
                g.DrawLine(divPen, 48, 224, W - 48, 224);

                // ── 7. Feature rows ───────────────────────────────────────
                var features = new[]
                {
                    "Medicine Management",
                    "Order Processing",
                    "Customer Portal",
                    "Reports & Analytics",
                    "Inventory Control"
                };
                using var fFeat  = new Font("Segoe UI", 10f);
                using var brFeat = new SolidBrush(Color.FromArgb(225, 239, 254));   // blue-100
                using var brChk  = new SolidBrush(Color.FromArgb(110, 231, 183));   // green-300
                using var fChk   = new Font("Segoe UI", 8f, FontStyle.Bold);
                var sfChk = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                int fy = 240;
                foreach (var feat in features)
                {
                    // Green circle check
                    using var chkCircle = new SolidBrush(Color.FromArgb(35, 255, 255, 255));
                    g.FillEllipse(chkCircle, 48, fy + 2, 20, 20);
                    g.DrawString("✓", fChk, brChk, new RectangleF(48, fy + 2, 20, 20), sfChk);
                    // Feature label
                    g.DrawString(feat, fFeat, brFeat, 76, fy);
                    fy += 30;
                }

                // ── 8. Bottom tagline + version ───────────────────────────
                using var fTag = new Font("Segoe UI", 9f, FontStyle.Italic);
                using var brTag = new SolidBrush(Color.FromArgb(140, 255, 255, 255));
                g.DrawString("Your trusted pharmacy partner", fTag, brTag,
                    new RectangleF(0, 498, W, 20), sfC);
                using var fVer = new Font("Segoe UI", 8f);
                using var brVer = new SolidBrush(Color.FromArgb(90, 255, 255, 255));
                g.DrawString("Version 1.0  ·  Professional Edition", fVer, brVer,
                    new RectangleF(0, 548, W, 18), sfC);
            };
            Controls.Add(pnlLeft);

            // Thin vertical separator — slightly darker blue edge
            Controls.Add(new Panel
            {
                Location  = new Point(370, 0),
                Size      = new Size(1, 580),
                BackColor = Color.FromArgb(29, 78, 216)
            });

            // ══════════════════════════════════════════════════════
            // RIGHT PANEL — Clean white login area
            // ══════════════════════════════════════════════════════
            var pnlRight = new Panel
            {
                Location  = new Point(371, 0),
                Size      = new Size(589, 580),
                BackColor = Color.White
            };

            // ── Compact header ─────────────────────────────────────
            // Blue circle icon
            var pnlIcon = new Panel
            {
                Location  = new Point(55, 36),
                Size      = new Size(40, 40),
                BackColor = Color.Transparent
            };
            pnlIcon.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new SolidBrush(Color.FromArgb(37, 99, 235));
                g.FillEllipse(br, 0, 0, 39, 39);
                using var brW = new SolidBrush(Color.White);
                g.FillRectangle(brW, 16, 8, 8, 24);
                g.FillRectangle(brW, 8, 16, 24, 8);
            };
            pnlRight.Controls.Add(pnlIcon);

            pnlRight.Controls.Add(new Label
            {
                Text      = "SmartMed Pharmacy",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize  = true,
                Location  = new Point(102, 44),
                BackColor = Color.Transparent
            });

            // Thin header underline
            pnlRight.Controls.Add(new Panel
            {
                Location  = new Point(55, 88),
                Size      = new Size(480, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            });

            // ── Tab bar — underline style ──────────────────────────
            var pnlTabBar = new Panel
            {
                Location  = new Point(55, 100),
                Size      = new Size(480, 44),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlTabBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawRectangle(pen, 0, 0, pnlTabBar.Width - 1, pnlTabBar.Height - 1);
            };

            btnAdminTab = MakeTabBtn("🛡  Admin Login",    true);
            btnAdminTab.Location = new Point(0, 0);
            btnAdminTab.Size     = new Size(240, 44);
            btnCustTab  = MakeTabBtn("👤  Customer Login", false);
            btnCustTab.Location  = new Point(240, 0);
            btnCustTab.Size      = new Size(240, 44);
            btnAdminTab.Click += (s, e) => SwitchTab(true);
            btnCustTab.Click  += (s, e) => SwitchTab(false);
            pnlTabBar.Controls.AddRange(new Control[] { btnAdminTab, btnCustTab });
            pnlRight.Controls.Add(pnlTabBar);

            // ── Welcome text ───────────────────────────────────────
            pnlRight.Controls.Add(new Label
            {
                Text      = "Welcome back! Sign in to continue.",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize  = true,
                Location  = new Point(55, 156)
            });

            // ── Field panels ───────────────────────────────────────
            pnlAdminFields = BuildAdminFields();
            pnlAdminFields.Location = new Point(55, 178);
            pnlAdminFields.Visible  = true;

            pnlCustFields = BuildCustFields();
            pnlCustFields.Location = new Point(55, 178);
            pnlCustFields.Visible  = false;

            pnlRight.Controls.AddRange(new Control[] { pnlAdminFields, pnlCustFields });
            Controls.Add(pnlRight);
        }

        private Panel BuildAdminFields()
        {
            var panel = new Panel { Size = new Size(480, 370), BackColor = Color.Transparent };

            // Username
            panel.Controls.Add(FieldLabel("Username", 0, 0));
            var pnlUser = InputBox(0, 22);
            txtAdminUser = InputText(400);
            txtAdminUser.SetPlaceholder("Enter your username");
            txtAdminUser.TextChanged += (s, e) => lblAdminUserErr.Visible = false;
            pnlUser.Controls.Add(txtAdminUser);
            panel.Controls.Add(pnlUser);
            lblAdminUserErr = ErrLabel(0, 66);
            panel.Controls.Add(lblAdminUserErr);

            // Password
            panel.Controls.Add(FieldLabel("Password", 0, 82));
            var pnlPass = InputBox(82, 104);
            txtAdminPass = InputText(400);
            txtAdminPass.UseSystemPasswordChar = true;
            txtAdminPass.SetPlaceholder("Enter your password");
            txtAdminPass.KeyDown   += (s, e) => { if (e.KeyCode == Keys.Enter) DoAdminLogin(); };
            txtAdminPass.TextChanged += (s, e) => lblAdminPassErr.Visible = false;
            btnAdminShowPwd = EyeBtn();
            btnAdminShowPwd.Click += (s, e) =>
            {
                txtAdminPass.UseSystemPasswordChar = !txtAdminPass.UseSystemPasswordChar;
                btnAdminShowPwd.Text = txtAdminPass.UseSystemPasswordChar ? "👁" : "🙈";
            };
            pnlPass.Controls.AddRange(new Control[] { txtAdminPass, btnAdminShowPwd });
            panel.Controls.Add(pnlPass);
            lblAdminPassErr = ErrLabel(0, 148);
            panel.Controls.Add(lblAdminPassErr);

            // Remember me + Forgot
            chkRemember = new CheckBox
            {
                Text      = "Remember me",
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent,
                Location  = new Point(0, 164),
                AutoSize  = true,
                Cursor    = Cursors.Hand
            };
            lnkForgot = new LinkLabel
            {
                Text            = "Forgot Password?",
                Font            = AppTheme.FontSmall,
                Location        = new Point(352, 166),
                AutoSize        = true,
                LinkColor       = AppTheme.Primary,
                ActiveLinkColor = AppTheme.PrimaryHover
            };
            lnkForgot.LinkClicked += (s, e) =>
                MessageBox.Show("Contact your system administrator to reset the password.",
                    "Forgot Password", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnAdminLogin = MakeSignInBtn(0, 195);
            btnAdminLogin.Click += (s, e) => DoAdminLogin();

            panel.Controls.AddRange(new Control[] { chkRemember, lnkForgot, btnAdminLogin });
            return panel;
        }

        private Panel BuildCustFields()
        {
            var panel = new Panel { Size = new Size(480, 370), BackColor = Color.Transparent };

            // Email
            panel.Controls.Add(FieldLabel("Email Address", 0, 0));
            var pnlEmail = InputBox(0, 22);
            txtCustEmail = InputText(400);
            txtCustEmail.SetPlaceholder("Enter your email address");
            txtCustEmail.TextChanged += (s, e) => lblCustEmailErr.Visible = false;
            pnlEmail.Controls.Add(txtCustEmail);
            panel.Controls.Add(pnlEmail);
            lblCustEmailErr = ErrLabel(0, 66);
            panel.Controls.Add(lblCustEmailErr);

            // Password
            panel.Controls.Add(FieldLabel("Password", 0, 82));
            var pnlPass = InputBox(82, 104);
            txtCustPass = InputText(400);
            txtCustPass.UseSystemPasswordChar = true;
            txtCustPass.SetPlaceholder("Enter your password");
            txtCustPass.KeyDown   += (s, e) => { if (e.KeyCode == Keys.Enter) DoCustLogin(); };
            txtCustPass.TextChanged += (s, e) => lblCustPassErr.Visible = false;
            btnCustShowPwd = EyeBtn();
            btnCustShowPwd.Click += (s, e) =>
            {
                txtCustPass.UseSystemPasswordChar = !txtCustPass.UseSystemPasswordChar;
                btnCustShowPwd.Text = txtCustPass.UseSystemPasswordChar ? "👁" : "🙈";
            };
            pnlPass.Controls.AddRange(new Control[] { txtCustPass, btnCustShowPwd });
            panel.Controls.Add(pnlPass);
            lblCustPassErr = ErrLabel(0, 148);
            panel.Controls.Add(lblCustPassErr);

            btnCustLogin = MakeSignInBtn(0, 173);
            btnCustLogin.Click += (s, e) => DoCustLogin();
            panel.Controls.Add(btnCustLogin);

            // Register row
            var pnlReg = new Panel { Location = new Point(0, 228), Size = new Size(480, 28), BackColor = Color.Transparent };
            pnlReg.Controls.Add(new Label
            {
                Text      = "Don't have an account?",
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(100, 4)
            });
            lnkRegister = new LinkLabel
            {
                Text            = "Register here",
                Font            = AppTheme.FontBold,
                Location        = new Point(272, 4),
                AutoSize        = true,
                LinkColor       = AppTheme.Primary,
                ActiveLinkColor = AppTheme.PrimaryHover
            };
            lnkRegister.LinkClicked += (s, e) => new RegisterForm().ShowDialog(this);
            pnlReg.Controls.Add(lnkRegister);
            panel.Controls.Add(pnlReg);

            return panel;
        }

        // ── Login handlers ────────────────────────────────────────
        private void DoAdminLogin()
        {
            bool ok = true;
            if (string.IsNullOrWhiteSpace(txtAdminUser.Text))
            { lblAdminUserErr.Text = "⚠ Username is required"; lblAdminUserErr.Visible = true; ok = false; }
            if (string.IsNullOrWhiteSpace(txtAdminPass.Text))
            { lblAdminPassErr.Text = "⚠ Password is required"; lblAdminPassErr.Visible = true; ok = false; }
            if (!ok) return;

            btnAdminLogin.Text    = "Signing in...";
            btnAdminLogin.Enabled = false;
            Application.DoEvents();

            var (success, message, admin) = _loginService.AdminLogin(txtAdminUser.Text.Trim(), txtAdminPass.Text);
            if (success)
            {
                AppState.CurrentAdmin = admin;
                Properties.Settings.Default.SavedUsername = chkRemember.Checked ? txtAdminUser.Text.Trim() : "";
                Properties.Settings.Default.Save();
                new Admin.AdminMainForm().Show();
                Hide();
            }
            else
            {
                lblAdminPassErr.Text    = "⚠ " + message;
                lblAdminPassErr.Visible = true;
                btnAdminLogin.Text      = "Sign In";
                btnAdminLogin.Enabled   = true;
            }
        }

        private void DoCustLogin()
        {
            bool ok = true;
            if (string.IsNullOrWhiteSpace(txtCustEmail.Text))
            { lblCustEmailErr.Text = "⚠ Email is required"; lblCustEmailErr.Visible = true; ok = false; }
            if (string.IsNullOrWhiteSpace(txtCustPass.Text))
            { lblCustPassErr.Text = "⚠ Password is required"; lblCustPassErr.Visible = true; ok = false; }
            if (!ok) return;

            btnCustLogin.Text    = "Signing in...";
            btnCustLogin.Enabled = false;
            Application.DoEvents();

            var (success, message, customer) = _loginService.CustomerLogin(txtCustEmail.Text.Trim(), txtCustPass.Text);
            if (success)
            {
                AppState.CurrentCustomer = customer;
                new Customer.CustomerMainForm().Show();
                Hide();
            }
            else
            {
                lblCustPassErr.Text    = "⚠ " + message;
                lblCustPassErr.Visible = true;
                btnCustLogin.Text      = "Sign In";
                btnCustLogin.Enabled   = true;
            }
        }

        private void SwitchTab(bool isAdmin)
        {
            _isAdminTab = isAdmin;

            // Active tab: white bg, blue text, blue bottom border
            // Inactive tab: light gray bg, muted text
            btnAdminTab.BackColor = isAdmin ? Color.White : Color.FromArgb(248, 250, 252);
            btnAdminTab.ForeColor = isAdmin ? AppTheme.Primary : AppTheme.TextMuted;
            btnCustTab.BackColor  = !isAdmin ? Color.White : Color.FromArgb(248, 250, 252);
            btnCustTab.ForeColor  = !isAdmin ? AppTheme.Primary : AppTheme.TextMuted;
            btnAdminTab.Font      = isAdmin  ? new Font("Segoe UI", 9.5f, FontStyle.Bold) : new Font("Segoe UI", 9.5f);
            btnCustTab.Font       = !isAdmin ? new Font("Segoe UI", 9.5f, FontStyle.Bold) : new Font("Segoe UI", 9.5f);
            btnAdminTab.Invalidate();
            btnCustTab.Invalidate();

            pnlAdminFields.Visible = isAdmin;
            pnlCustFields.Visible  = !isAdmin;
        }

        private void LoadSavedCredentials()
        {
            try
            {
                var saved = Properties.Settings.Default.SavedUsername;
                if (!string.IsNullOrEmpty(saved))
                {
                    txtAdminUser.Text   = saved;
                    chkRemember.Checked = true;
                }
            }
            catch { }
        }

        // ── Control factory helpers ───────────────────────────────
        private Label FieldLabel(string text, int x, int y) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize  = true,
            Location  = new Point(x, y)
        };

        private Panel InputBox(int fieldY, int panelTop) => new Panel
        {
            Location    = new Point(0, panelTop),
            Size        = new Size(480, 40),
            BackColor   = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        private TextBox InputText(int width) => new TextBox
        {
            BackColor   = Color.White,
            ForeColor   = AppTheme.TextPrimary,
            Font        = AppTheme.FontInput,
            Width       = width,
            Height      = 28,
            Location    = new Point(10, 5),
            BorderStyle = BorderStyle.None
        };

        private Button EyeBtn() => new Button
        {
            Text      = "👁",
            Location  = new Point(440, 5),
            Size      = new Size(34, 28),
            BackColor = Color.White,
            ForeColor = AppTheme.TextMuted,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Font      = new Font("Segoe UI Emoji", 11f),
            FlatAppearance = { BorderSize = 0 }
        };

        private Label ErrLabel(int x, int y) => new Label
        {
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.Danger,
            AutoSize  = true,
            Location  = new Point(x, y),
            Visible   = false
        };

        private Button MakeTabBtn(string text, bool active) => new Button
        {
            Text      = text,
            BackColor = active ? Color.White : Color.FromArgb(248, 250, 252),
            ForeColor = active ? AppTheme.Primary : AppTheme.TextMuted,
            Font      = active ? new Font("Segoe UI", 9.5f, FontStyle.Bold) : new Font("Segoe UI", 9.5f),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };

        private Button MakeSignInBtn(int x, int y)
        {
            var btn = new Button
            {
                Text      = "Sign In",
                Location  = new Point(x, y),
                Size      = new Size(480, 46),
                BackColor = AppTheme.Primary,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btn.MouseEnter += (s, e) => btn.BackColor = AppTheme.PrimaryHover;
            btn.MouseLeave += (s, e) => btn.BackColor = AppTheme.Primary;
            return btn;
        }
    }
}
