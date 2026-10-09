using System;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;

namespace SmartMed.Forms.Admin
{
    public class DashboardPanel : UserControl
    {
        private readonly AdminMainForm     _mainForm;
        private readonly OrderRepository   _orderRepo = new OrderRepository();
        private readonly MedicineRepository _medRepo  = new MedicineRepository();
        private readonly CustomerRepository _custRepo = new CustomerRepository();

        public DashboardPanel(AdminMainForm mainForm)
        {
            _mainForm  = mainForm;
            BackColor  = AppTheme.Background;
            Dock       = DockStyle.Fill;
            AutoScroll = true;
            Build();
            // Vertical scroll only — suppress horizontal scrollbar
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            AutoScrollMinSize = new Size(1, AutoScrollMinSize.Height);
        }

        private void Build()
        {
            decimal todaySales   = _orderRepo.GetTodaySales();
            decimal monthRevenue = _orderRepo.GetMonthlyRevenue();
            int totalCusts       = _custRepo.GetTotalCount();
            int totalMeds        = _medRepo.GetTotalCount();
            int expiredMeds      = _medRepo.GetExpiredCount();
            int lowStock         = _medRepo.GetLowStockCount();
            int pendingOrders    = _orderRepo.GetPendingCount();
            int completedOrders  = _orderRepo.GetCompletedCount();

            // Welcome
            Controls.Add(new Label
            {
                Text      = $"Welcome back, {AppState.CurrentAdmin?.FullName ?? "Admin"}!",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(10, 10)
            });
            Controls.Add(new Label
            {
                Text      = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(10, 42)
            });

            // ── Stat Cards row 1 ─────────────────────────────────
            // cw=185, gap=14 → 5 cards total width = 20 + 5*185 + 4*14 = 1001px (fits 1060px content)
            int cw = 185, ch = 100, gap = 14, sx = 20, sy = 78;
            AddCard("Today's Sales",    $"Rs. {todaySales:N0}",    AppTheme.Primary, sx,                sy, "💰");
            AddCard("Monthly Revenue",  $"Rs. {monthRevenue:N0}",  AppTheme.Accent,  sx+(cw+gap),       sy, "📈");
            AddCard("Total Customers",  totalCusts.ToString(),     AppTheme.Success, sx+(cw+gap)*2,     sy, "👥");
            AddCard("Total Medicines",  totalMeds.ToString(),      Color.FromArgb(124,58,237), sx+(cw+gap)*3, sy, "💊");
            AddCard("Pending Orders",   pendingOrders.ToString(),  AppTheme.Warning, sx+(cw+gap)*4,     sy, "📦");

            // ── Stat Cards row 2 ─────────────────────────────────
            sy += ch + gap;
            AddCard("Delivered Orders", completedOrders.ToString(), AppTheme.Success, sx,             sy, "✅");
            AddCard("Low Stock Items",  lowStock.ToString(),        AppTheme.Warning, sx+(cw+gap),    sy, "⚠");
            AddCard("Expired Medicines",expiredMeds.ToString(),     AppTheme.Danger,  sx+(cw+gap)*2,  sy, "🗑");

            int cardH = 235;

            // ── Top Selling & Recent Orders ───────────────────────
            sy += ch + gap + 10;

            // Top Selling card  (x=20, w=565 → right edge 585)
            var cardTop    = CreateSectionCard(sx, sy, 565, cardH, "📊  Top Selling Medicines", "Medicines");
            var topSelling = _medRepo.GetTopSelling(8);
            if (topSelling.Rows.Count == 0)
            {
                cardTop.Controls.Add(CreateTopSellingEmpty(563, cardH - 45));
            }
            else
            {
                var dgvTop = CreateTopSellingGrid(1, 44, 563, cardH - 45, topSelling);
                cardTop.Controls.Add(dgvTop);
            }
            Controls.Add(cardTop);

            // Recent Orders card  (x=600, w=410 → right edge 1010)
            var cardOrders = CreateSectionCard(600, sy, 410, cardH, "🛒  Recent Orders", "Orders");
            var orders = _orderRepo.GetAll();
            var dt = new System.Data.DataTable();
            dt.Columns.AddRange(new[]
            {
                new System.Data.DataColumn("OrderNo"),
                new System.Data.DataColumn("Customer"),
                new System.Data.DataColumn("Amount"),
                new System.Data.DataColumn("Status")
            });
            int n = Math.Min(10, orders.Count);
            for (int i = 0; i < n; i++)
            {
                var o = orders[i];
                dt.Rows.Add($"#{o.OrderID:D5}", o.CustomerName, $"Rs. {o.FinalAmount:F2}", o.Status);
            }
            if (dt.Rows.Count == 0)
                cardOrders.Controls.Add(CreateEmptyState(408, cardH - 45, "No recent orders"));
            else
            {
                var dgvOrders = CreateRecentOrdersGrid(1, 44, 408, cardH - 45, dt);
                cardOrders.Controls.Add(dgvOrders);
            }
            Controls.Add(cardOrders);

            // ── Expired Medicines (full-width row) ────────────────
            sy += cardH + 14;
            int expiredCardH = 175;
            var cardExpired = CreateSectionCard(sx, sy, 989, expiredCardH, "🗑  Expired Medicines", "Medicines");
            var expiredList = _medRepo.GetExpired(8);
            if (expiredList.Rows.Count == 0)
            {
                cardExpired.Controls.Add(CreateEmptyState(987, expiredCardH - 45, "No expired medicines"));
            }
            else
            {
                var listExpired = CreateExpiredList(987, expiredCardH - 45, expiredList);
                cardExpired.Controls.Add(listExpired);
            }
            Controls.Add(cardExpired);

            // ── Quick Actions ─────────────────────────────────────
            sy += expiredCardH + 14;
            AddSectionLabel("Quick Actions", sx, sy);
            sy += 30;
            AddAction("Add Medicine",  AppTheme.Primary,  new Point(sx,       sy), () => _mainForm.NavigateTo("Medicines"));
            AddAction("View Orders",   AppTheme.Success,  new Point(sx+155,   sy), () => _mainForm.NavigateTo("Orders"));
            AddAction("Customers",     AppTheme.Accent,   new Point(sx+310,   sy), () => _mainForm.NavigateTo("Customers"));
            AddAction("View Reports",  AppTheme.Warning,  new Point(sx+465,   sy), () => _mainForm.NavigateTo("Reports"));

            // Bottom spacer so buttons never touch the footer
            Controls.Add(new Panel { Location = new Point(0, sy + 70), Size = new Size(1, 24), BackColor = Color.Transparent });
        }

