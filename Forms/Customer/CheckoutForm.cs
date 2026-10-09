using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartMed.Models;
using SmartMed.Services;
using SmartMed.UI;

namespace SmartMed.Forms.Customer
{
    public class CheckoutForm : Form
    {
        private readonly OrderService _orderService = new OrderService();

        private ComboBox       cmbPayment;
        private DateTimePicker dtpPickup;
        private TextBox        txtNotes;
        private PictureBox     pbPresc;
        private Label          lblUploadHint;
        private string         _prescPath;

        private const int FORM_W  = 560;
        private const int HDR_H   = 72;
        private const int FTR_H   = 60;
        private const int PAD     = 20;
        private const int ROW_H   = 28;
        private const int FIELD_H = 34;
        private const int SEC_GAP = 16;

        public CheckoutForm()
        {
            // Calculate form height to fit all content without scrolling
            int n         = AppState.Cart.Count;
            int summaryH  = 24 + n * ROW_H + 44;           // title + items + divider+total
            int paymentH  = 24 + 18 + FIELD_H;             // title + label + field (side by side)
            int extraH    = 24 + 18 + 68 + 10 + 18 + 52;   // title + presc label+zone + notes label+box
            int bodyH        = PAD + summaryH + SEC_GAP + 1 + SEC_GAP
                              + paymentH + SEC_GAP + 1 + SEC_GAP
                              + extraH + PAD;
            int naturalFormH = HDR_H + bodyH + FTR_H;

            // Cap the window at the screen size; if content is taller, the body scrolls
            // instead of being clipped off (Notes field / footer were disappearing on
            // smaller screens before this).
            int formH = Math.Min(naturalFormH, Screen.PrimaryScreen.WorkingArea.Height - 60);

            Text            = "Place Order";
            ClientSize      = new Size(FORM_W, formH);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;

            BuildHeader();
            BuildBody(formH - HDR_H - FTR_H, bodyH);
            BuildFooter(formH - FTR_H);
        }

