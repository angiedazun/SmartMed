using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;
using CustomerModel = SmartMed.Models.Customer;

namespace SmartMed.Forms.Admin
{
    public class CustomerPanel : UserControl
    {
        private readonly CustomerRepository _repo = new CustomerRepository();
        private DataGridView dgv;
        private TextBox txtSearch;
        private Label lblTotal;
        private List<CustomerModel> _customers = new List<CustomerModel>();
        private CustomerModel _selected;

        public CustomerPanel()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            txtSearch = new TextBox
            {
                Location = new Point(20, 15),
                Size = new Size(280, 32),
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.FontInput,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadData();
            lblTotal = new Label { Location = new Point(20, 44), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMuted };

            var btnAdd = AppTheme.CreateButton("+ Add Customer", AppTheme.Primary, 140, 36);
            btnAdd.Location = new Point(700, 13);
            btnAdd.Click += (s, e) => OpenForm(null);

            pnlTop.Controls.AddRange(new Control[] { txtSearch, lblTotal, btnAdd });

            dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(dgv);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Index < _customers.Count)
                    _selected = _customers[dgv.SelectedRows[0].Index];
            };
            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _customers.Count) return;
                if (dgv.Columns[e.ColumnIndex].Name == "Status")
                {
                    e.CellStyle.ForeColor = AppTheme.StatusColor(_customers[e.RowIndex].Status);
                    e.CellStyle.Font = AppTheme.FontBold;
                }
            };

            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = AppTheme.Surface };
            var btnEdit    = AppTheme.CreateButton("✏ Edit",    AppTheme.Primary, 110, 36);
            var btnEnable  = AppTheme.CreateButton("✅ Enable",  AppTheme.Success, 110, 36);
            var btnDisable = AppTheme.CreateButton("🚫 Disable", AppTheme.Danger,  110, 36);
            var btnOrders  = AppTheme.CreateButton("📦 Orders",  AppTheme.Accent,  110, 36);
            btnEdit.Location    = new Point(20, 7);
            btnEnable.Location  = new Point(140, 7);
            btnDisable.Location = new Point(260, 7);
            btnOrders.Location  = new Point(380, 7);
            btnEdit.Click    += (s, e) => { if (_selected != null) OpenForm(_selected); };
            btnEnable.Click  += (s, e) => SetStatus("Active");
            btnDisable.Click += (s, e) => SetStatus("Disabled");
            btnOrders.Click  += (s, e) => ViewOrders();
            pnlActions.Controls.AddRange(new Control[] { btnEdit, btnEnable, btnDisable, btnOrders });

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            Controls.Add(pnlActions);
        }

        private void LoadData()
        {
            _customers = _repo.GetAll(txtSearch.Text.Trim());
            lblTotal.Text = $"Total: {_customers.Count} customers";

            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                new DataColumn("ID"), new DataColumn("Name"), new DataColumn("Email"),
                new DataColumn("Phone"), new DataColumn("Address"), new DataColumn("Registered"), new DataColumn("Status")
            });
            foreach (var c in _customers)
                dt.Rows.Add(c.CustomerID, c.FullName, c.Email, c.Phone,
                    c.Address, c.RegisteredDate.ToString("dd/MM/yyyy"), c.Status);

            dgv.DataSource = dt;
        }

        private void OpenForm(CustomerModel c)
        {
            var form = new CustomerForm(c);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void SetStatus(string status)
        {
            if (_selected == null) { MessageBox.Show("Select a customer.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"{status} '{_selected.FullName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _repo.SetStatus(_selected.CustomerID, status);
                LoadData();
            }
        }

        private void ViewOrders()
        {
            if (_selected == null) { MessageBox.Show("Select a customer first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            var orders = new OrderRepository().GetAll(customerId: _selected.CustomerID);

            var form = new Form
            {
                Text            = $"Orders — {_selected.FullName}",
                Size            = new Size(860, 560),
                MinimumSize     = new Size(700, 450),
                BackColor       = AppTheme.Background,
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            // ── Header ────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.White };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            pnlHeader.Controls.Add(new Label
            {
                Text      = $"Orders — {_selected.FullName}",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(22, 12)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = $"{orders.Count} order(s)   |   Customer ID: #{_selected.CustomerID:D4}   |   {_selected.Email}",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(24, 46)
            });

            // ── Summary stat pills ─────────────────────────────────
            var pnlStats = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = AppTheme.Background };

            decimal totalSpent  = 0;
            int     pending     = 0;
            int     delivered   = 0;
            foreach (var o in orders)
            {
                totalSpent += o.FinalAmount;
                if (o.Status == "Pending")   pending++;
                if (o.Status == "Delivered") delivered++;
            }
            AddStatPill(pnlStats, "Total Orders",  orders.Count.ToString(),   AppTheme.Primary,          16);
            AddStatPill(pnlStats, "Total Spent",   $"Rs. {totalSpent:N0}",    AppTheme.Success,          190);
            AddStatPill(pnlStats, "Pending",       pending.ToString(),         AppTheme.Warning,          364);
            AddStatPill(pnlStats, "Delivered",     delivered.ToString(),       Color.FromArgb(6,182,212), 538);

            // ── Grid card ─────────────────────────────────────────
            var pnlCard = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(16, 8, 16, 8) };
            var card    = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            if (orders.Count == 0)
            {
                card.Controls.Add(new Label
                {
                    Text      = "📭\r\nNo orders found for this customer",
                    Font      = new Font("Segoe UI", 11f),
                    ForeColor = AppTheme.TextMuted,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock      = DockStyle.Fill,
                    BackColor = Color.Transparent
                });
            }
            else
            {
                var dgvO = new DataGridView { Dock = DockStyle.Fill };
                AppTheme.StyleDataGrid(dgvO);
                dgvO.RowTemplate.Height = 38;
                dgvO.CellBorderStyle    = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvO.GridColor          = Color.FromArgb(235, 235, 235);

                dgvO.CellFormatting += (s, e) =>
                {
                    if (e.RowIndex < 0) return;
                    if (dgvO.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                    {
                        Color sc = OrderStatusColor(e.Value.ToString());
                        e.CellStyle.ForeColor = sc;
                        e.CellStyle.BackColor = Color.FromArgb(20, sc);
                        e.CellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    }
                };
                dgvO.RowPrePaint += (s, e) =>
                {
                    if (e.RowIndex % 2 == 1)
                        dgvO.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);
                };

                var dt = new DataTable();
                dt.Columns.AddRange(new[]
                {
                    new DataColumn("Order ID"),
                    new DataColumn("Date"),
                    new DataColumn("Amount"),
                    new DataColumn("Payment"),
                    new DataColumn("Pickup Date"),
                    new DataColumn("Status")
                });
                foreach (var o in orders)
                    dt.Rows.Add(
                        $"#{o.OrderID:D5}",
                        o.OrderDate.ToString("dd MMM yyyy  HH:mm"),
                        $"Rs. {o.FinalAmount:F2}",
                        o.PaymentMethod,
                        o.PickupDate?.ToString("dd MMM yyyy") ?? "—",
                        o.Status);
                dgvO.DataSource = dt;

                if (dgvO.Columns.Count == 6)
                {
                    dgvO.Columns[0].Width = 90;
                    dgvO.Columns[1].Width = 165;
                    dgvO.Columns[2].Width = 110;
                    dgvO.Columns[3].Width = 120;
                    dgvO.Columns[4].Width = 120;
                    dgvO.Columns[5].Width = 100;
                }
                card.Controls.Add(dgvO);
            }

            pnlCard.Controls.Add(card);

            // ── Footer ────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.White };
            pnlFooter.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };
            var btnClose = AppTheme.CreateButton("✕  Close", AppTheme.Surface, 110, 34);
            btnClose.ForeColor = AppTheme.TextPrimary;
            btnClose.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnClose.Anchor    = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.Location  = new Point(form.ClientSize.Width - 130, 7);
            btnClose.Click    += (s, e) => form.Close();
            form.SizeChanged  += (s, e) => btnClose.Location = new Point(form.ClientSize.Width - 130, 7);
            pnlFooter.Controls.Add(btnClose);

            form.Controls.Add(pnlCard);
            form.Controls.Add(pnlStats);
            form.Controls.Add(pnlHeader);
            form.Controls.Add(pnlFooter);
            form.ShowDialog();
        }

        private static void AddStatPill(Panel parent, string label, string value, Color color, int x)
        {
            var pill = new Panel
            {
                Location  = new Point(x, 12),
                Size      = new Size(162, 52),
                BackColor = Color.White
            };
            pill.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var border = new Pen(AppTheme.Border);
                g.DrawRectangle(border, 0, 0, pill.Width - 1, pill.Height - 1);
                using var accent = new SolidBrush(color);
                g.FillRectangle(accent, 0, 0, 5, pill.Height);
                using var bg = new SolidBrush(Color.FromArgb(12, color));
                g.FillRectangle(bg, 5, 0, pill.Width - 5, pill.Height);
            };
            pill.Controls.Add(new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = color,
                AutoSize  = true,
                Location  = new Point(14, 6),
                BackColor = Color.Transparent
            });
            pill.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(14, 34),
                BackColor = Color.Transparent
            });
            parent.Controls.Add(pill);
        }

        private static Color OrderStatusColor(string status) => status switch
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


