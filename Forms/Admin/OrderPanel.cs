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

namespace SmartMed.Forms.Admin
{
    public class OrderPanel : UserControl
    {
        private readonly OrderRepository _repo    = new OrderRepository();
        private readonly OrderService    _service = new OrderService();

        // Placeholder sentinel — compare by text to avoid color-mismatch bugs
        private const string SearchPh = "Search order # or customer…";

        private DataGridView _dgv;
        private ComboBox     _cmbStatus;
        private TextBox      _txtSearch;
        private Label        _lblCount;
        private List<Order>  _orders = new List<Order>();

        // Status-action buttons
        private Button _btnApprove, _btnReady, _btnDeliver, _btnReject, _btnCancel;
        private Button _btnInvoice, _btnExport;
        private Panel  _pnlActions;

        // Detail panel controls
        private Panel        _pnlDetailEmpty, _pnlDetailContent;
        private Label        _lblDOrderId, _lblDDate, _lblDStatus;
        private Label        _lblDName, _lblDPhone, _lblDAddr;
        private Label        _lblDPayment, _lblDPickup;
        private Label        _lblDTotal, _lblDNotes;
        private DataGridView _dgvItems;

        public OrderPanel()
        {
            BackColor = AppTheme.Background;
            Dock      = DockStyle.Fill;
            Build();
            LoadData();
        }

        // ── Layout ────────────────────────────────────────────────────────────
        //
        //  UserControl (Fill)
        //  ├── topBar       Dock=Top   (full width)
        //  └── pnlMain      Dock=Fill
        //      ├── pnlDetail Dock=Right  300 px
        //      ├── pnlSep    Dock=Right    1 px
        //      └── pnlLeft   Dock=Fill
        //          ├── actionBar  Dock=Bottom
        //          └── _dgv       Dock=Fill
        //
        private void Build()
        {
            BuildTopBar();

            var pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };

            var pnlDetail = new Panel { Dock = DockStyle.Right, Width = 300, BackColor = Color.White };
            var pnlSep    = new Panel { Dock = DockStyle.Right, Width = 1,   BackColor = Color.FromArgb(229, 231, 235) };
            var pnlLeft   = new Panel { Dock = DockStyle.Fill };

            BuildGrid(pnlLeft);
            BuildActionBar(pnlLeft);
            BuildDetail(pnlDetail);

            // Add children: LAST added = highest z-order for WinForms docking (Fill first, Right last)
            pnlMain.Controls.Add(pnlLeft);    // Fill  → added first → docked last ✓
            pnlMain.Controls.Add(pnlSep);     // Right → docked before pnlLeft
            pnlMain.Controls.Add(pnlDetail);  // Right → docked first (highest index) ✓

            Controls.Add(pnlMain);
        }