        // ── Header ────────────────────────────────────────────────────────────
        private void BuildHeader()
        {
            var pnl = new Panel { Location = new Point(0, 0), Size = new Size(FORM_W, HDR_H) };
            pnl.Paint += (s, e) =>
            {
                using var br = new LinearGradientBrush(pnl.ClientRectangle,
                    AppTheme.Primary, AppTheme.PrimaryHover, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(br, pnl.ClientRectangle);
            };

            var ico = new Panel { Location = new Point(16, 12), Size = new Size(48, 48), BackColor = Color.Transparent };
            ico.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundRect(new Rectangle(0, 0, 47, 47), 10);
                using var br   = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
                g.FillPath(br, path);
                TextRenderer.DrawText(g, "🛒", new Font("Segoe UI Emoji", 16f),
                    new Rectangle(0, 0, 48, 48), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            pnl.Controls.Add(ico);
            pnl.Controls.Add(MkLbl("Place Your Order",
                new Font("Segoe UI", 14f, FontStyle.Bold), Color.White, new Point(74, 12)));
            pnl.Controls.Add(MkLbl("Review your details before confirming",
                new Font("Segoe UI", 8.5f), Color.FromArgb(180, 255, 255, 255), new Point(75, 42)));

            var badge = new Label
            {
                Text      = $"{AppState.Cart.Count} item{(AppState.Cart.Count != 1 ? "s" : "")}  ·  Rs. {AppState.CartTotal():F2}",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 255, 255, 255),
                AutoSize  = true,
                Padding   = new Padding(10, 5, 10, 5),
                Location  = new Point(FORM_W - 190, 22)
            };
            pnl.Controls.Add(badge);
            Controls.Add(pnl);
        }

        // ── Body ──────────────────────────────────────────────────────────────
        private void BuildBody(int viewportH, int contentH)
        {
            var body = new Panel
            {
                Location   = new Point(0, HDR_H),
                Size       = new Size(FORM_W, viewportH),
                BackColor  = Color.White,
                AutoScroll = true
            };
            Controls.Add(body);

            int y  = PAD;
            int iw = FORM_W - PAD * 2;

            // ── Order Summary ─────────────────────────────────────────────────
            body.Controls.Add(SecTitle("🧾  Order Summary", iw, y)); y += 24;

            foreach (var item in AppState.Cart)
            {
                body.Controls.Add(MkLbl(
                    $"{item.MedicineName}  ×  {item.Quantity}",
                    AppTheme.FontLabel, AppTheme.TextPrimary, new Point(PAD, y)));

                var s = MkLbl($"Rs. {item.Subtotal:F2}", AppTheme.FontBold, AppTheme.Primary, Point.Empty);
                s.Location = new Point(PAD + iw - s.PreferredWidth, y);
                body.Controls.Add(s);
                y += ROW_H;
            }

            // Divider
            body.Controls.Add(new Panel { Location = new Point(PAD, y + 4), Size = new Size(iw, 1), BackColor = AppTheme.Border });
            y += 12;

            // Total row
            body.Controls.Add(MkLbl("Order Total", AppTheme.FontBold, AppTheme.TextMuted, new Point(PAD, y + 3)));
            var lblTot = new Label
            {
                Text      = $"Rs. {AppState.CartTotal():F2}",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.Success,
                AutoSize  = true,
                BackColor = Color.Transparent
            };
            lblTot.Location = new Point(PAD + iw - lblTot.PreferredWidth, y);
            body.Controls.Add(lblTot);
            y += 32;

            // Section separator
            y += SEC_GAP;
            body.Controls.Add(HRule(iw, y)); y += 1 + SEC_GAP;

            // ── Payment Details (side-by-side) ────────────────────────────────
            body.Controls.Add(SecTitle("💳  Payment Details", iw, y)); y += 24;

            int half = (iw - 14) / 2;

            body.Controls.Add(MkLbl("Payment Method", AppTheme.FontBold, AppTheme.TextMuted, new Point(PAD, y)));
            body.Controls.Add(MkLbl("Pickup Date",    AppTheme.FontBold, AppTheme.TextMuted, new Point(PAD + half + 14, y)));
            y += 18;

            cmbPayment = new ComboBox
            {
                Location      = new Point(PAD, y),
                Size          = new Size(half, FIELD_H),
                BackColor     = AppTheme.Background,
                ForeColor     = AppTheme.TextPrimary,
                Font          = AppTheme.FontLabel,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            cmbPayment.Items.AddRange(new object[] { "Cash on Pickup", "Card Payment", "Online Transfer" });
            cmbPayment.SelectedIndex = 0;
            body.Controls.Add(cmbPayment);

            dtpPickup = new DateTimePicker
            {
                Location = new Point(PAD + half + 14, y),
                Width    = half,
                Format   = DateTimePickerFormat.Short,
                MinDate  = DateTime.Today,
                Font     = AppTheme.FontLabel
            };
            body.Controls.Add(dtpPickup);
            y += FIELD_H;

            // Section separator
            y += SEC_GAP;
            body.Controls.Add(HRule(iw, y)); y += 1 + SEC_GAP;

            // ── Additional Info ───────────────────────────────────────────────
            body.Controls.Add(SecTitle("📋  Additional Info", iw, y)); y += 24;

            // Prescription
            body.Controls.Add(MkLbl("Prescription  (required for Rx medicines)",
                AppTheme.FontBold, AppTheme.TextMuted, new Point(PAD, y))); y += 18;

            var pnlUp = new Panel
            {
                Location  = new Point(PAD, y),
                Size      = new Size(iw, 68),
                BackColor = Color.FromArgb(239, 246, 255),
                Cursor    = Cursors.Hand
            };
            pnlUp.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(147, 197, 253), 1.5f) { DashStyle = DashStyle.Dash };
                e.Graphics.DrawRectangle(pen, 1, 1, pnlUp.Width - 3, pnlUp.Height - 3);
            };

            pbPresc = new PictureBox
            {
                Location = new Point(8, 6), Size = new Size(56, 56),
                BackColor = Color.Transparent, SizeMode = PictureBoxSizeMode.Zoom
            };
            lblUploadHint = new Label
            {
                Text      = "📎   Click here to upload prescription  (.jpg / .png)",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(96, 165, 250),
                AutoSize  = true,
                Location  = new Point(72, 22),
                BackColor = Color.Transparent
            };
            var btnUp = new Button
            {
                Size      = pnlUp.Size, Location = Point.Empty,
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnUp.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _prescPath         = ofd.FileName;
                    pbPresc.Image      = Image.FromFile(ofd.FileName);
                    lblUploadHint.Text = "✔  Prescription uploaded";
                    lblUploadHint.ForeColor = AppTheme.Success;
                    pnlUp.BackColor    = Color.FromArgb(220, 252, 231);
                    pnlUp.Invalidate();
                }
            };
            pnlUp.Controls.AddRange(new Control[] { pbPresc, lblUploadHint, btnUp });
            btnUp.BringToFront();
            body.Controls.Add(pnlUp);
            y += 68 + 10;

