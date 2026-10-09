using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;

namespace SmartMed.Forms.Customer
{
    public class CustomerDashboardPanel : UserControl
    {
        private readonly CustomerMainForm _main;
        private readonly OrderRepository  _orderRepo = new OrderRepository();

        // panels that need to fill the width on resize
        private Panel _pnlStats;
        private Panel _pnlQA;
        private Panel _pnlRO;
        private Panel[] _statCards = new Panel[5];

        public CustomerDashboardPanel(CustomerMainForm main)
        {
            _main      = main;
            BackColor  = AppTheme.Background;
            Dock       = DockStyle.Fill;
            AutoScroll = true;
            Build();
            // Suppress horizontal scrollbar
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            AutoScrollMinSize = new Size(1, AutoScrollMinSize.Height);
            SizeChanged += (s, e) => AdjustLayout();
        }

        // ── Dynamic width adjust ─────────────────────────────────
        private void AdjustLayout()
        {
            int w = Math.Max(ClientSize.Width - 40, 760); // 20px margin each side

            // Stat cards row — share width equally
            if (_pnlStats != null)
            {
                _pnlStats.Width = w;
                int cw = (w - 4 * 12) / 5;   // 4 gaps of 12px
                for (int i = 0; i < 5; i++)
                {
                    if (_statCards[i] == null) continue;
                    _statCards[i].Location = new Point(i * (cw + 12), 0);
                    _statCards[i].Width    = cw;
                }
            }
            if (_pnlQA != null) _pnlQA.Width = w;
            if (_pnlRO != null) _pnlRO.Width = w;
        }

        private void Build()
        {
            var cust = AppState.CurrentCustomer;
            if (cust == null) return;

            var orders = _orderRepo.GetAll(customerId: cust.CustomerID);
            int active = 0, completed = 0;
            decimal spent = 0;
            foreach (var o in orders)
            {
                if (o.Status == "Pending" || o.Status == "Approved" || o.Status == "Ready") active++;
                if (o.Status == "Delivered") { completed++; spent += o.FinalAmount; }
            }
            int cartCount = AppState.CartCount();

            // ── Welcome header ───────────────────────────────────
            Controls.Add(new Label
            {
                Text      = $"Welcome, {cust.FirstName} {cust.LastName}!",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(20, 16)
            });
            Controls.Add(new Label
            {
                Text      = $"Member since {cust.RegisteredDate:MMMM yyyy}",
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(20, 46)
            });

            // ── Stat cards row ───────────────────────────────────
            // Container panel — width adjusted dynamically
            _pnlStats = new Panel
            {
                Location  = new Point(20, 72),
                Size      = new Size(900, 88),
                BackColor = Color.Transparent
            };

            string[] titles  = { "Total Orders", "Active Orders", "Completed", "Total Spent", "Cart Items" };
            string[] values  = { orders.Count.ToString(), active.ToString(), completed.ToString(), $"Rs. {spent:N0}", cartCount.ToString() };
            string[] icons   = { "📦", "⏳", "✅", "💰", "🛒" };
            Color[]  accents = { AppTheme.Primary, AppTheme.Warning, AppTheme.Success, AppTheme.Accent, Color.FromArgb(6, 182, 212) };

            for (int i = 0; i < 5; i++)
            {
                _statCards[i] = MakeStatCard(titles[i], values[i], icons[i], accents[i]);
                _pnlStats.Controls.Add(_statCards[i]);
            }
            Controls.Add(_pnlStats);

            // ── Quick Actions card ───────────────────────────────
            int sy = 72 + 88 + 10;
            _pnlQA = MakeCard(20, sy, 900, 96, "⚡  Quick Actions");

            int bx = 14, by = 46, bw = 178, bh = 38;
            AddBtn(_pnlQA, "🔍 Search Medicine", AppTheme.Primary,          bx,               by, bw, bh, () => _main.NavigateTo("Search Medicine"));
            AddBtn(_pnlQA, "🛒 View Cart",       AppTheme.Warning,          bx+(bw+12),       by, bw, bh, () => _main.NavigateTo("My Cart"));
            AddBtn(_pnlQA, "📦 My Orders",       AppTheme.Success,          bx+(bw+12)*2,     by, bw, bh, () => _main.NavigateTo("My Orders"));
            AddBtn(_pnlQA, "👤 My Profile",      Color.FromArgb(99,102,241),bx+(bw+12)*3,     by, bw, bh, () => _main.NavigateTo("My Profile"));
            Controls.Add(_pnlQA);

            // ── Recent Orders card ───────────────────────────────
            sy += 96 + 10;
            int gridH = 200;
            _pnlRO = MakeCard(20, sy, 900, gridH + 44, "🛒  Recent Orders");

            if (orders.Count == 0)
            {
                _pnlRO.Controls.Add(new Label
                {
                    Text      = "📭\r\nYou haven't placed any orders yet.",
                    Font      = new Font("Segoe UI", 10.5f),
                    ForeColor = AppTheme.TextMuted,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(1, 44),
                    Size      = new Size(898, gridH),
                    BackColor = Color.Transparent,
                    Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                });
            }
            else
            {
                int rowCount = Math.Min(8, orders.Count);
                var dt = new System.Data.DataTable();
                dt.Columns.AddRange(new[]
                {
                    new System.Data.DataColumn("Order ID"),
                    new System.Data.DataColumn("Date"),
                    new System.Data.DataColumn("Items"),
                    new System.Data.DataColumn("Amount"),
                    new System.Data.DataColumn("Status")
                });
                for (int i = 0; i < rowCount; i++)
                {
                    var o = orders[i];
                    dt.Rows.Add(
                        $"#{o.OrderID:D5}",
                        o.OrderDate.ToString("dd MMM yyyy"),
                        o.ItemCount.ToString(),
                        $"Rs. {o.FinalAmount:F2}",
                        o.Status);
                }

                var dgv = new DataGridView
                {
                    Location            = new Point(1, 44),
                    Size                = new Size(898, gridH),
                    Anchor              = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                    ScrollBars          = ScrollBars.Vertical,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };
                AppTheme.StyleDataGrid(dgv);
                dgv.RowTemplate.Height = 38;
                dgv.CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal;
                dgv.GridColor         = Color.FromArgb(235, 235, 235);
                dgv.DataSource        = dt;

                // Proportional fill weights — no horizontal overflow
                if (dgv.Columns.Count == 5)
                {
                    dgv.Columns["Order ID"].FillWeight = 12;
                    dgv.Columns["Date"].FillWeight     = 20;
                    dgv.Columns["Items"].FillWeight    = 9;
                    dgv.Columns["Amount"].FillWeight   = 18;
                    dgv.Columns["Status"].FillWeight   = 14;
                    dgv.Columns["Items"].DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
                    dgv.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgv.Columns["Amount"].DefaultCellStyle.Padding   = new Padding(0, 0, 10, 0);
                }

                dgv.CellFormatting += (s, e) =>
                {
                    if (e.RowIndex < 0 || e.RowIndex >= rowCount) return;
                    string col = dgv.Columns[e.ColumnIndex].Name;
                    if (col == "Status" && e.Value != null)
                    {
                        // No BackColor — semi-transparent alpha causes double-render glitch in DataGridView
                        e.CellStyle.ForeColor = StatusColor(e.Value.ToString());
                        e.CellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    }
                    else if (col == "Order ID")
                    {
                        e.CellStyle.ForeColor = AppTheme.Primary;
                        e.CellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    }
                };
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

                // "View All →" button in header
                var btnAll = new Button
                {
                    Text      = "View All →",
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = AppTheme.Primary,
                    BackColor = Color.Transparent,
                    Cursor    = Cursors.Hand,
                    AutoSize  = true,
                    Location  = new Point(_pnlRO.Width - 85, 13),
                    Anchor    = AnchorStyles.Right | AnchorStyles.Top
                };
                btnAll.FlatAppearance.BorderSize = 0;
                btnAll.Click += (s, e) => _main.NavigateTo("My Orders");
                _pnlRO.Controls.Add(btnAll);
                _pnlRO.Controls.Add(dgv);
            }
            Controls.Add(_pnlRO);
        }