        // ── Top bar ───────────────────────────────────────────────────────────
        private void BuildTopBar()
        {
            var bar = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Color.White };
            bar.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawLine(p, 0, bar.Height - 1, bar.Width, bar.Height - 1);
            };

            _txtSearch = new TextBox
            {
                Location    = new Point(16, 11),
                Size        = new Size(232, 32),
                BackColor   = Color.FromArgb(248, 250, 252),
                ForeColor   = Color.FromArgb(150, 155, 165),
                Font        = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                Text        = SearchPh
            };
            _txtSearch.GotFocus  += (s, e) =>
            {
                if (_txtSearch.Text == SearchPh)
                { _txtSearch.Text = ""; _txtSearch.ForeColor = AppTheme.TextPrimary; }
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                { _txtSearch.Text = SearchPh; _txtSearch.ForeColor = Color.FromArgb(150, 155, 165); }
            };
            _txtSearch.TextChanged += (s, e) => LoadData();

            _cmbStatus = new ComboBox
            {
                Location      = new Point(260, 11),
                Size          = new Size(150, 32),
                BackColor     = Color.FromArgb(248, 250, 252),
                ForeColor     = AppTheme.TextPrimary,
                Font          = new Font("Segoe UI", 9f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            _cmbStatus.Items.AddRange(new object[]
                { "All Status", "Pending", "Approved", "Ready", "Delivered", "Cancelled", "Rejected" });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (s, e) => LoadData();

            _lblCount = new Label
            {
                AutoSize  = true,
                Location  = new Point(422, 15),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            };

            bar.Controls.AddRange(new Control[] { _txtSearch, _cmbStatus, _lblCount });
            Controls.Add(bar);
        }

        // ── Orders grid ───────────────────────────────────────────────────────
        private void BuildGrid(Panel host)
        {
            _dgv = new DataGridView
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
                ColumnHeadersHeight   = 40,
                RowTemplate           = { Height = 48 }
            };
            _dgv.EnableHeadersVisualStyles = false;

            var hdrStyle = _dgv.ColumnHeadersDefaultCellStyle;
            hdrStyle.BackColor  = Color.FromArgb(248, 250, 252);
            hdrStyle.ForeColor  = AppTheme.TextMuted;
            hdrStyle.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            hdrStyle.Padding    = new Padding(10, 0, 0, 0);
            _dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            var rowStyle = _dgv.DefaultCellStyle;
            rowStyle.BackColor          = Color.White;
            rowStyle.ForeColor          = AppTheme.TextPrimary;
            rowStyle.Font               = new Font("Segoe UI", 9f);
            rowStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            rowStyle.SelectionForeColor = AppTheme.TextPrimary;
            rowStyle.Padding            = new Padding(10, 0, 0, 0);

            _dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            _dgv.CellFormatting   += GridCellFormatting;
            _dgv.SelectionChanged += GridSelectionChanged;

            host.Controls.Add(_dgv);
        }

        // ── Action bar ────────────────────────────────────────────────────────
        private void BuildActionBar(Panel host)
        {
            var bar = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
            bar.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawLine(p, 0, 0, bar.Width, 0);
            };

            // Left: status-aware workflow buttons (at most 2 shown at once, ~208px)
            _pnlActions = new Panel { Location = new Point(12, 8), Size = new Size(220, 36), BackColor = Color.Transparent };

            _btnApprove = WfBtn("Approve",  Color.FromArgb(22, 163, 74));
            _btnReady   = WfBtn("Ready",    Color.FromArgb(8, 145, 178));
            _btnDeliver = WfBtn("Deliver",  AppTheme.Primary);
            _btnReject  = WfBtn("Reject",   Color.FromArgb(220, 38, 38));
            _btnCancel  = WfBtn("Cancel",   Color.FromArgb(180, 83, 9));

            _btnApprove.Click += (s, e) => DoStatus("Approved");
            _btnReady.Click   += (s, e) => DoStatus("Ready");
            _btnDeliver.Click += (s, e) => DoStatus("Delivered");
            _btnReject.Click  += (s, e) => DoStatus("Rejected");
            _btnCancel.Click  += (s, e) => DoStatus("Cancelled");
            _pnlActions.Controls.AddRange(new Control[]
                { _btnApprove, _btnReady, _btnDeliver, _btnReject, _btnCancel });

            // Right: Invoice + Export — a right-docked FlowLayoutPanel keeps these
            // flush to the edge regardless of the bar's width at layout time,
            // instead of computing X positions manually (which could go stale).
            _btnExport = new Button
            {
                Text      = "Export",
                Size      = new Size(78, 36),
                Margin    = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 253, 244),
                ForeColor = Color.FromArgb(22, 163, 74),
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand
            };
            _btnExport.FlatAppearance.BorderColor = Color.FromArgb(187, 247, 208);
            _btnExport.Click += (s, e) =>
                ExcelExporter.Export(BuildExportTable(), "Orders", "Orders_Report");

            _btnInvoice = new Button
            {
                Text      = "Invoice",
                Size      = new Size(82, 36),
                Margin    = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AppTheme.TextMuted,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand
            };
            _btnInvoice.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            _btnInvoice.FlatAppearance.BorderSize  = 1;
            _btnInvoice.Click += Invoice_Click;

            var pnlRight = new FlowLayoutPanel
            {
                Dock          = DockStyle.Right,
                Width         = 8 + _btnInvoice.Width + 8 + _btnExport.Width + 12,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents  = false,
                AutoSize      = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 8, 12, 0)
            };
            pnlRight.Controls.Add(_btnExport);
            pnlRight.Controls.Add(_btnInvoice);

            bar.Controls.AddRange(new Control[] { _pnlActions, pnlRight });

            SetActionButtons(null);
            host.Controls.Add(bar);
        }

        // ── Detail panel ──────────────────────────────────────────────────────
        private void BuildDetail(Panel host)
        {
            // Header strip
            var hdr = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White };
            hdr.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(AppTheme.Primary), 0, 0, 3, hdr.Height);
                using var p = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawLine(p, 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
            };
            hdr.Controls.Add(new Label
            {
                Text      = "  Order Details",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(12, 11),
                BackColor = Color.Transparent
            });
            host.Controls.Add(hdr);

            // Empty state
            _pnlDetailEmpty = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _pnlDetailEmpty.Paint += (s, e) =>
            {
                var g  = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r  = new RectangleF(0, 0, _pnlDetailEmpty.Width, _pnlDetailEmpty.Height);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var big = new Font("Segoe UI", 28f);
                using var sm  = new Font("Segoe UI", 8.5f);
                float cy = _pnlDetailEmpty.Height / 2f;
                g.DrawString("📋", big, new SolidBrush(Color.FromArgb(200, 210, 225)),
                    new RectangleF(0, cy - 50, _pnlDetailEmpty.Width, 50), sf);
                g.DrawString("Select an order to view details", sm,
                    new SolidBrush(Color.FromArgb(150, 160, 175)),
                    new RectangleF(0, cy + 10, _pnlDetailEmpty.Width, 30), sf);
            };
            host.Controls.Add(_pnlDetailEmpty);

            // Scrollable content
            _pnlDetailContent = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = Color.White,
                AutoScroll = true,
                Visible    = false
            };
            _pnlDetailContent.HorizontalScroll.Enabled = false;
            _pnlDetailContent.HorizontalScroll.Visible = false;
            BuildDetailContent(_pnlDetailContent);
            host.Controls.Add(_pnlDetailContent);
        }

        private void BuildDetailContent(Panel p)
        {
            const int lx = 14;
            int y = 14;

            _lblDOrderId = Lbl("", new Font("Segoe UI", 10.5f, FontStyle.Bold), AppTheme.Primary, lx, y); y += 24;
            _lblDDate    = Lbl("", new Font("Segoe UI", 8f), AppTheme.TextMuted, lx, y); y += 30;

            _lblDStatus = new Label
            {
                AutoSize  = true,
                Location  = new Point(lx, y),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding   = new Padding(6, 2, 6, 2)
            };
            p.Controls.Add(_lblDStatus);
            y += 30;

            p.Controls.Add(HLine(lx, y)); y += 18;

            p.Controls.Add(SecHdr("CUSTOMER", lx, y)); y += 20;
            _lblDName  = Lbl("", new Font("Segoe UI", 9.5f, FontStyle.Bold), AppTheme.TextPrimary, lx, y); y += 22;
            _lblDPhone = Lbl("", new Font("Segoe UI", 8.5f), AppTheme.TextMuted, lx, y);                    y += 20;
            _lblDAddr  = Lbl("", new Font("Segoe UI", 8.5f), AppTheme.TextMuted, lx, y);                    y += 26;

            p.Controls.Add(HLine(lx, y)); y += 18;

            p.Controls.Add(SecHdr("ORDER INFO", lx, y)); y += 20;
            _lblDPayment = Lbl("", new Font("Segoe UI", 8.5f), AppTheme.TextPrimary, lx, y); y += 20;
            _lblDPickup  = Lbl("", new Font("Segoe UI", 8.5f), AppTheme.TextPrimary, lx, y); y += 26;

            p.Controls.Add(HLine(lx, y)); y += 18;

            p.Controls.Add(SecHdr("ITEMS", lx, y)); y += 22;

            _dgvItems = new DataGridView
            {
                Location              = new Point(lx, y),
                Size                  = new Size(260, 100),
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                BorderStyle           = BorderStyle.None,
                BackgroundColor       = Color.White,
                GridColor             = Color.FromArgb(232, 236, 241),
                CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars            = ScrollBars.Vertical,
                ColumnHeadersHeight   = 28,
                RowTemplate           = { Height = 26 }
            };
            _dgvItems.EnableHeadersVisualStyles = false;
            _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextMuted;
            _dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            _dgvItems.DefaultCellStyle.Font               = new Font("Segoe UI", 8f);
            _dgvItems.DefaultCellStyle.BackColor          = Color.White;
            _dgvItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            _dgvItems.DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "med",   HeaderText = "Medicine", FillWeight = 55 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "qty",   HeaderText = "Qty",      FillWeight = 15 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "amt",   HeaderText = "Amount",   FillWeight = 30 });
            p.Controls.Add(_dgvItems);
            y += 112;

            p.Controls.Add(HLine(lx, y)); y += 16;

            _lblDTotal = new Label
            {
                AutoSize  = true,
                Location  = new Point(lx, y),
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74),
                BackColor = Color.Transparent
            };
            p.Controls.Add(_lblDTotal);
            y += 28;

            _lblDNotes = Lbl("", new Font("Segoe UI", 8f), AppTheme.TextMuted, lx, y);
            y += 24;

            p.AutoScrollMinSize = new Size(1, y + 10);

            p.Controls.AddRange(new Control[]
                { _lblDOrderId, _lblDDate, _lblDName, _lblDPhone, _lblDAddr,
                  _lblDPayment, _lblDPickup, _lblDTotal, _lblDNotes });

            p.SizeChanged += (s, e) =>
            {
                int w = Math.Max(80, p.Width - lx * 2);
                _dgvItems.Width        = w;
                _lblDNotes.MaximumSize = new Size(w, 0);
                _lblDAddr.MaximumSize  = new Size(w, 0);
            };
        }

        // ── Load data ─────────────────────────────────────────────────────────
        private void LoadData()
        {
            string status = _cmbStatus.SelectedIndex == 0 ? "" : _cmbStatus.SelectedItem.ToString();
            _orders = _repo.GetAll(status);

            // Compare by text — avoids false match when placeholder text has different color
            bool hasSearch = _txtSearch.Text != SearchPh && !string.IsNullOrWhiteSpace(_txtSearch.Text);
            if (hasSearch)
            {
                string q = _txtSearch.Text.Trim().ToLower();
                _orders = _orders.FindAll(o =>
                    o.CustomerName.ToLower().Contains(q) ||
                    o.OrderID.ToString().Contains(q));
            }

            _lblCount.Text = $"{_orders.Count} order{(_orders.Count == 1 ? "" : "s")}";

            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Order #"),
                new DataColumn("Customer"),
                new DataColumn("Date"),
                new DataColumn("Payment"),
                new DataColumn("Total"),
                new DataColumn("Status")
            });

            foreach (var o in _orders)
                dt.Rows.Add(
                    $"#{o.OrderID:D5}",
                    o.CustomerName,
                    o.OrderDate.ToString("dd/MM/yyyy"),
                    o.PaymentMethod,
                    $"Rs. {o.FinalAmount:N2}",
                    o.Status);

            _dgv.DataSource = dt;

            if (_dgv.Columns.Count > 0)
            {
                _dgv.Columns["Order #"].FillWeight  = 10;
                _dgv.Columns["Customer"].FillWeight = 27;
                _dgv.Columns["Date"].FillWeight     = 14;
                _dgv.Columns["Payment"].FillWeight  = 18;
                _dgv.Columns["Total"].FillWeight    = 16;
                _dgv.Columns["Status"].FillWeight   = 13;

                _dgv.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _dgv.Columns["Total"].DefaultCellStyle.Padding   = new Padding(0, 0, 10, 0);
            }

            SetActionButtons(null);
            ShowEmpty();
        }

        // ── Selection ─────────────────────────────────────────────────────────
        private void GridSelectionChanged(object sender, EventArgs e)
        {
            var o = GetSelected();
            if (o == null) { SetActionButtons(null); ShowEmpty(); return; }
            SetActionButtons(o.Status);
            ShowDetail(o);
        }

        private void ShowEmpty()
        {
            _pnlDetailContent.Visible = false;
            _pnlDetailEmpty.Visible   = true;
        }

        private void ShowDetail(Order o)
        {
            var full = _repo.GetByID(o.OrderID);

            _lblDOrderId.Text = $"Order  #{o.OrderID:D5}";
            _lblDDate.Text    = $"Placed  {o.OrderDate:dd MMM yyyy,  HH:mm}";

            _lblDStatus.Text      = $"  {o.Status}  ";
            _lblDStatus.ForeColor = StatusFg(o.Status);
            _lblDStatus.BackColor = StatusBg(o.Status);

            _lblDName.Text    = o.CustomerName;
            _lblDPhone.Text   = string.IsNullOrEmpty(o.CustomerPhone) ? "No phone on file" : $"☎  {o.CustomerPhone}";
            _lblDAddr.Text    = string.IsNullOrEmpty(o.CustomerAddress) ? "No address on file" : $"⌖  {o.CustomerAddress}";

            _lblDPayment.Text = $"Payment:  {o.PaymentMethod}";
            _lblDPickup.Text  = $"Pickup:    {(o.PickupDate.HasValue ? o.PickupDate.Value.ToString("dd MMM yyyy") : "Not set")}";

            _lblDTotal.Text  = $"Total:  Rs. {o.FinalAmount:N2}";
            _lblDNotes.Text  = string.IsNullOrWhiteSpace(o.Notes) ? "" : $"Note: {o.Notes}";

            _dgvItems.Rows.Clear();
            int cnt = 0;
            if (full?.Items != null)
            {
                cnt = full.Items.Count;
                foreach (var item in full.Items)
                    _dgvItems.Rows.Add(item.MedicineName, item.Quantity, $"Rs. {item.Subtotal:N2}");
            }
            _dgvItems.Height = 28 + Math.Max(1, Math.Min(5, cnt)) * 26;

            _pnlDetailEmpty.Visible   = false;
            _pnlDetailContent.Visible = true;
        }

        // ── Status-aware buttons ──────────────────────────────────────────────
        private void SetActionButtons(string status)
        {
            foreach (Control c in _pnlActions.Controls) c.Visible = false;
            if (status == null) return;
            int x = 0;
            void Show(Button b) { b.Location = new Point(x, 0); b.Visible = true; x += b.Width + 8; }
            switch (status)
            {
                case "Pending":  Show(_btnApprove); Show(_btnReject);  break;
                case "Approved": Show(_btnReady);   Show(_btnCancel);  break;
                case "Ready":    Show(_btnDeliver); Show(_btnCancel);  break;
            }
        }

        // ── Cell formatting ───────────────────────────────────────────────────
        private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _orders.Count) return;
            var o   = _orders[e.RowIndex];
            string col = _dgv.Columns[e.ColumnIndex].Name;
            if (col == "Order #")
            {
                e.CellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                e.CellStyle.ForeColor = AppTheme.Primary;
            }
            else if (col == "Status")
            {
                e.CellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                e.CellStyle.ForeColor = StatusFg(o.Status);
            }
            else if (col == "Total")
            {
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
        }

        // ── Status update ─────────────────────────────────────────────────────
        private void DoStatus(string next)
        {
            var sel = GetSelected();
            if (sel == null) return;
            string verb = next switch
            {
                "Approved"  => "Approve",
                "Ready"     => "Mark as Ready",
                "Delivered" => "Mark as Delivered",
                "Rejected"  => "Reject",
                "Cancelled" => "Cancel",
                _           => next
            };
            if (MessageBox.Show($"{verb} order #{sel.OrderID:D5}?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            var (ok, msg) = _service.UpdateStatus(sel.OrderID, next);
            MessageBox.Show(msg, ok ? "Done" : "Error", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            LoadData();
        }

        // ── Invoice ───────────────────────────────────────────────────────────
        private void Invoice_Click(object sender, EventArgs e)
        {
            var sel = GetSelected();
            if (sel == null) { MessageBox.Show("Select an order first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var order = _repo.GetByID(sel.OrderID);
            if (order != null) PDFExporter.ExportInvoice(order);
        }

        // ── Export ────────────────────────────────────────────────────────────
        private DataTable BuildExportTable()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                new DataColumn("Order #"), new DataColumn("Customer"), new DataColumn("Phone"),
                new DataColumn("Address"), new DataColumn("Date"), new DataColumn("Pickup Date"),
                new DataColumn("Payment"), new DataColumn("Total"), new DataColumn("Status")
            });
            foreach (var o in _orders)
                dt.Rows.Add($"#{o.OrderID:D5}", o.CustomerName, o.CustomerPhone, o.CustomerAddress,
                    o.OrderDate.ToString("dd/MM/yyyy"),
                    o.PickupDate?.ToString("dd/MM/yyyy") ?? "—",
                    o.PaymentMethod, $"Rs. {o.FinalAmount:N2}", o.Status);
            return dt;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Order GetSelected()
        {
            if (_dgv.SelectedRows.Count == 0) return null;
            int idx = _dgv.SelectedRows[0].Index;
            return (idx >= 0 && idx < _orders.Count) ? _orders[idx] : null;
        }

        private static Button WfBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text      = text,
                Size      = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Label Lbl(string t, Font f, Color c, int x, int y) =>
            new Label { Text = t, Font = f, ForeColor = c, AutoSize = true,
                        Location = new Point(x, y), BackColor = Color.Transparent };

        private static Label SecHdr(string t, int x, int y) =>
            new Label { Text = t, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(150, 158, 170), AutoSize = true,
                        Location = new Point(x, y), BackColor = Color.Transparent };

        private static Panel HLine(int x, int y) =>
            new Panel { Location = new Point(x, y), Size = new Size(268, 1),
                        BackColor = Color.FromArgb(229, 231, 235) };

        private static Color StatusFg(string s) => s switch
        {
            "Pending"   => Color.FromArgb(146, 64,  14),
            "Approved"  => Color.FromArgb(6,   95,  70),
            "Ready"     => Color.FromArgb(7,   89, 133),
            "Delivered" => Color.FromArgb(22, 101,  52),
            "Cancelled" => Color.FromArgb(153, 27,  27),
            "Rejected"  => Color.FromArgb(153, 27,  27),
            _           => Color.FromArgb(75,  85,  99)
        };

        private static Color StatusBg(string s) => s switch
        {
            "Pending"   => Color.FromArgb(254, 243, 199),
            "Approved"  => Color.FromArgb(209, 250, 229),
            "Ready"     => Color.FromArgb(207, 250, 254),
            "Delivered" => Color.FromArgb(220, 252, 231),
            "Cancelled" => Color.FromArgb(254, 226, 226),
            "Rejected"  => Color.FromArgb(254, 226, 226),
            _           => Color.FromArgb(243, 244, 246)
        };
    }
}
