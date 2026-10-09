using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SmartMed.Models;
using SmartMed.Repository;
using SmartMed.UI;

namespace SmartMed.Forms.Customer
{
    public class SearchMedicinePanel : UserControl
    {
        private readonly CustomerMainForm _main;
        private readonly MedicineRepository _medRepo = new MedicineRepository();
        private readonly CategoryRepository _catRepo  = new CategoryRepository();
        private FlowLayoutPanel _flp;
        private TextBox  txtSearch;
        private ComboBox cmbCategory;
        private CheckBox chkStock, chkDiscount;
        private Label    lblCount;
        private List<Medicine> _medicines = new List<Medicine>();

        public SearchMedicinePanel(CustomerMainForm main)
        {
            _main     = main;
            BackColor = AppTheme.Background;
            Dock      = DockStyle.Fill;
            Build();
            LoadCategories();
            LoadMedicines();
        }

        // ── Layout ────────────────────────────────────────────────────────────
        private void Build()
        {
            // ── Filter bar ────────────────────────────────────────────────────
            var pnlBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = Color.White
            };
            pnlBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlBar.Height - 1, pnlBar.Width, pnlBar.Height - 1);

            // Search box with placeholder
            txtSearch = new TextBox
            {
                Location    = new Point(20, 16),
                Size        = new Size(340, 34),
                BackColor   = AppTheme.Background,
                ForeColor   = AppTheme.TextPrimary,
                Font        = AppTheme.FontInput,
                BorderStyle = BorderStyle.FixedSingle
            };
            SetPlaceholder(txtSearch, "Search medicine name, code…");
            txtSearch.TextChanged += (s, e) => LoadMedicines();

            cmbCategory = new ComboBox
            {
                Location      = new Point(375, 17),
                Size          = new Size(180, 34),
                BackColor     = AppTheme.Background,
                ForeColor     = AppTheme.TextPrimary,
                Font          = AppTheme.FontLabel,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadMedicines();

            chkStock = new CheckBox
            {
                Text      = "In Stock Only",
                Location  = new Point(570, 20),
                AutoSize  = true,
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };
            chkDiscount = new CheckBox
            {
                Text      = "Discounted",
                Location  = new Point(690, 20),
                AutoSize  = true,
                Font      = AppTheme.FontLabel,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };
            chkStock.CheckedChanged    += (s, e) => LoadMedicines();
            chkDiscount.CheckedChanged += (s, e) => LoadMedicines();

            lblCount = new Label
            {
                Text      = "",
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(20, 45)
            };

            pnlBar.Controls.AddRange(new Control[] { txtSearch, cmbCategory, chkStock, chkDiscount, lblCount });

            // ── Card grid ─────────────────────────────────────────────────────
            _flp = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                BackColor     = AppTheme.Background,
                AutoScroll    = true,
                Padding       = new Padding(16, 16, 0, 16),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true
            };

