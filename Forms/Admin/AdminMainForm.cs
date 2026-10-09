using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Forms.Login;
using SmartMed.Services;
using SmartMed.UI;

namespace SmartMed.Forms.Admin
{
    public class AdminMainForm : Form
    {
        private Panel  pnlNav, pnlTopBar, pnlContent;
        private Label  lblPageTitle, lblAdminName, lblAdminRole, lblClock;
        private Button btnActive;
        private System.Windows.Forms.Timer timerClock;
        private readonly NotificationService _notifService = new NotificationService();
        private bool _isLoggingOut = false;

        public AdminMainForm()
        {
            InitForm();
            BuildLayout();
            NavigateTo("Dashboard");
            StartClock();
            CheckAlerts();
        }

        private void InitForm()
        {
            Text            = "SmartMed Pharmacy - Admin Dashboard";
            WindowState     = FormWindowState.Maximized;
            BackColor       = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize     = new Size(1200, 700);
            Font            = AppTheme.FontLabel;
        }

        private void BuildLayout()
        {
            // TableLayoutPanel guarantees exact cell positions – no DockStyle ambiguity.
            //
            //  col 0 (220px fixed)  |  col 1 (fill)
            //  ─────────────────────────────────────
            //  row 0 (61px, span 2) → pnlTopBar  full-width top bar
            //  row 1 (fill)         → pnlNav  |  pnlContent
            //
            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 2,
                ColumnCount = 2,
                Padding     = new Padding(0),
                Margin      = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AppTheme.NavWidth));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, AppTheme.TopBarHeight + 1));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent,  100f));

            // ── Top bar (spans both columns) ───────────────────────
            BuildTopBar();
            tbl.SetColumnSpan(pnlTopBar, 2);
            tbl.Controls.Add(pnlTopBar, 0, 0);

            // ── Sidebar ────────────────────────────────────────────
            BuildNav();
            tbl.Controls.Add(pnlNav, 0, 1);

            // ── Content ────────────────────────────────────────────
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            tbl.Controls.Add(pnlContent, 1, 1);

            Controls.Add(tbl);
        }

        // ── Sidebar ───────────────────────────────────────────────────
        private void BuildNav()
        {
            pnlNav = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.SidebarBg };

            // ── Background gradient ──────────────────────────────────────
            pnlNav.Paint += (s, e) =>
            {
                using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(0, pnlNav.Height),
                    AppTheme.SidebarBg, AppTheme.SidebarBg2);
                e.Graphics.FillRectangle(br, pnlNav.ClientRectangle);
                // Right-edge separator line
                using var pen = new Pen(Color.FromArgb(40, 255, 255, 255));
                e.Graphics.DrawLine(pen, pnlNav.Width - 1, 0, pnlNav.Width - 1, pnlNav.Height);
            };

            // ── Logo row (84px) — all drawn via Paint, fully reliable ────
            var pnlLogo = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(AppTheme.NavWidth, 84),
                BackColor = AppTheme.SidebarLogoBg
            };
            pnlLogo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // ── Icon box: rounded square with pharmacy cross ─────────
                int ix = 15, iy = 20, iw = 44, ih = 44, ir = 9;
                using var iconPath = new System.Drawing.Drawing2D.GraphicsPath();
                iconPath.AddArc(ix,          iy,          ir * 2, ir * 2, 180, 90);
                iconPath.AddArc(ix + iw - ir * 2, iy,          ir * 2, ir * 2, 270, 90);
                iconPath.AddArc(ix + iw - ir * 2, iy + ih - ir * 2, ir * 2, ir * 2,   0, 90);
                iconPath.AddArc(ix,          iy + ih - ir * 2, ir * 2, ir * 2,  90, 90);
                iconPath.CloseFigure();
                // White rounded box on blue background
                using var iconBg = new SolidBrush(Color.FromArgb(255, 255, 255));
                g.FillPath(iconBg, iconPath);

                // Cross symbol in blue (matches sidebar)
                using var crossPen = new Pen(Color.FromArgb(37, 99, 235), 2.8f)
                { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
                int cx = ix + iw / 2, cy = iy + ih / 2;
                g.DrawLine(crossPen, cx, cy - 10, cx, cy + 10);
                g.DrawLine(crossPen, cx - 10, cy, cx + 10, cy);

                // ── Brand text ────────────────────────────────────────────
                using var fBrand = new Font("Segoe UI", 13f, FontStyle.Bold);
                using var fSub   = new Font("Segoe UI",  8f);
                using var brW    = new SolidBrush(Color.White);
                using var brM    = new SolidBrush(Color.FromArgb(219, 234, 254)); // blue-100
                g.DrawString("SmartMed",    fBrand, brW, 67f, 21f);
                g.DrawString("Admin Portal", fSub,  brM, 68f, 46f);

                // Bottom separator
                using var sepPen = new Pen(AppTheme.SidebarSep);
                g.DrawLine(sepPen, 0, pnlLogo.Height - 1, pnlLogo.Width, pnlLogo.Height - 1);
            };
            pnlNav.Controls.Add(pnlLogo);

            // ── Nav items ────────────────────────────────────────────────
            var navItems = new[]
            {
                ("🏠", "Dashboard"),
                ("💊", "Medicines"),
                ("📦", "Orders"),
                ("👥", "Customers"),
                ("🏭", "Suppliers"),
                ("🏷", "Categories"),
                ("💰", "Discounts"),
                ("📊", "Reports"),
                ("⚙", "Settings")
            };

            int navY = 94;
            foreach (var (icon, name) in navItems)
            {
                var btn = MakeNavBtn(icon, name);
                btn.Location = new Point(0, navY);
                btn.Click   += (s, e) => NavigateTo(name);
                pnlNav.Controls.Add(btn);
                navY += 44;
            }

            // ── Footer: admin info + separator + logout ──────────────────
            var sepBot = new Panel
            {
                Size      = new Size(AppTheme.NavWidth, 1),
                BackColor = AppTheme.SidebarSep,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Admin avatar + name row
            var pnlAdminInfo = new Panel
            {
                Size      = new Size(AppTheme.NavWidth, 54),
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
            };
            pnlAdminInfo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Avatar circle — white with blue initials
                using var avBr = new SolidBrush(Color.FromArgb(70, 255, 255, 255));
                g.FillEllipse(avBr, 12, 12, 32, 32);
                using var avBorder = new Pen(Color.FromArgb(120, 255, 255, 255), 1.5f);
                g.DrawEllipse(avBorder, 12, 12, 32, 32);
                string initials = "A";
                var adm = AppState.CurrentAdmin;
                if (adm?.FullName?.Length > 0) initials = adm.FullName[0].ToString().ToUpper();
                using var fInit = new Font("Segoe UI", 11f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(initials, fInit, Brushes.White, new RectangleF(12, 12, 32, 32), sf);
                // Name + role
                using var fName = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var fRole = new Font("Segoe UI", 7.5f);
                using var brW   = new SolidBrush(Color.White);
                using var brM   = new SolidBrush(Color.FromArgb(219, 234, 254)); // blue-100
                string name = adm?.FullName ?? "Admin User";
                string role = adm?.Role ?? "Administrator";
                if (name.Length > 16) name = name.Substring(0, 14) + "…";
                g.DrawString(name, fName, brW, 52f, 15f);
                g.DrawString(role, fRole, brM, 53f, 34f);
            };

            var btnLogout = new Button
            {
                Text      = "  ←  Sign Out",
                Size      = new Size(AppTheme.NavWidth, 42),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(248, 113, 113),  // red-400
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(35, 239, 68, 68) }
            };
            btnLogout.Click += Logout_Click;

            pnlNav.Controls.AddRange(new Control[] { sepBot, pnlAdminInfo, btnLogout });

            void Pin()
            {
                btnLogout.Location    = new Point(0, pnlNav.Height - 42);
                pnlAdminInfo.Location = new Point(0, pnlNav.Height - 96);
                sepBot.Location       = new Point(0, pnlNav.Height - 100);
            }
            pnlNav.SizeChanged += (s, e) => Pin();
            Load               += (s, e) => Pin();
        }

        private Button MakeNavBtn(string icon, string name)
        {
            var btn = new Button
            {
                Text      = $"      {icon}   {name}",
                Size      = new Size(AppTheme.NavWidth, 44),
                BackColor = Color.Transparent,
                ForeColor = AppTheme.SidebarText,
                Font      = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Tag       = name,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = AppTheme.SidebarHover }
            };
            // Draw left accent bar for active state
            btn.Paint += (s, e) =>
            {
                if (btn == btnActive)
                {
                    using var bar = new SolidBrush(AppTheme.SidebarActive);
                    e.Graphics.FillRectangle(bar, 0, 0, 3, btn.Height);
                }
            };
            btn.MouseEnter += (s, e) => { if (btn != btnActive) { btn.BackColor = AppTheme.SidebarHover; btn.ForeColor = Color.White; } };
            btn.MouseLeave += (s, e) => { if (btn != btnActive) { btn.BackColor = Color.Transparent; btn.ForeColor = AppTheme.SidebarText; } };
            return btn;
        }

        // ── Top bar ───────────────────────────────────────────────────
        private void BuildTopBar()
        {
            pnlTopBar = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White
            };

            // Bottom border of topbar
            pnlTopBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new System.Drawing.Pen(AppTheme.Border), 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);

            lblPageTitle = new Label
            {
                Text      = "Dashboard",
                Font      = AppTheme.FontHeader,
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(24, 16)
            };

            lblClock = new Label
            {
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(600, 22)
            };

            var pnlAdmin = new Panel { Size = new Size(200, 50), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            lblAdminName = new Label { Text = AppState.CurrentAdmin?.FullName ?? "Admin", Font = AppTheme.FontBold, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, 8) };
            lblAdminRole = new Label { Text = AppState.CurrentAdmin?.Role ?? "Administrator", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(0, 28) };
            pnlAdmin.Controls.AddRange(new Control[] { lblAdminName, lblAdminRole });

            pnlTopBar.Controls.AddRange(new Control[] { lblPageTitle, lblClock, pnlAdmin });
            pnlTopBar.SizeChanged += (s, e) =>
            {
                pnlAdmin.Location = new Point(pnlTopBar.Width - 210, 5);
                lblClock.Location = new Point(pnlTopBar.Width - 420, 20);
            };
        }

        // ── Navigation ────────────────────────────────────────────────
        public void NavigateTo(string page)
        {
            lblPageTitle.Text = page;
            pnlContent.Controls.Clear();

            foreach (Control c in pnlNav.Controls)
                if (c is Button b && b.Tag?.ToString() == page)
                { SetActive(b); break; }

            Control panel = page switch
            {
                "Dashboard"  => new DashboardPanel(this),
                "Medicines"  => new MedicinePanel(),
                "Orders"     => new OrderPanel(),
                "Customers"  => new CustomerPanel(),
                "Suppliers"  => new SupplierPanel(),
                "Categories" => new CategoryPanel(),
                "Discounts"  => new DiscountPanel(),
                "Reports"    => new ReportPanel(),
                "Settings"   => new SettingsPanel(),
                _            => new DashboardPanel(this)
            };

            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
        }

        private void SetActive(Button btn)
        {
            if (btnActive != null)
            {
                btnActive.BackColor = Color.Transparent;
                btnActive.ForeColor = AppTheme.SidebarText;
                btnActive.Font      = new Font("Segoe UI", 9.5f);
                btnActive.Invalidate();
            }
            btnActive = btn;
            btnActive.BackColor = AppTheme.SidebarActiveBg;  // subtle indigo tint
            btnActive.ForeColor = Color.White;
            btnActive.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnActive.Invalidate();  // triggers Paint → draws left bar
        }

        private void StartClock()
        {
            timerClock = new System.Windows.Forms.Timer { Interval = 1000 };
            timerClock.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("ddd, dd MMM  HH:mm:ss");
            timerClock.Start();
        }

        private void CheckAlerts()
        {
            try
            {
                var alerts = _notifService.GetAlerts();
                if (alerts.Count > 0)
                    MessageBox.Show(string.Join(Environment.NewLine, alerts), "SmartMed Alerts",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch { }
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _isLoggingOut = true;
                timerClock?.Stop();
                AppState.ClearAdmin();
                new LoginForm().Show();
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timerClock?.Stop();
            base.OnFormClosing(e);
            if (!_isLoggingOut)
                Application.Exit();
        }
    }
}
