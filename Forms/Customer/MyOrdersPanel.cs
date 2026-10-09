using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartMed.Models;
using SmartMed.Repository;
using SmartMed.Services;
using SmartMed.UI;
using SmartMed.Utilities;

namespace SmartMed.Forms.Customer
{
    public class MyOrdersPanel : UserControl
    {
        private readonly OrderRepository _orderRepo = new OrderRepository();
        private readonly OrderService    _orderSvc  = new OrderService();
        private DataGridView dgv;
        private ComboBox     cmbStatus;
        private Label        lblCount;
        private Button       btnCancel;
        private List<Order>  _orders = new List<Order>();
        private Panel        pnlEmptyState;

        public MyOrdersPanel()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadOrders();
        }

        private void Build()
        {
            // ── Header bar ────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 70,
                BackColor = Color.White
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            var lblTitle = new Label
            {
                Text      = "My Orders",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(24, 18)
            };

            lblCount = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.White,
                BackColor = AppTheme.Primary,
                AutoSize  = false,
                Size      = new Size(36, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Location  = new Point(148, 24)
            };

            var lblFilter = new Label
            {
                Text      = "Filter:",
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(560, 26)
            };

            cmbStatus = new ComboBox
            {
                Location      = new Point(610, 22),
                Size          = new Size(160, 30),
                BackColor     = AppTheme.Surface,
                ForeColor     = AppTheme.TextPrimary,
                Font          = AppTheme.FontLabel,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            cmbStatus.Items.AddRange(new object[] { "All Orders", "Pending", "Approved", "Ready", "Delivered", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => LoadOrders();

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblCount, lblFilter, cmbStatus });

            // ── Grid card ─────────────────────────────────────────
            var pnlCard = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = AppTheme.Background,
                Padding   = new Padding(20, 14, 20, 0)
            };

            var cardWrap = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White
            };
            cardWrap.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, cardWrap.Width - 1, cardWrap.Height - 1);
            };

            dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(dgv);
            dgv.CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor         = Color.FromArgb(235, 235, 235);
            dgv.RowTemplate.Height = 40;
            dgv.SelectionChanged += (s, e) =>
            {
                var sel = GetSelected();
                if (sel == null || btnCancel == null) return;
                // Only Pending orders can be cancelled by customers
                bool canCancel = sel.Status == "Pending";
                btnCancel.Enabled   = canCancel;
                btnCancel.BackColor = canCancel ? AppTheme.Danger : Color.FromArgb(200, 200, 200);
                btnCancel.ForeColor = Color.White;
            };
            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _orders.Count) return;
                if (dgv.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    string st = e.Value.ToString();
                    e.CellStyle.ForeColor  = StatusColor(st);
                    e.CellStyle.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    e.CellStyle.BackColor  = Color.FromArgb(20, StatusColor(st));
                }
            };
            dgv.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex % 2 == 1)
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);
            };

            // Empty state
            pnlEmptyState = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Visible = false };
            pnlEmptyState.Controls.Add(new Label
            {
                Text      = "📦\r\nNo orders found",
                Font      = new Font("Segoe UI", 12f),
                ForeColor = AppTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent
            });

            cardWrap.Controls.Add(dgv);
            cardWrap.Controls.Add(pnlEmptyState);
            pnlCard.Controls.Add(cardWrap);

            // ── Action bar ────────────────────────────────────────
            var pnlActions = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 58,
                BackColor = Color.White
            };
            pnlActions.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, 0, pnlActions.Width, 0);
            };

            var btnView    = MakeBtn("👁  View Details",      AppTheme.Primary, 150, 38,  20);
            var btnInvoice = MakeBtn("🧾  Download Invoice",  AppTheme.Accent,  170, 38, 180);
            btnCancel      = MakeBtn("✖  Cancel Order",       AppTheme.Danger,  140, 38, 360);
            btnCancel.Enabled = false;   // disabled until a Pending order is selected

            btnView.Click    += (s, e) => ViewDetails();
            btnInvoice.Click += (s, e) =>
            {
                var sel = GetSelected();
                if (sel == null) { ShowHint(); return; }
                var order = _orderRepo.GetByID(sel.OrderID);
                if (order != null) PDFExporter.ExportInvoice(order);
            };
            btnCancel.Click += (s, e) => CancelOrder();

            // Status legend
            AddLegend(pnlActions, "Pending",   AppTheme.Warning, 530);
            AddLegend(pnlActions, "Approved",  AppTheme.Primary, 620);
            AddLegend(pnlActions, "Delivered", AppTheme.Success, 710);
            AddLegend(pnlActions, "Cancelled", AppTheme.Danger,  810);

            pnlActions.Controls.AddRange(new Control[] { btnView, btnInvoice, btnCancel });

            Controls.Add(pnlCard);
            Controls.Add(pnlHeader);
            Controls.Add(pnlActions);
        }

        private Button MakeBtn(string text, Color color, int w, int h, int x)
        {
            var btn = AppTheme.CreateButton(text, color, w, h);
            btn.Location = new Point(x, (58 - h) / 2);
            return btn;
        }

        private void AddLegend(Panel parent, string label, Color color, int x)
        {
            parent.Controls.Add(new Panel
            {
                Location  = new Point(x, 22),
                Size      = new Size(10, 10),
                BackColor = color
            });
            parent.Controls.Add(new Label
            {
                Text      = label,
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(x + 14, 20)
            });
        }

        private void LoadOrders()
        {
            if (AppState.CurrentCustomer == null) return;
            string filter = cmbStatus.SelectedIndex == 0 ? "" : cmbStatus.SelectedItem.ToString();
            _orders = _orderRepo.GetAll(filter, AppState.CurrentCustomer.CustomerID);

            lblCount.Text = _orders.Count.ToString();

            bool empty = _orders.Count == 0;
            pnlEmptyState.Visible = empty;
            dgv.Visible           = !empty;

            if (empty) return;

            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Order ID"),
                new DataColumn("Date"),
                new DataColumn("Items"),
                new DataColumn("Amount"),
                new DataColumn("Payment"),
                new DataColumn("Pickup Date"),
                new DataColumn("Status")
            });
            foreach (var o in _orders)
                dt.Rows.Add(
                    $"#{o.OrderID:D5}",
                    o.OrderDate.ToString("dd MMM yyyy  HH:mm"),
                    o.ItemCount.ToString(),
                    $"Rs. {o.FinalAmount:F2}",
                    o.PaymentMethod,
                    o.PickupDate?.ToString("dd MMM yyyy") ?? "—",
                    o.Status);
            dgv.DataSource = dt;

            // Column widths
            if (dgv.Columns.Count == 7)
            {
                dgv.Columns[0].Width = 90;
                dgv.Columns[1].Width = 155;
                dgv.Columns[2].Width = 60;
                dgv.Columns[3].Width = 110;
                dgv.Columns[4].Width = 110;
                dgv.Columns[5].Width = 120;
                dgv.Columns[6].Width = 100;
            }
        }

        private void ViewDetails()
        {
            var sel = GetSelected();
            if (sel == null) { ShowHint(); return; }
            var order = _orderRepo.GetByID(sel.OrderID);
            if (order == null) return;

            Color statusFg = StatusColor(order.Status);
            Color statusBg = StatusBgColor(order.Status);

            var form = new Form
            {
                Text            = $"Order #{order.OrderID:D5}",
                ClientSize      = new Size(700, 600),   // client-coords, no title-bar offset
                BackColor       = Color.FromArgb(245, 247, 250),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MaximizeBox     = false,
                MinimizeBox     = false
            };

            // ── Gradient header ───────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 90 };
            hdr.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    hdr.ClientRectangle,
                    Color.FromArgb(37, 99, 235), Color.FromArgb(29, 78, 216),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                g.FillRectangle(br, hdr.ClientRectangle);
            };

            // Order # (white, large)
            hdr.Controls.Add(new Label
            {
                Text      = $"Order  #{order.OrderID:D5}",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(24, 16),
                BackColor = Color.Transparent
            });

            // Status badge
            var badge = new Label
            {
                Text      = $"  {order.Status}  ",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = statusFg,
                BackColor = statusBg,
                AutoSize  = true,
                Location  = new Point(24, 55),
                Padding   = new Padding(6, 3, 6, 3)
            };
            hdr.Controls.Add(badge);

            // Date
            hdr.Controls.Add(new Label
            {
                Text      = order.OrderDate.ToString("dddd, dd MMM yyyy  •  HH:mm"),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(190, 210, 255),
                AutoSize  = true,
                Location  = new Point(badge.Right + 16, 58),
                BackColor = Color.Transparent
            });

            form.Controls.Add(hdr);

            // ── Info cards row ────────────────────────────────────────────────
            const int INFO_H = 110, PAD = 14;

            var pnlInfoRow = new Panel
            {
                Location  = new Point(PAD, hdr.Height + PAD),
                Size      = new Size(700 - PAD * 2, INFO_H),
                BackColor = Color.Transparent
            };

            // Customer card
            var cardLeft = InfoCard(0, 0, (700 - PAD * 2) / 2 - 6, INFO_H);
            AddCardTitle(cardLeft, "CUSTOMER", 12, 12);
            AddCardRow(cardLeft, "★", order.CustomerName,                                              12, 32, bold: true);
            AddCardRow(cardLeft, "☎", string.IsNullOrEmpty(order.CustomerPhone) ? "—" : order.CustomerPhone, 12, 54);
            AddCardRow(cardLeft, "⌖", string.IsNullOrEmpty(order.CustomerAddress) ? "—" : order.CustomerAddress, 12, 72);

            // Order info card
            var cardRight = InfoCard((700 - PAD * 2) / 2 + 6, 0, (700 - PAD * 2) / 2 - 6, INFO_H);
            AddCardTitle(cardRight, "ORDER INFO", 12, 12);
            AddCardRow(cardRight, "💳", order.PaymentMethod,                                                            12, 32);
            AddCardRow(cardRight, "📅", order.PickupDate.HasValue ? order.PickupDate.Value.ToString("dd MMM yyyy") : "Not set", 12, 54);
            AddCardRow(cardRight, "📝", string.IsNullOrEmpty(order.Notes) ? "No notes" : order.Notes,                  12, 72);

            pnlInfoRow.Controls.Add(cardLeft);
            pnlInfoRow.Controls.Add(cardRight);
            form.Controls.Add(pnlInfoRow);

            // ── Items section label ───────────────────────────────────────────
            int itemsY = hdr.Height + PAD + INFO_H + PAD;
            var lblItems = new Label
            {
                Text      = "ORDER ITEMS",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 158, 170),
                AutoSize  = true,
                Location  = new Point(PAD, itemsY),
                BackColor = Color.Transparent
            };
            form.Controls.Add(lblItems);

            // ── Items grid ────────────────────────────────────────────────────
            int gridY = itemsY + 22;
            int gridH = 600 - gridY - 66;  // leave room for footer

            var gridWrap = new Panel
            {
                Location    = new Point(PAD, gridY),
                Size        = new Size(700 - PAD * 2, gridH),
                BackColor   = Color.White,
                BorderStyle = BorderStyle.None
            };
            gridWrap.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawRectangle(p, 0, 0, gridWrap.Width - 1, gridWrap.Height - 1);
            };

            var dgvItems = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible     = false,
                MultiSelect           = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                GridColor             = Color.FromArgb(241, 245, 249),
                CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
                ScrollBars            = ScrollBars.Vertical,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight   = 38,
                RowTemplate           = { Height = 40 }
            };
            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(248, 250, 252);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(100, 110, 125);
            dgvItems.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgvItems.ColumnHeadersDefaultCellStyle.Padding    = new Padding(10, 0, 0, 0);
            dgvItems.ColumnHeadersBorderStyle                  = DataGridViewHeaderBorderStyle.Single;
            dgvItems.DefaultCellStyle.BackColor                = Color.White;
            dgvItems.DefaultCellStyle.ForeColor                = AppTheme.TextPrimary;
            dgvItems.DefaultCellStyle.Font                     = new Font("Segoe UI", 9f);
            dgvItems.DefaultCellStyle.SelectionBackColor       = Color.FromArgb(219, 234, 254);
            dgvItems.DefaultCellStyle.SelectionForeColor       = AppTheme.TextPrimary;
            dgvItems.DefaultCellStyle.Padding                  = new Padding(10, 0, 0, 0);
            dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "med",   HeaderText = "Medicine",   FillWeight = 36 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "code",  HeaderText = "Code",       FillWeight = 17 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "qty",   HeaderText = "Qty",        FillWeight = 10 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "price", HeaderText = "Unit Price", FillWeight = 18 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "sub",   HeaderText = "Subtotal",   FillWeight = 19 });

            dgvItems.Columns["qty"].DefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            dgvItems.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["price"].DefaultCellStyle.Padding   = new Padding(0, 0, 10, 0);
            dgvItems.Columns["sub"].DefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["sub"].DefaultCellStyle.Padding     = new Padding(0, 0, 10, 0);
            dgvItems.Columns["sub"].DefaultCellStyle.Font        = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvItems.Columns["sub"].DefaultCellStyle.ForeColor   = Color.FromArgb(37, 99, 235);

            // Cell formatting for first column
            dgvItems.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (dgvItems.Columns[e.ColumnIndex].Name == "med")
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            };

            foreach (var item in order.Items)
                dgvItems.Rows.Add(item.MedicineName, item.MedicineCode, item.Quantity,
                    $"Rs. {item.Price:F2}", $"Rs. {item.Subtotal:F2}");

            gridWrap.Controls.Add(dgvItems);
            form.Controls.Add(gridWrap);

            // ── Footer ────────────────────────────────────────────────────────
            var footer = new Panel
            {
                Location  = new Point(0, 600 - 62),
                Size      = new Size(700, 62),
                BackColor = Color.White
            };
            footer.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawLine(p, 0, 0, footer.Width, 0);
            };

            footer.Controls.Add(new Label
            {
                Text      = $"Total Payable",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(120, 128, 140),
                AutoSize  = true,
                Location  = new Point(20, 10),
                BackColor = Color.Transparent
            });
            footer.Controls.Add(new Label
            {
                Text      = $"Rs. {order.FinalAmount:N2}",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74),
                AutoSize  = true,
                Location  = new Point(20, 28),
                BackColor = Color.Transparent
            });

            var btnClose = new Button
            {
                Text      = "Close",
                Size      = new Size(100, 38),
                Location  = new Point(700 - 120, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnClose.FlatAppearance.BorderSize = 0;
            footer.Controls.Add(btnClose);
            form.Controls.Add(footer);

            form.CancelButton = btnClose;
            form.ShowDialog();
        }

        // ── Detail form helpers ────────────────────────────────────────────────
        private static Panel InfoCard(int x, int y, int w, int h)
        {
            var p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = Color.White };
            p.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                using var br = new SolidBrush(Color.FromArgb(37, 99, 235));
                e.Graphics.FillRectangle(br, 0, 0, 3, p.Height);
            };
            return p;
        }

        private static void AddCardTitle(Panel card, string text, int x, int y)
        {
            card.Controls.Add(new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 158, 170),
                AutoSize  = true,
                Location  = new Point(x, y),
                BackColor = Color.Transparent
            });
        }

        private static void AddCardRow(Panel card, string icon, string text, int x, int y, bool bold = false)
        {
            card.Controls.Add(new Label
            {
                Text      = icon,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(37, 99, 235),
                AutoSize  = true,
                Location  = new Point(x, y),
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text        = text,
                Font        = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor   = bold ? AppTheme.TextPrimary : AppTheme.TextMuted,
                AutoSize    = true,
                Location    = new Point(x + 22, y),
                BackColor   = Color.Transparent,
                MaximumSize = new Size(card.Width - x - 30, 0)
            });
        }

        private void CancelOrder()
        {
            var sel = GetSelected();
            if (sel == null) { ShowHint(); return; }

            if (sel.Status == "Delivered")
            {
                MessageBox.Show("This order has already been delivered and cannot be cancelled.",
                    "Cannot Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (sel.Status != "Pending")
            {
                MessageBox.Show($"Orders with status '{sel.Status}' cannot be cancelled.\nOnly Pending orders can be cancelled.",
                    "Cannot Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Cancel order #{sel.OrderID:D5}?\nThis will restore the medicine stock.",
                    "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var (ok, msg) = _orderSvc.UpdateStatus(sel.OrderID, "Cancelled");
                MessageBox.Show(ok ? "Order cancelled. Stock has been restored." : msg,
                    ok ? "Cancelled" : "Error", MessageBoxButtons.OK,
                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                LoadOrders();
            }
        }

        private Order GetSelected()
        {
            if (dgv.SelectedRows.Count == 0) return null;
            int idx = dgv.SelectedRows[0].Index;
            return (idx >= 0 && idx < _orders.Count) ? _orders[idx] : null;
        }

        private static void ShowHint() =>
            MessageBox.Show("Please select an order first.", "No Selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private static Color StatusBgColor(string status) => status switch
        {
            "Pending"   => Color.FromArgb(254, 243, 199),
            "Approved"  => Color.FromArgb(219, 234, 254),
            "Ready"     => Color.FromArgb(207, 250, 254),
            "Delivered" => Color.FromArgb(220, 252, 231),
            "Cancelled" => Color.FromArgb(254, 226, 226),
            "Rejected"  => Color.FromArgb(254, 226, 226),
            _           => Color.FromArgb(243, 244, 246)
        };
    }
}