            Controls.Add(_flp);
            Controls.Add(pnlBar);
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new ComboItem(0, "All Categories"));
            foreach (var c in _catRepo.GetAll())
                cmbCategory.Items.Add(new ComboItem(c.CategoryID, c.CategoryName));
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadMedicines()
        {
            int    catId  = (cmbCategory.SelectedItem as ComboItem)?.Id ?? 0;
            string search = txtSearch.ForeColor == AppTheme.TextMuted ? "" : txtSearch.Text.Trim();

            _medicines = _medRepo.GetAll(search, catId, "Active");
            if (chkStock.Checked)    _medicines = _medicines.FindAll(m => m.Stock > 0);
            if (chkDiscount.Checked) _medicines = _medicines.FindAll(m => m.DiscountPercentage > 0);

            lblCount.Text = $"{_medicines.Count} medicine{(_medicines.Count != 1 ? "s" : "")} found";

            _flp.SuspendLayout();
            _flp.Controls.Clear();

            if (_medicines.Count == 0)
            {
                _flp.Controls.Add(MakeEmptyState());
            }
            else
            {
                foreach (var med in _medicines)
                    _flp.Controls.Add(MakeMedicineCard(med));
            }
            _flp.ResumeLayout();
        }

        // ── Medicine Card ─────────────────────────────────────────────────────
        private Panel MakeMedicineCard(Medicine med)
        {
            bool inStock = med.Stock > 0;

            var card = new Panel
            {
                Size      = new Size(234, 348),
                BackColor = Color.White,
                Margin    = new Padding(0, 0, 12, 12),
                Cursor    = Cursors.Default
            };

            // Border + shadow via Paint
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Drop shadow
                using var shadow = new SolidBrush(Color.FromArgb(12, 0, 0, 0));
                g.FillRectangle(shadow, 3, 3, card.Width - 2, card.Height - 2);
                // Card border
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                g.DrawRectangle(pen, 0, 0, card.Width - 4, card.Height - 4);
            };

            // ── Image area ────────────────────────────────────────────────────
            var pnlImg = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(230, 128),
                BackColor = Color.FromArgb(239, 246, 255)
            };
            pnlImg.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var br = new LinearGradientBrush(
                    pnlImg.ClientRectangle,
                    Color.FromArgb(239, 246, 255),
                    Color.FromArgb(219, 234, 254),
                    LinearGradientMode.Vertical);
                g.FillRectangle(br, pnlImg.ClientRectangle);
                using var sep = new Pen(Color.FromArgb(186, 212, 250));
                g.DrawLine(sep, 0, pnlImg.Height - 1, pnlImg.Width, pnlImg.Height - 1);
            };

            // Pill icon (centred)
            var lblIcon = new Label
            {
                Text      = "💊",
                Font      = new Font("Segoe UI Emoji", 38f),
                ForeColor = Color.FromArgb(147, 197, 253),
                Size      = new Size(230, 128),
                Location  = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlImg.Controls.Add(lblIcon);

            // Real image if present
            if (!string.IsNullOrEmpty(med.Image) && System.IO.File.Exists(med.Image))
            {
                try
                {
                    var pb = new PictureBox
                    {
                        Location  = new Point(0, 0),
                        Size      = new Size(230, 128),
                        SizeMode  = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent
                    };
                    pb.Image = Image.FromFile(med.Image);
                    pnlImg.Controls.Add(pb);
                    pb.BringToFront();
                    lblIcon.Visible = false;
                }
                catch { }
            }

            // Out-of-stock overlay
            if (!inStock)
            {
                var overlay = new Panel
                {
                    Location  = new Point(0, 0),
                    Size      = new Size(230, 128),
                    BackColor = Color.FromArgb(100, 255, 255, 255)
                };
                var oos = new Label
                {
                    Text      = "Out of Stock",
                    Font      = AppTheme.FontBold,
                    ForeColor = AppTheme.Danger,
                    Size      = new Size(230, 128),
                    Location  = new Point(0, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                overlay.Controls.Add(oos);
                pnlImg.Controls.Add(overlay);
                overlay.BringToFront();
            }

            // Discount badge
            if (med.DiscountPercentage > 0)
            {
                var badge = new Label
                {
                    Text      = $"-{med.DiscountPercentage:F0}%",
                    Font      = AppTheme.FontBold,
                    ForeColor = Color.White,
                    BackColor = AppTheme.Danger,
                    AutoSize  = true,
                    Location  = new Point(8, 8),
                    Padding   = new Padding(5, 2, 5, 2)
                };
                pnlImg.Controls.Add(badge);
                badge.BringToFront();
            }

            // Rx badge
            if (med.PrescriptionRequired)
            {
                var rx = new Label
                {
                    Text      = "Rx",
                    Font      = AppTheme.FontBold,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(245, 158, 11),
                    AutoSize  = true,
                    Location  = new Point(195, 8),
                    Padding   = new Padding(5, 2, 5, 2)
                };
                pnlImg.Controls.Add(rx);
                rx.BringToFront();
            }

            card.Controls.Add(pnlImg);

            // ── Info area ─────────────────────────────────────────────────────
            // Name
            var lblName = new Label
            {
                Text      = med.MedicineName,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location  = new Point(12, 136),
                Size      = new Size(206, 38),
                AutoEllipsis = true
            };

            // Category · Dosage
            string catDosage = med.CategoryName;
            if (!string.IsNullOrWhiteSpace(med.Dosage)) catDosage += $"  ·  {med.Dosage}";
            var lblCat = new Label
            {
                Text      = catDosage,
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                Location  = new Point(12, 176),
                Size      = new Size(206, 16),
                AutoEllipsis = true
            };

            // Price row
            var lblPrice = new Label
            {
                Text      = $"Rs. {med.FinalPrice:F2}",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                AutoSize  = true,
                Location  = new Point(12, 196)
            };

            card.Controls.AddRange(new Control[] { lblName, lblCat, lblPrice });

            if (med.DiscountPercentage > 0)
            {
                var lblOld = new Label
                {
                    Text      = $"Rs. {med.Price:F2}",
                    Font      = AppTheme.FontSmall,
                    ForeColor = AppTheme.TextMuted,
                    AutoSize  = true,
                    Location  = new Point(118, 204)
                };
                lblOld.Paint += (s, e) =>
                {
                    using var p = new Pen(AppTheme.TextMuted);
                    e.Graphics.DrawLine(p, 0, lblOld.Height / 2, lblOld.Width, lblOld.Height / 2);
                };
                card.Controls.Add(lblOld);
            }

            // Stock indicator
            var lblStock = new Label
            {
                Text      = inStock ? $"● In Stock  ({med.Stock} available)" : "● Out of Stock",
                Font      = AppTheme.FontSmall,
                ForeColor = inStock ? AppTheme.Success : AppTheme.Danger,
                AutoSize  = true,
                Location  = new Point(12, 222)
            };
            card.Controls.Add(lblStock);

            // Separator
            var sep2 = new Panel { Location = new Point(12, 246), Size = new Size(206, 1), BackColor = AppTheme.Border };
            card.Controls.Add(sep2);

            // ── Qty selector row ──────────────────────────────────────────────
            var lblQtyHint = new Label
            {
                Text      = "Qty:",
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(12, 263)
            };

            int qty = 1;

            var btnMinus = new Button
            {
                Text     = "−",
                Size     = new Size(30, 30),
                Location = new Point(62, 254),
                FlatStyle = FlatStyle.Flat,
                Font     = new Font("Segoe UI", 12f, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = AppTheme.TextPrimary,
                Cursor   = Cursors.Hand,
                Enabled  = inStock,
                FlatAppearance = { BorderColor = AppTheme.Border, BorderSize = 1 }
            };

            var lblQty = new Label
            {
                Text      = "1",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Size      = new Size(44, 30),
                Location  = new Point(96, 254),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = AppTheme.Background,
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnPlus = new Button
            {
                Text     = "+",
                Size     = new Size(30, 30),
                Location = new Point(144, 254),
                FlatStyle = FlatStyle.Flat,
                Font     = new Font("Segoe UI", 12f, FontStyle.Bold),
                BackColor = AppTheme.Primary,
                ForeColor = Color.White,
                Cursor   = Cursors.Hand,
                Enabled  = inStock,
                FlatAppearance = { BorderSize = 0 }
            };

            if (inStock)
            {
                btnMinus.Click += (s, e) =>
                {
                    if (qty > 1) { qty--; lblQty.Text = qty.ToString(); }
                };
                btnPlus.Click += (s, e) =>
                {
                    if (qty < med.Stock) { qty++; lblQty.Text = qty.ToString(); }
                    else
                        MessageBox.Show($"Maximum available stock: {med.Stock}", "Stock Limit",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
            }

            card.Controls.AddRange(new Control[] { lblQtyHint, btnMinus, lblQty, btnPlus });

            // ── Add to Cart button ────────────────────────────────────────────
            var btnAdd = AppTheme.CreateButton(
                inStock ? "🛒  Add to Cart" : "Out of Stock",
                inStock ? AppTheme.Primary : AppTheme.TextMuted,
                206, 36);
            btnAdd.Location = new Point(12, 294);
            btnAdd.Enabled  = inStock;
            if (inStock)
                btnAdd.Click += (s, e) => AddToCart(med, btnAdd, () => qty);

            card.Controls.Add(btnAdd);
            return card;
        }

        // ── Empty state ───────────────────────────────────────────────────────
        private Control MakeEmptyState()
        {
            var pnl = new Panel { Size = new Size(500, 300), BackColor = Color.Transparent };
            var lbl = new Label
            {
                Text      = "🔍\r\nNo medicines found.\r\nTry a different search or category.",
                Font      = new Font("Segoe UI", 11f),
                ForeColor = AppTheme.TextMuted,
                Size      = new Size(500, 300),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnl.Controls.Add(lbl);
            return pnl;
        }

        // ── Add to Cart logic ─────────────────────────────────────────────────
        private void AddToCart(Medicine med, Button btn, Func<int> getQty)
        {
            int qty = getQty();
            var existing = AppState.Cart.Find(i => i.MedicineID == med.MedicineID);
            if (existing != null)
            {
                int newQty = existing.Quantity + qty;
                if (newQty > med.Stock)
                {
                    MessageBox.Show(
                        $"Only {med.Stock} unit(s) available.\nYou already have {existing.Quantity} in your cart.",
                        "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existing.Quantity = newQty;
                existing.Subtotal = existing.Quantity * existing.Price;
            }
            else
            {
                AppState.Cart.Add(new OrderItem
                {
                    MedicineID   = med.MedicineID,
                    MedicineName = med.MedicineName,
                    MedicineCode = med.MedicineCode,
                    Price        = med.FinalPrice,
                    Quantity     = qty,
                    Subtotal     = med.FinalPrice * qty
                });
            }
            _main.UpdateCartBadge();

            btn.Text      = $"✓  Added {qty}!";
            btn.BackColor = AppTheme.Success;
            var t = new System.Windows.Forms.Timer { Interval = 1400 };
            t.Tick += (s, e) =>
            {
                btn.Text      = "🛒  Add to Cart";
                btn.BackColor = AppTheme.Primary;
                t.Stop(); t.Dispose();
            };
            t.Start();
        }

        // ── Placeholder helper ────────────────────────────────────────────────
        private static void SetPlaceholder(TextBox tb, string text)
        {
            tb.Text      = text;
            tb.ForeColor = AppTheme.TextMuted;
            tb.GotFocus += (s, e) =>
            {
                if (tb.ForeColor == AppTheme.TextMuted)
                { tb.Text = ""; tb.ForeColor = AppTheme.TextPrimary; }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                { tb.Text = text; tb.ForeColor = AppTheme.TextMuted; }
            };
        }
    }
}