            // Notes
            body.Controls.Add(MkLbl("Notes  (optional)", AppTheme.FontBold, AppTheme.TextMuted, new Point(PAD, y)));
            y += 18;

            var notesPnl = new Panel
            {
                Location  = new Point(PAD, y),
                Size      = new Size(iw, 52),
                BackColor = AppTheme.Background
            };
            notesPnl.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, notesPnl.Width - 1, notesPnl.Height - 1);
            };
            txtNotes = new TextBox
            {
                Location    = new Point(4, 4),
                Size        = new Size(iw - 8, 44),
                BackColor   = AppTheme.Background,
                ForeColor   = AppTheme.TextPrimary,
                Font        = AppTheme.FontLabel,
                BorderStyle = BorderStyle.None,
                Multiline   = true
            };
            notesPnl.Controls.Add(txtNotes);
            body.Controls.Add(notesPnl);

            body.AutoScrollMinSize = new Size(0, contentH);
        }

        // ── Footer ────────────────────────────────────────────────────────────
        private void BuildFooter(int topY)
        {
            var ftr = new Panel { Location = new Point(0, topY), Size = new Size(FORM_W, FTR_H), BackColor = Color.White };
            ftr.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, 0, ftr.Width, 0);
                // Subtle top gradient tint
                using var br = new LinearGradientBrush(ftr.ClientRectangle,
                    Color.FromArgb(8, 0, 0, 0), Color.Transparent, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(br, 0, 0, ftr.Width, 4);
            };

            var btnCancel = new Button
            {
                Text      = "Cancel",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.FromArgb(243, 244, 246),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(100, 38),
                Location  = new Point(PAD, (FTR_H - 38) / 2),
                FlatAppearance = { BorderColor = AppTheme.Border, BorderSize = 1 }
            };
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = AppTheme.Border;
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.FromArgb(243, 244, 246);
            btnCancel.Click += (s, e) => Close();

            var btnPlace = new Button
            {
                Text      = "  ✔  Place Order  →",
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.Success,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(200, 38),
                Location  = new Point(FORM_W - 200 - PAD, (FTR_H - 38) / 2),
                FlatAppearance = { BorderSize = 0 }
            };
            btnPlace.MouseEnter += (s, e) => btnPlace.BackColor = Color.FromArgb(15, 143, 64);
            btnPlace.MouseLeave += (s, e) => btnPlace.BackColor = AppTheme.Success;
            btnPlace.Click += PlaceOrder_Click;

            ftr.Controls.AddRange(new Control[] { btnCancel, btnPlace });
            Controls.Add(ftr);
        }

        // ── Place Order ───────────────────────────────────────────────────────
        private void PlaceOrder_Click(object sender, EventArgs e)
        {
            var order = new Order
            {
                CustomerID    = AppState.CurrentCustomer.CustomerID,
                Items         = AppState.Cart,
                Total         = AppState.CartTotal(),
                Discount      = 0,
                FinalAmount   = AppState.CartTotal(),
                PaymentMethod = cmbPayment.SelectedItem?.ToString() ?? "Cash on Pickup",
                PickupDate    = dtpPickup.Value.Date,
                Notes         = txtNotes.Text.Trim(),
                Status        = "Pending"
            };

            var (success, message, orderId) = _orderService.PlaceOrder(order);
            if (success)
            {
                if (!string.IsNullOrEmpty(_prescPath))
                    new Repository.PrescriptionRepository().Create(new Prescription
                    {
                        CustomerID = AppState.CurrentCustomer.CustomerID,
                        OrderID    = orderId,
                        Image      = _prescPath,
                        Status     = "Pending"
                    });

                AppState.Cart.Clear();
                MessageBox.Show(
                    $"Order #{orderId:D5} placed successfully!\n\nThe pharmacy will review your order shortly.",
                    "Order Placed ✔", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(message, "Order Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static Label SecTitle(string text, int w, int y) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
            AutoSize = false, Size = new Size(w, 22), Location = new Point(PAD, y)
        };

        private static Panel HRule(int w, int y) =>
            new Panel { Location = new Point(PAD, y), Size = new Size(w, 1), BackColor = Color.FromArgb(229, 231, 235) };

        private static Label MkLbl(string text, Font font, Color color, Point loc) =>
            new Label { Text = text, Font = font, ForeColor = color, AutoSize = true, Location = loc, BackColor = Color.Transparent };

        private static GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X,               r.Y,               rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y,               rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2,  0, 90);
            p.AddArc(r.X,               r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