        // ── Helpers ───────────────────────────────────────────────
        private Panel MakeStatCard(string title, string value, string icon, Color accent)
        {
            var card = new Panel { Size = new Size(170, 88), BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var border = new Pen(AppTheme.Border);
                g.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
                using var bar  = new SolidBrush(accent);
                g.FillRectangle(bar, 0, 0, 4, card.Height);
                using var tint = new SolidBrush(Color.FromArgb(14, accent));
                g.FillRectangle(tint, card.Width - 46, 0, 46, card.Height);
            };
            card.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI Emoji", 15f), ForeColor = accent, AutoSize = true, Location = new Point(card.Width - 38, 8), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(10, 14), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = title, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(10, 52), BackColor = Color.Transparent });
            card.Controls.Add(new Panel { Location = new Point(10, 76), Size = new Size(36, 3), BackColor = accent });
            return card;
        }

        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            var hdr = new Panel { Location = new Point(0, 0), Size = new Size(w, 43), BackColor = Color.White, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            hdr.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
            };
            hdr.Controls.Add(new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(14, 13),
                BackColor = Color.Transparent
            });
            card.Controls.Add(hdr);
            return card;
        }

        private void AddBtn(Panel card, string text, Color color, int x, int y, int w, int h, Action onClick)
        {
            var btn = AppTheme.CreateButton(text, color, w, h);
            btn.Location = new Point(x, y);
            btn.Click   += (s, e) => onClick();
            card.Controls.Add(btn);
        }

        private static Color StatusColor(string status) => status switch
        {
            "Pending"   => AppTheme.Warning,
            "Approved"  => AppTheme.Primary,
            "Ready"     => Color.FromArgb(6, 182, 212),
            "Delivered" => AppTheme.Success,
            "Cancelled" => AppTheme.Danger,
            "Rejected"  => AppTheme.Danger,
            _           => AppTheme.TextMuted
        };
    }
}
