using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartMed.UI;

namespace SmartMed.Forms.Customer
{
    public class CartPanel : UserControl
    {
        private readonly CustomerMainForm _main;

        private DataGridView _dgv;
        private Label  _lblItems, _lblTotal, _lblItemCount;
        private Panel  _pnlCartCard, _pnlEmpty, _pnlSummary;
        private Button _btnCheckout, _btnClear;

        private const int MARGIN  = 24;
        private const int CARD_W  = 900;
        private const int HDR_H   = 76;
        private const int CART_Y  = MARGIN + HDR_H - 1;
        private const int CART_H  = 420;
        private const int SUM_Y   = CART_Y + CART_H - 1;
        private const int SUM_H   = 80;

        public CartPanel(CustomerMainForm main)
        {
            _main      = main;
            BackColor  = AppTheme.Background;
            Dock       = DockStyle.Fill;
            AutoScroll = true;
            Build();
            LoadCart();
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            AutoScrollMinSize = new Size(1, SUM_Y + SUM_H + 24);
            SizeChanged += (s, e) => AdjustLayout();
        }

        // ── Responsive resizing ───────────────────────────────────────────────
        private void AdjustLayout()
        {
            int w = Math.Max(ClientSize.Width - MARGIN * 2, 700);
            foreach (Control c in Controls)
                if (c?.Tag?.ToString() == "stretch") c.Width = w;

            if (_dgv      != null) _dgv.Width      = _pnlCartCard.Width;
            if (_pnlEmpty != null) _pnlEmpty.Width = _pnlCartCard.Width;

            if (_pnlSummary != null)
            {
                int checkX = _pnlSummary.Width - _btnCheckout.Width - 16;
                _btnCheckout.Location  = new Point(checkX, (_pnlSummary.Height - _btnCheckout.Height) / 2);
                _lblTotal.Location     = new Point(checkX - 200, 16);
                _lblItemCount.Location = new Point(checkX - 200, 44);
            }
        }

        // ── Build ─────────────────────────────────────────────────────────────
        private void Build()
        {
            BuildHeader();
            BuildCartCard();
            BuildSummary();
        }

        // ── 1. Header card ────────────────────────────────────────────────────
        private void BuildHeader()
        {
            var pnl = new Panel
            {
                Location  = new Point(MARGIN, MARGIN),
                Size      = new Size(CARD_W, HDR_H),
                BackColor = Color.White,
                Tag       = "stretch"
            };
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var border = new Pen(AppTheme.Border);
                g.DrawRectangle(border, 0, 0, pnl.Width - 1, pnl.Height - 1);
                // Gradient left accent
                using var acc = new LinearGradientBrush(
                    new Point(0, 0), new Point(0, pnl.Height),
                    AppTheme.Primary, AppTheme.PrimaryHover);
                g.FillRectangle(acc, 0, 0, 4, pnl.Height);
            };

            // Icon box
            var pnlIcon = new Panel { Location = new Point(18, 14), Size = new Size(48, 48), BackColor = Color.Transparent };
            pnlIcon.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundRect(new Rectangle(0, 0, 47, 47), 11);
                using var br   = new LinearGradientBrush(new Point(0,0), new Point(0,47),
                    AppTheme.Primary, AppTheme.PrimaryHover);
                g.FillPath(br, path);
                TextRenderer.DrawText(g, "🛒", new Font("Segoe UI Emoji", 15f),
                    new Rectangle(0, 0, 48, 48), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lblTitle = new Label
            {
                Text      = "Shopping Cart",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(78, 13),
                BackColor = Color.Transparent
            };
            _lblItems = new Label
            {
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(79, 44),
                BackColor = Color.Transparent
            };

            var btnContinue = new Button
            {
                Text      = "← Continue Shopping",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.Primary,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                AutoSize  = true,
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                Location  = new Point(CARD_W - 180, 26),
                FlatAppearance = { BorderSize = 0 }
            };
            btnContinue.Click += (s, e) => _main.NavigateTo("Search Medicine");

            pnl.Controls.AddRange(new Control[] { pnlIcon, lblTitle, _lblItems, btnContinue });
            Controls.Add(pnl);
        }

        // ── 2. Cart items card ────────────────────────────────────────────────
        private void BuildCartCard()
        {
            _pnlCartCard = new Panel
            {
                Location  = new Point(MARGIN, CART_Y),
                Size      = new Size(CARD_W, CART_H),
                BackColor = Color.White,
                Tag       = "stretch"
            };
            _pnlCartCard.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, _pnlCartCard.Width - 1, _pnlCartCard.Height - 1);
            };

