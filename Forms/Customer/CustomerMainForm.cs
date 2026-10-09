using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.UI;

namespace SmartMed.Forms.Customer
{
    public class CustomerMainForm : Form
    {
        private Panel  pnlNav, pnlTopBar, pnlContent;
        private Label  lblPageTitle, lblCartCount, lblCustName;
        private Button btnActive;
        private bool   _isLoggingOut = false;

        public CustomerMainForm()
        {
            InitForm();
            BuildLayout();
            NavigateTo("My Dashboard");
        }

        private void InitForm()
        {
            Text        = "SmartMed - Customer Portal";
            WindowState = FormWindowState.Maximized;
            BackColor   = AppTheme.Background;
            MinimumSize = new Size(1100, 680);
            Font        = AppTheme.FontLabel;
        }

        private void BuildLayout()
        {
            // TableLayoutPanel: col 0 = sidebar (220px), col 1 = content (fill)
            //                   row 0 = topbar spanning 2 cols, row 1 = sidebar+content
            var tbl = new TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                RowCount        = 2,
                ColumnCount     = 2,
                Padding         = new Padding(0),
                Margin          = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AppTheme.NavWidth));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, AppTheme.TopBarHeight + 1));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent,  100f));

            BuildTopBar();
            tbl.SetColumnSpan(pnlTopBar, 2);
            tbl.Controls.Add(pnlTopBar, 0, 0);

            BuildNav();
            tbl.Controls.Add(pnlNav, 0, 1);

            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            tbl.Controls.Add(pnlContent, 1, 1);

            Controls.Add(tbl);
        }

        private void BuildNav()
        {
            pnlNav = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.SidebarBg };

            // ── Background gradient ───────────────────────────────────────
            pnlNav.Paint += (s, e) =>
            {
                using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(0, pnlNav.Height),
                    AppTheme.SidebarBg, AppTheme.SidebarBg2);
                e.Graphics.FillRectangle(br, pnlNav.ClientRectangle);
                using var pen = new Pen(Color.FromArgb(40, 255, 255, 255));
                e.Graphics.DrawLine(pen, pnlNav.Width - 1, 0, pnlNav.Width - 1, pnlNav.Height);
            };

            // ── Logo row — fully painted, no child label controls ─────────
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

                // Icon box: rounded square with green cross (customer = teal/green)
                int ix = 15, iy = 20, iw = 44, ih = 44, ir = 9;
                using var iconPath = new System.Drawing.Drawing2D.GraphicsPath();
                iconPath.AddArc(ix,               iy,               ir * 2, ir * 2, 180, 90);
                iconPath.AddArc(ix + iw - ir * 2, iy,               ir * 2, ir * 2, 270, 90);
                iconPath.AddArc(ix + iw - ir * 2, iy + ih - ir * 2, ir * 2, ir * 2,   0, 90);
                iconPath.AddArc(ix,               iy + ih - ir * 2, ir * 2, ir * 2,  90, 90);
                iconPath.CloseFigure();
                // White box on blue background
                using var iconBg = new SolidBrush(Color.White);
                g.FillPath(iconBg, iconPath);

                // Cross in blue (matches sidebar)
                using var crossPen = new Pen(Color.FromArgb(37, 99, 235), 2.8f)
                { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
                int cx = ix + iw / 2, cy = iy + ih / 2;
                g.DrawLine(crossPen, cx, cy - 10, cx, cy + 10);
                g.DrawLine(crossPen, cx - 10, cy, cx + 10, cy);

                using var fBrand = new Font("Segoe UI", 13f, FontStyle.Bold);
                using var fSub   = new Font("Segoe UI",  8f);
                using var brW    = new SolidBrush(Color.White);
                using var brM    = new SolidBrush(Color.FromArgb(219, 234, 254)); // blue-100
                g.DrawString("SmartMed",       fBrand, brW, 67f, 21f);
                g.DrawString("Customer Portal", fSub,  brM, 68f, 46f);

                using var sepPen = new Pen(AppTheme.SidebarSep);
                g.DrawLine(sepPen, 0, pnlLogo.Height - 1, pnlLogo.Width, pnlLogo.Height - 1);
            };
            pnlNav.Controls.Add(pnlLogo);

            var sepTop = new Panel { Location = new Point(0, 84), Size = new Size(AppTheme.NavWidth, 0) }; // placeholder

            var navItems = new[]
            {
                ("🏠", "My Dashboard"),
                ("🔍", "Search Medicine"),
                ("🛒", "My Cart"),
                ("📦", "My Orders"),
                ("👤", "My Profile")
            };

            int navY = 94;
            foreach (var (icon, name) in navItems)
            {
                var btn = new Button
                {
                    Text      = $"      {icon}   {name}",
                    Size      = new Size(AppTheme.NavWidth, 44),
                    Location  = new Point(0, navY),
                    BackColor = Color.Transparent,
                    ForeColor = AppTheme.SidebarText,
                    Font      = new Font("Segoe UI", 9.5f),
                    FlatStyle = FlatStyle.Flat,
                    Cursor    = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Tag       = name,
                    FlatAppearance = { BorderSize = 0, MouseOverBackColor = AppTheme.SidebarHover }
                };
                btn.Paint += (s, e) =>
                {
                    if (btn == btnActive)
                    {
                        using var bar = new SolidBrush(Color.White); // white bar on blue sidebar
                        e.Graphics.FillRectangle(bar, 0, 0, 3, btn.Height);
                    }
                };
                btn.Click      += (s, e) => NavigateTo(name);
                btn.MouseEnter += (s, e) => { if (btn != btnActive) { btn.BackColor = AppTheme.SidebarHover; btn.ForeColor = Color.White; } };
                btn.MouseLeave += (s, e) => { if (btn != btnActive) { btn.BackColor = Color.Transparent; btn.ForeColor = AppTheme.SidebarText; } };
                pnlNav.Controls.Add(btn);
                navY += 44;
            }

            // ── Footer: user info + separator + logout ────────────────────
            var sepBot = new Panel { Size = new Size(AppTheme.NavWidth, 1), BackColor = AppTheme.SidebarSep, Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };

            var pnlUserInfo = new Panel
            {
                Size      = new Size(AppTheme.NavWidth, 54),
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
            };
            pnlUserInfo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Avatar: white semi-transparent circle on blue
                using var avBr = new SolidBrush(Color.FromArgb(70, 255, 255, 255));
                g.FillEllipse(avBr, 12, 12, 32, 32);
                using var avBorder = new Pen(Color.FromArgb(120, 255, 255, 255), 1.5f);
                g.DrawEllipse(avBorder, 12, 12, 32, 32);
                string initials = "C";
                var cust = AppState.CurrentCustomer;
                if (cust?.FirstName?.Length > 0) initials = cust.FirstName[0].ToString().ToUpper();
                using var fInit = new Font("Segoe UI", 11f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(initials, fInit, Brushes.White, new RectangleF(12, 12, 32, 32), sf);
                using var fName = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var fSub  = new Font("Segoe UI", 7.5f);
                using var brW   = new SolidBrush(Color.White);
                using var brM   = new SolidBrush(Color.FromArgb(219, 234, 254)); // blue-100
                var cst = AppState.CurrentCustomer;
                string nm = cst != null ? $"{cst.FirstName} {cst.LastName}" : "Customer";
                if (nm.Length > 16) nm = nm.Substring(0, 14) + "…";
                g.DrawString(nm, fName, brW, 52f, 15f);
                g.DrawString("Customer Account", fSub, brM, 53f, 34f);
            };

            var btnLogout = new Button
            {
                Text      = "  ←  Sign Out",
                Size      = new Size(AppTheme.NavWidth, 42),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(248, 113, 113),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(35, 239, 68, 68) }
            };
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _isLoggingOut = true;
                    AppState.ClearCustomer();
                    new Login.LoginForm().Show();
                    Close();
                }
            };

            pnlNav.Controls.AddRange(new Control[] { sepBot, pnlUserInfo, btnLogout });

            void Pin()
            {
                btnLogout.Location    = new Point(0, pnlNav.Height - 42);
                pnlUserInfo.Location  = new Point(0, pnlNav.Height - 96);
                sepBot.Location       = new Point(0, pnlNav.Height - 100);
            }
            pnlNav.SizeChanged += (s, e) => Pin();
            Load               += (s, e) => Pin();
        }

        private void BuildTopBar()
        {
            pnlTopBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            pnlTopBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new System.Drawing.Pen(AppTheme.Border), 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);

            lblPageTitle = new Label { Text = "Dashboard", Font = AppTheme.FontHeader, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(20, 16) };

            lblCustName = new Label
            {
                Text      = $"Hello, {AppState.CurrentCustomer?.FirstName}!",
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };

            lblCartCount = new Label
            {
                Text      = "🛒 0",
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.Warning,
                AutoSize  = true,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            lblCartCount.Click += (s, e) => NavigateTo("My Cart");

            pnlTopBar.Controls.AddRange(new Control[] { lblPageTitle, lblCustName, lblCartCount });
            pnlTopBar.SizeChanged += (s, e) =>
            {
                lblCustName.Location  = new Point(pnlTopBar.Width - 220, 20);
                lblCartCount.Location = new Point(pnlTopBar.Width - 80,  20);
            };
        }

        public void NavigateTo(string page)
        {
            lblPageTitle.Text = page;
            UpdateCartBadge();
            pnlContent.Controls.Clear();

            foreach (Control c in pnlNav.Controls)
                if (c is Button nb && nb.Tag?.ToString() == page)
                { SetActive(nb); break; }

            Control panel = page switch
            {
                "My Dashboard"    => new CustomerDashboardPanel(this),
                "Search Medicine" => new SearchMedicinePanel(this),
                "My Cart"         => new CartPanel(this),
                "My Orders"       => new MyOrdersPanel(),
                "My Profile"      => new CustomerProfilePanel(),
                _                 => new CustomerDashboardPanel(this)
            };
            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
        }

        public void UpdateCartBadge()
        {
            int count = AppState.CartCount();
            lblCartCount.Text      = $"🛒 {count}";
            lblCartCount.ForeColor = count > 0 ? AppTheme.Warning : AppTheme.TextMuted;
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
            btnActive.BackColor = AppTheme.SidebarActiveBg;  // white tint on blue
            btnActive.ForeColor = Color.White;
            btnActive.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnActive.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!_isLoggingOut)
                Application.Exit();
        }
    }
}