        private void AddCard(string title, string value, Color accent, int x, int y, string icon)
        {
            var card = new Panel
            {
                Location  = new Point(x, y),
                Size      = new Size(185, 100),
                BackColor = Color.White
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(AppTheme.Border, 1))
                    g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var br = new SolidBrush(accent))
                    g.FillRectangle(br, 0, 0, 4, card.Height);
                using (var br2 = new SolidBrush(Color.FromArgb(15, accent)))
                    g.FillRectangle(br2, card.Width - 58, 0, 58, card.Height);
            };

            card.Controls.Add(new Label
            {
                Text      = icon,
                Font      = new Font("Segoe UI Emoji", 18f),
                ForeColor = accent,
                AutoSize  = true,
                Location  = new Point(card.Width - 44, 10),
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(12, 18),
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text      = title,
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(12, 58),
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Panel
            {
                Location  = new Point(12, 80),
                Size      = new Size(50, 3),
                BackColor = accent
            });

            Controls.Add(card);
        }

        private void AddSectionLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text      = text,
                Font      = AppTheme.FontHeader,
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(x, y)
            });
        }

        // ── Recent Orders grid — explicit columns sized to fit exactly,
        //    no horizontal scrollbar, no stray selection highlight ──────
        private DataGridView CreateRecentOrdersGrid(int x, int y, int w, int h,
            System.Data.DataTable source)
        {
            var dgv = new DataGridView
            {
                Location              = new Point(x, y),
                Size                  = new Size(w, h),
                AutoGenerateColumns   = false,
                ColumnHeadersVisible  = true,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                ScrollBars            = ScrollBars.Vertical,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                GridColor             = Color.FromArgb(229, 231, 235),
                Font                  = new Font("Segoe UI", 9f),
                RowTemplate           = { Height = 32 },
                DefaultCellStyle =
                {
                    BackColor        = Color.White,
                    ForeColor        = AppTheme.TextPrimary,
                    SelectionBackColor = Color.White,
                    SelectionForeColor = AppTheme.TextPrimary,
                    Padding          = new Padding(4, 0, 4, 0)
                },
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor          = Color.FromArgb(248, 249, 250),
                    ForeColor          = AppTheme.TextMuted,
                    SelectionBackColor = Color.FromArgb(248, 249, 250),
                    SelectionForeColor = AppTheme.TextMuted,
                    Font               = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Padding            = new Padding(6, 0, 0, 0)
                },
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor          = Color.FromArgb(250, 251, 252),
                    SelectionBackColor = Color.FromArgb(250, 251, 252),
                    SelectionForeColor = AppTheme.TextPrimary
                }
            };
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgv.ColumnHeadersHeight = 32;
            dgv.EnableHeadersVisualStyles = false;

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Order #", DataPropertyName = "OrderNo", Width = 66,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = { ForeColor = AppTheme.Primary, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Customer", DataPropertyName = "Customer",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amount", DataPropertyName = "Amount", Width = 90,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Status", DataPropertyName = "Status", Width = 82,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }
            });

            dgv.CellFormatting += (s, e) =>
            {
                if (dgv.Columns[e.ColumnIndex].DataPropertyName != "Status" || e.RowIndex < 0 || e.Value == null) return;
                string status = e.Value.ToString();
                e.CellStyle.ForeColor = status == "Completed" ? AppTheme.Success
                    : status == "Pending"   ? AppTheme.Warning
                    : status == "Cancelled" ? AppTheme.Danger
                    : AppTheme.TextPrimary;
            };

            dgv.DataSource = source;
            dgv.ClearSelection();
            dgv.CurrentCell = null;
            return dgv;
        }

        private void AddAction(string text, Color color, Point loc, Action onClick)
        {
            var btn = AppTheme.CreateButton(text, color, 145, 40);
            btn.Location = loc;
            btn.Click   += (s, e) => onClick();
            Controls.Add(btn);
        }

        private Panel CreateSectionCard(int x, int y, int w, int h, string title, string navTarget)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // Header strip
            var header = new Panel { Location = new Point(0, 0), Size = new Size(w, 43), BackColor = Color.White };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            header.Controls.Add(new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(14, 13),
                BackColor = Color.Transparent
            });

            var btnAll = new Button
            {
                Text      = "View All →",
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.Primary,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                AutoSize  = true,
                Location  = new Point(w - 82, 13)
            };
            btnAll.FlatAppearance.BorderSize = 0;
            btnAll.Click += (s, e) => _mainForm.NavigateTo(navTarget);
            header.Controls.Add(btnAll);

            card.Controls.Add(header);
            return card;
        }

        private Panel CreateEmptyState(int w, int h, string message)
        {
            var pnl = new Panel { Location = new Point(1, 44), Size = new Size(w, h), BackColor = Color.White };
            pnl.Controls.Add(new Label
            {
                Text      = $"📭\r\n{message}",
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent
            });
            return pnl;
        }

        // ── Professional empty state for Top Selling ──────────────
        private Panel CreateTopSellingEmpty(int w, int h)
        {
            var pnl = new Panel
            {
                Location  = new Point(1, 44),
                Size      = new Size(w, h),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            // Centre everything vertically
            int iconSize = 52;
            int totalContentH = iconSize + 10 + 22 + 6 + 16;
            int startY = (h - totalContentH) / 2;
            int centreX = w / 2;

            // Coloured circle background for icon
            var iconCircle = new Panel
            {
                Size      = new Size(iconSize, iconSize),
                Location  = new Point(centreX - iconSize / 2, startY),
                BackColor = Color.FromArgb(219, 234, 254)  // light blue
            };
            iconCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var br = new SolidBrush(Color.FromArgb(219, 234, 254));
                e.Graphics.FillEllipse(br, 0, 0, iconSize - 1, iconSize - 1);
            };
            iconCircle.Controls.Add(new Label
            {
                Text      = "📊",
                Font      = new Font("Segoe UI Emoji", 18f),
                AutoSize  = false,
                Size      = new Size(iconSize, iconSize),
                Location  = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });
            pnl.Controls.Add(iconCircle);

            // Heading
            pnl.Controls.Add(new Label
            {
                Text      = "No Sales Data Yet",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = false,
                Size      = new Size(w, 22),
                Location  = new Point(0, startY + iconSize + 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });

            // Sub-text
            pnl.Controls.Add(new Label
            {
                Text      = "Complete orders to see your best-selling medicines here.",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize  = false,
                Size      = new Size(w - 40, 16),
                Location  = new Point(20, startY + iconSize + 10 + 22 + 6),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });

            return pnl;
        }

        // ── Professional chip-card list for Expired Medicines ────
        private Panel CreateExpiredList(int w, int h, System.Data.DataTable source)
        {
            // Wrapping chip cards — makes good use of a wide, short card
            // instead of a single narrow column of stacked rows.
            var container = new FlowLayoutPanel
            {
                Location    = new Point(1, 44),
                Size        = new Size(w, h),
                BackColor   = Color.White,
                AutoScroll  = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding     = new Padding(10, 8, 10, 8)
            };

            const int chipW = 220, chipH = 78;
            foreach (System.Data.DataRow row in source.Rows)
            {
                string name  = row["MedicineName"].ToString();
                string code  = row["MedicineCode"] == DBNull.Value ? "" : row["MedicineCode"].ToString();
                string cat   = row["CategoryName"]  == DBNull.Value ? "" : row["CategoryName"].ToString();
                DateTime exp = Convert.ToDateTime(row["ExpiryDate"]);
                int daysAgo  = Math.Max(0, (DateTime.Today - exp.Date).Days);

                var chip = new Panel
                {
                    Size      = new Size(chipW, chipH),
                    Margin    = new Padding(0, 0, 12, 12),
                    BackColor = Color.FromArgb(254, 242, 242)
                };
                chip.Paint += (s, e) =>
                {
                    using var pen = new Pen(Color.FromArgb(252, 165, 165));
                    e.Graphics.DrawRectangle(pen, 0, 0, chip.Width - 1, chip.Height - 1);
                    using var accent = new SolidBrush(AppTheme.Danger);
                    e.Graphics.FillRectangle(accent, 0, 0, 4, chip.Height);
                };

                chip.Controls.Add(new Label
                {
                    Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary,
                    AutoSize = false, AutoEllipsis = true, Location = new Point(14, 8),
                    Size = new Size(chipW - 24, 16), BackColor = Color.Transparent
                });
                chip.Controls.Add(new Label
                {
                    Text = string.IsNullOrEmpty(code) ? cat : $"{code} • {cat}",
                    Font = new Font("Segoe UI", 7.5f), ForeColor = AppTheme.TextMuted,
                    AutoSize = false, AutoEllipsis = true, Location = new Point(14, 26),
                    Size = new Size(chipW - 24, 14), BackColor = Color.Transparent
                });
                chip.Controls.Add(new Label
                {
                    Text = $"Expired {exp:dd MMM yyyy}  ({daysAgo}d)",
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = AppTheme.Danger,
                    AutoSize = false, AutoEllipsis = true, Location = new Point(14, 48),
                    Size = new Size(chipW - 24, 16), BackColor = Color.Transparent
                });

                container.Controls.Add(chip);
            }

            return container;
        }

        // ── Ranked data grid for Top Selling ─────────────────────
        private DataGridView CreateTopSellingGrid(int x, int y, int w, int h,
            System.Data.DataTable source)
        {
            var dgv = new DataGridView
            {
                Location              = new Point(x, y),
                Size                  = new Size(w, h),
                AutoGenerateColumns   = false,
                ColumnHeadersVisible  = true,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                ScrollBars            = ScrollBars.Vertical,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                GridColor             = Color.FromArgb(229, 231, 235),
                Font                  = new Font("Segoe UI", 9f),
                RowTemplate           = { Height = 30 },
                DefaultCellStyle =
                {
                    BackColor        = Color.White,
                    ForeColor        = AppTheme.TextPrimary,
                    SelectionBackColor = Color.White,
                    SelectionForeColor = AppTheme.TextPrimary,
                    Padding          = new Padding(4, 0, 4, 0)
                },
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor          = Color.FromArgb(248, 249, 250),
                    ForeColor          = AppTheme.TextMuted,
                    SelectionBackColor = Color.FromArgb(248, 249, 250),
                    SelectionForeColor = AppTheme.TextMuted,
                    Font               = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Padding            = new Padding(6, 0, 0, 0)
                },
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor          = Color.FromArgb(250, 251, 252),
                    SelectionBackColor = Color.FromArgb(250, 251, 252),
                    SelectionForeColor = AppTheme.TextPrimary
                }
            };
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgv.ColumnHeadersHeight = 32;
            dgv.EnableHeadersVisualStyles = false;

            // Rank column
            var colRank = new DataGridViewTextBoxColumn
            {
                HeaderText = "#",
                Width      = 36,
                ReadOnly   = true,
                SortMode   = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9f, FontStyle.Bold) }
            };
            dgv.Columns.Add(colRank);

            // Medicine name
            var colName = new DataGridViewTextBoxColumn
            {
                HeaderText   = "Medicine Name",
                DataPropertyName = "MedicineName",
                Width        = 250,
                ReadOnly     = true,
                SortMode     = DataGridViewColumnSortMode.NotSortable
            };
            dgv.Columns.Add(colName);

            // Units sold
            var colQty = new DataGridViewTextBoxColumn
            {
                HeaderText   = "Units Sold",
                DataPropertyName = "TotalSold",
                Width        = 95,
                ReadOnly     = true,
                SortMode     = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgv.Columns.Add(colQty);

            // Revenue
            var colRev = new DataGridViewTextBoxColumn
            {
                HeaderText   = "Revenue",
                DataPropertyName = "TotalRevenue",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly     = true,
                SortMode     = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle =
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    ForeColor = AppTheme.Success,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Format    = "\"Rs. \"#,##0.00"
                }
            };
            dgv.Columns.Add(colRev);

            dgv.DataSource = source;

            // Fill rank numbers and colour top-3 rows
            dgv.DataBindingComplete += (s, e) =>
            {
                Color[] rankColors = { Color.FromArgb(255, 215, 0), Color.FromArgb(192, 192, 192), Color.FromArgb(205, 127, 50) };
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    dgv.Rows[i].Cells[0].Value = i + 1;
                    if (i < 3)
                        dgv.Rows[i].Cells[0].Style.ForeColor = rankColors[i];
                }
                dgv.ClearSelection();
                dgv.CurrentCell = null;
            };

            return dgv;
        }
    }
}