            // Card header strip
            var strip = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(CARD_W, 44),
                BackColor = Color.FromArgb(248, 250, 252),
                Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            strip.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, strip.Height - 1, strip.Width, strip.Height - 1);
                using var br = new SolidBrush(AppTheme.Primary);
                e.Graphics.FillRectangle(br, 0, 0, 4, strip.Height);
            };
            strip.Controls.Add(new Label
            {
                Text      = "  🛍  Your Items",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(10, 12),
                BackColor = Color.Transparent
            });
            _pnlCartCard.Controls.Add(strip);

            // DataGridView
            _dgv = new DataGridView
            {
                Location    = new Point(0, 44),
                Size        = new Size(CARD_W, CART_H - 44),
                BorderStyle = BorderStyle.None
            };
            AppTheme.StyleDataGrid(_dgv);
            _dgv.RowTemplate.Height  = 52;
            _dgv.CellBorderStyle    = DataGridViewCellBorderStyle.SingleHorizontal;
            _dgv.GridColor          = Color.FromArgb(241, 245, 249);
            _dgv.ColumnHeadersHeight = 44;
            _dgv.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(248, 250, 252);
            _dgv.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9f, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.Padding    = new Padding(10, 0, 0, 0);
            _dgv.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            _dgv.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;
            _dgv.MultiSelect        = false;
            _dgv.ReadOnly           = true;

            // Columns
            var cNo  = new DataGridViewTextBoxColumn { Name = "no",       HeaderText = "#",          Width = 44  };
            var cMed = new DataGridViewTextBoxColumn { Name = "medicine",  HeaderText = "Medicine",   Width = 200, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var cPrc = new DataGridViewTextBoxColumn { Name = "unitprice", HeaderText = "Unit Price", Width = 120 };

            var cMin = new DataGridViewButtonColumn  { Name = "minus", HeaderText = "", Width = 38,
                Text = "−", UseColumnTextForButtonValue = true };
            cMin.DefaultCellStyle.Alignment          = DataGridViewContentAlignment.MiddleCenter;
            cMin.DefaultCellStyle.BackColor          = Color.FromArgb(241, 245, 249);
            cMin.DefaultCellStyle.ForeColor          = AppTheme.TextPrimary;
            cMin.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
            cMin.DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;
            cMin.DefaultCellStyle.Font               = new Font("Segoe UI", 13f, FontStyle.Bold);
            cMin.DefaultCellStyle.Padding            = new Padding(0);

            var cQty = new DataGridViewTextBoxColumn { Name = "qty", HeaderText = "Qty", Width = 54 };
            cQty.DefaultCellStyle.Alignment          = DataGridViewContentAlignment.MiddleCenter;
            cQty.DefaultCellStyle.Font               = new Font("Segoe UI", 10f, FontStyle.Bold);
            cQty.DefaultCellStyle.ForeColor          = AppTheme.TextPrimary;
            cQty.DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;

            var cPls = new DataGridViewButtonColumn  { Name = "plus", HeaderText = "", Width = 38,
                Text = "+", UseColumnTextForButtonValue = true };
            cPls.DefaultCellStyle.Alignment          = DataGridViewContentAlignment.MiddleCenter;
            cPls.DefaultCellStyle.BackColor          = AppTheme.Primary;
            cPls.DefaultCellStyle.ForeColor          = Color.White;
            cPls.DefaultCellStyle.SelectionBackColor = AppTheme.PrimaryHover;
            cPls.DefaultCellStyle.SelectionForeColor = Color.White;
            cPls.DefaultCellStyle.Font               = new Font("Segoe UI", 13f, FontStyle.Bold);
            cPls.DefaultCellStyle.Padding            = new Padding(0);

            var cSub = new DataGridViewTextBoxColumn { Name = "subtotal", HeaderText = "Subtotal", Width = 130 };
            cSub.DefaultCellStyle.Font               = new Font("Segoe UI", 9f, FontStyle.Bold);
            cSub.DefaultCellStyle.ForeColor          = AppTheme.Primary;
            cSub.DefaultCellStyle.SelectionForeColor = Color.White;

            var cDel = new DataGridViewButtonColumn { Name = "remove", HeaderText = "", Width = 50,
                Text = "✕", UseColumnTextForButtonValue = true };
            cDel.DefaultCellStyle.Alignment          = DataGridViewContentAlignment.MiddleCenter;
            cDel.DefaultCellStyle.BackColor          = Color.FromArgb(254, 242, 242);
            cDel.DefaultCellStyle.ForeColor          = AppTheme.Danger;
            cDel.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 226, 226);
            cDel.DefaultCellStyle.SelectionForeColor = AppTheme.Danger;
            cDel.DefaultCellStyle.Font               = new Font("Segoe UI", 10f, FontStyle.Bold);
            cDel.DefaultCellStyle.Padding            = new Padding(0);

            _dgv.Columns.AddRange(cNo, cMed, cPrc, cMin, cQty, cPls, cSub, cDel);
            _dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgv.Columns["medicine"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            _dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                _dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                    e.RowIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 252, 255);
            };
            _dgv.CellClick += DgvCellClick;

            // Empty state
            _pnlEmpty = new Panel
            {
                Location  = new Point(0, 44),
                Size      = new Size(CARD_W, CART_H - 44),
                BackColor = Color.White
            };
            _pnlEmpty.Paint += DrawEmptyState;

            var btnBrowse = new Button
            {
                Text      = "  Browse Medicines →",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.Primary,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(210, 42)
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click      += (s, e) => _main.NavigateTo("Search Medicine");
            btnBrowse.MouseEnter += (s, e) => btnBrowse.BackColor = AppTheme.PrimaryHover;
            btnBrowse.MouseLeave += (s, e) => btnBrowse.BackColor = AppTheme.Primary;
            _pnlEmpty.Controls.Add(btnBrowse);

            void PinBrowseBtn()
            {
                // blockH: circle 116 + gap 16 + title 28 + gap 8 + subtitle 20 + gap 20 + btn 42 = 250
                int startY = (_pnlEmpty.Height - 250) / 2;
                btnBrowse.Location = new Point((_pnlEmpty.Width - btnBrowse.Width) / 2,
                    startY + 116 + 16 + 28 + 8 + 20 + 20);
            }
            PinBrowseBtn();
            _pnlEmpty.SizeChanged += (s, e) => PinBrowseBtn();

            _pnlCartCard.Controls.Add(_dgv);
            _pnlCartCard.Controls.Add(_pnlEmpty);
            Controls.Add(_pnlCartCard);
        }

        // ── 3. Summary / footer card ──────────────────────────────────────────
        private void BuildSummary()
        {
            _pnlSummary = new Panel
            {
                Location  = new Point(MARGIN, SUM_Y),
                Size      = new Size(CARD_W, SUM_H),
                BackColor = Color.White,
                Tag       = "stretch"
            };
            _pnlSummary.Paint += (s, e) =>
            {
                var g = e.Graphics;
                // Border
                using var border = new Pen(AppTheme.Border);
                g.DrawRectangle(border, 0, 0, _pnlSummary.Width - 1, _pnlSummary.Height - 1);
                // Left accent
                using var acc = new LinearGradientBrush(
                    new Point(0,0), new Point(0, _pnlSummary.Height),
                    AppTheme.Primary, AppTheme.PrimaryHover);
                g.FillRectangle(acc, 0, 0, 4, _pnlSummary.Height);
                // Vertical divider before total
                using var div = new Pen(AppTheme.Border);
                g.DrawLine(div, _pnlSummary.Width - 380, 12, _pnlSummary.Width - 380, SUM_H - 12);
            };

            _btnClear = new Button
            {
                Text      = "🗑  Clear Cart",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppTheme.Danger,
                BackColor = Color.FromArgb(254, 242, 242),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(120, 36),
                Location  = new Point(16, (SUM_H - 36) / 2),
                FlatAppearance = { BorderColor = Color.FromArgb(252, 165, 165), BorderSize = 1 }
            };
            _btnClear.MouseEnter += (s, e) => _btnClear.BackColor = Color.FromArgb(254, 226, 226);
            _btnClear.MouseLeave += (s, e) => _btnClear.BackColor = Color.FromArgb(254, 242, 242);
            _btnClear.Click += (s, e) => { AppState.Cart.Clear(); _main.UpdateCartBadge(); LoadCart(); };

            var btnContinue2 = new Button
            {
                Text      = "← Continue",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                AutoSize  = true,
                Location  = new Point(148, (SUM_H - 26) / 2),
                FlatAppearance = { BorderSize = 0 }
            };
            btnContinue2.Click += (s, e) => _main.NavigateTo("Search Medicine");

            _lblTotal = new Label
            {
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(CARD_W - 380, 14)
            };
            _lblItemCount = new Label
            {
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(CARD_W - 380, 42)
            };

            _btnCheckout = new Button
            {
                Text      = "  ✔  Checkout  →",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.Success,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(170, 50),
                Location  = new Point(CARD_W - 186, (SUM_H - 50) / 2),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatAppearance = { BorderSize = 0 }
            };
            _btnCheckout.MouseEnter += (s, e) => _btnCheckout.BackColor = Color.FromArgb(15, 143, 64);
            _btnCheckout.MouseLeave += (s, e) => _btnCheckout.BackColor = AppTheme.Success;
            _btnCheckout.Click += BtnCheckout_Click;

            _pnlSummary.Controls.AddRange(new Control[]
                { _btnClear, btnContinue2, _lblTotal, _lblItemCount, _btnCheckout });
            Controls.Add(_pnlSummary);
        }

        // ── Load / Refresh ────────────────────────────────────────────────────
        private void LoadCart()
        {
            int count = AppState.Cart.Count;
            _lblItems.Text = count == 0 ? "Your cart is empty"
                                        : $"{count} item{(count != 1 ? "s" : "")} in cart";

            _dgv.Visible   = count > 0;
            _pnlEmpty.Visible = count == 0;

            if (count == 0)
            {
                _lblTotal.Text    = "Total:  Rs. 0.00";
                _lblItemCount.Text = "0 items";
                return;
            }

            _dgv.Rows.Clear();
            int row = 1;
            foreach (var item in AppState.Cart)
                _dgv.Rows.Add(row++, item.MedicineName,
                    $"Rs. {item.Price:F2}", null, item.Quantity, null,
                    $"Rs. {item.Subtotal:F2}", null);

            RefreshSummary();
        }

        private void RefreshSummary()
        {
            int     count = AppState.Cart.Count;
            decimal total = AppState.CartTotal();
            _lblTotal.Text     = $"Total:  Rs. {total:F2}";
            _lblItemCount.Text = $"{count} item{(count != 1 ? "s" : "")}";
            _lblItems.Text     = $"{count} item{(count != 1 ? "s" : "")} in cart";
            _main.UpdateCartBadge();
        }

        // ── Grid cell click ───────────────────────────────────────────────────
        private void DgvCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= AppState.Cart.Count) return;
            string col = _dgv.Columns[e.ColumnIndex].Name;
            var item = AppState.Cart[e.RowIndex];

            if (col == "remove")
            {
                AppState.Cart.RemoveAt(e.RowIndex);
                LoadCart();
            }
            else if (col == "minus")
            {
                if (item.Quantity <= 1) return;
                item.Quantity--;
                item.Subtotal = item.Quantity * item.Price;
                _dgv.Rows[e.RowIndex].Cells["qty"].Value      = item.Quantity;
                _dgv.Rows[e.RowIndex].Cells["subtotal"].Value = $"Rs. {item.Subtotal:F2}";
                RefreshSummary();
            }
            else if (col == "plus")
            {
                item.Quantity++;
                item.Subtotal = item.Quantity * item.Price;
                _dgv.Rows[e.RowIndex].Cells["qty"].Value      = item.Quantity;
                _dgv.Rows[e.RowIndex].Cells["subtotal"].Value = $"Rs. {item.Subtotal:F2}";
                RefreshSummary();
            }
        }

        // ── Checkout ──────────────────────────────────────────────────────────
        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (AppState.Cart.Count == 0)
            { MessageBox.Show("Cart is empty.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var form = new CheckoutForm();
            if (form.ShowDialog() == DialogResult.OK)
            { LoadCart(); _main.NavigateTo("My Orders"); }
        }

        // ── Empty state painter ───────────────────────────────────────────────
        private void DrawEmptyState(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // blockH = circle(116) + gap(16) + title(28) + gap(8) + subtitle(20) + gap(20) + btn(42) = 250
            int blockH = 250;
            int startY = (_pnlEmpty.Height - blockH) / 2;
            int cx     = _pnlEmpty.Width / 2;

            // Light blue circle
            using var circleBr = new SolidBrush(Color.FromArgb(235, 245, 255));
            g.FillEllipse(circleBr, cx - 58, startY, 116, 116);
            using var ringPen = new Pen(Color.FromArgb(80, 37, 99, 235), 1.5f);
            g.DrawEllipse(ringPen, cx - 58, startY, 116, 116);

            using var fIcon = new Font("Segoe UI Emoji", 28f);
            var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("🛒", fIcon, new SolidBrush(Color.FromArgb(100, 120, 180)),
                new RectangleF(cx - 58, startY, 116, 116), sfC);

            int titleY = startY + 116 + 16;
            using var fH  = new Font("Segoe UI", 13f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("Your cart is empty", fH, new SolidBrush(AppTheme.TextPrimary),
                new RectangleF(0, titleY, _pnlEmpty.Width, 28), sf);

            int subY = titleY + 28 + 8;
            using var fS = new Font("Segoe UI", 10f);
            g.DrawString("Browse medicines to find what you need.", fS, new SolidBrush(AppTheme.TextMuted),
                new RectangleF(0, subY, _pnlEmpty.Width, 20), sf);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X,                  r.Y,                   radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y,                   radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2,   0, 90);
            path.AddArc(r.X,                  r.Bottom - radius * 2, radius * 2, radius * 2,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
