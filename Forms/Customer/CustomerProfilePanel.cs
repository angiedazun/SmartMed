using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using SmartMed.Repository;
using SmartMed.UI;
using SmartMed.Utilities;

namespace SmartMed.Forms.Customer
{
    public class CustomerProfilePanel : UserControl
    {
        private readonly CustomerRepository _repo = new CustomerRepository();
        private TextBox txtFirst, txtLast, txtEmail, txtPhone, txtAddress;
        private TextBox txtOldPwd, txtNewPwd, txtConfirmPwd;
        private Label   lblPwdMsg, lblEmailErr;
        private Panel   pbAvatar;          // custom-drawn avatar
        private Label   lblAvatarInitials; // initials drawn inside

        // panels that stretch on resize
        private Panel _pnlHeader;
        private Panel _pnlLeft;
        private Panel _pnlRight;

        public CustomerProfilePanel()
        {
            BackColor  = AppTheme.Background;
            Dock       = DockStyle.Fill;
            AutoScroll = true;
            Build();
            Populate();
            // suppress horizontal scrollbar
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            AutoScrollMinSize = new Size(1, AutoScrollMinSize.Height);
            SizeChanged += (s, e) => AdjustLayout();
        }

        private void AdjustLayout()
        {
            int w = Math.Max(ClientSize.Width - 40, 700); // 20px each side
            if (_pnlHeader != null) _pnlHeader.Width = w;

            // split body into two equal columns
            int half = (w - 16) / 2;
            if (_pnlLeft != null)  { _pnlLeft.Width  = half; }
            if (_pnlRight != null) { _pnlRight.Width = half; _pnlRight.Left = _pnlLeft.Right + 16; }
        }

        private void Build()
        {
            var cust = AppState.CurrentCustomer;

            // ── Profile header banner ────────────────────────────────
            _pnlHeader = new Panel
            {
                Location  = new Point(20, 20),
                Size      = new Size(900, 110),
                BackColor = AppTheme.Primary
            };
            _pnlHeader.Paint += (s, e) =>
            {
                // right-side tint
                using var br = new SolidBrush(Color.FromArgb(40, Color.White));
                e.Graphics.FillRectangle(br, _pnlHeader.Width - 120, 0, 120, _pnlHeader.Height);
            };

            // Avatar circle
            pbAvatar = new Panel
            {
                Location  = new Point(20, 20),
                Size      = new Size(70, 70),
                BackColor = Color.FromArgb(255, 255, 255, 20)
            };
            pbAvatar.Paint += DrawAvatar;

            lblAvatarInitials = new Label
            {
                Text      = GetInitials(cust),
                Font      = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            pbAvatar.Controls.Add(lblAvatarInitials);

            // Name / sub-info labels
            _pnlHeader.Controls.Add(pbAvatar);
            _pnlHeader.Controls.Add(new Label
            {
                Text      = cust != null ? $"{cust.FirstName} {cust.LastName}" : "Customer",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(108, 18),
                BackColor = Color.Transparent
            });
            _pnlHeader.Controls.Add(new Label
            {
                Text      = cust?.Email ?? "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                AutoSize  = true,
                Location  = new Point(109, 50),
                BackColor = Color.Transparent
            });
            _pnlHeader.Controls.Add(new Label
            {
                Text      = $"Member since {cust?.RegisteredDate:MMMM yyyy}",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 255, 255, 255),
                AutoSize  = true,
                Location  = new Point(109, 72),
                BackColor = Color.Transparent
            });

            // Change Photo button (top-right of header)
            var btnPhoto = new Button
            {
                Text      = "📷  Change Photo",
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 255, 255, 255),
                Cursor    = Cursors.Hand,
                Size      = new Size(130, 30),
                Location  = new Point(_pnlHeader.Width - 148, 40),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top
            };
            btnPhoto.FlatAppearance.BorderColor = Color.FromArgb(120, 255, 255, 255);
            btnPhoto.Click += (s, e) =>
            {
                string path = ImageUploader.UploadProfileImage();
                if (path != null && AppState.CurrentCustomer != null)
                {
                    AppState.CurrentCustomer.ProfileImage = path;
                    _repo.Update(AppState.CurrentCustomer);
                    // Re-draw avatar with photo
                    pbAvatar.Tag = Image.FromFile(path);
                    lblAvatarInitials.Visible = false;
                    pbAvatar.Invalidate();
                }
            };
            _pnlHeader.Controls.Add(btnPhoto);
            Controls.Add(_pnlHeader);

            // ── Two-column body ──────────────────────────────────────
            int bodyY = 20 + 110 + 14;      // header bottom + gap
            int bodyW = 442;                  // each column; AdjustLayout fixes on resize

            // ── LEFT: Personal Information ───────────────────────────
            _pnlLeft = MakeCard(20, bodyY, bodyW, 400, "👤  Personal Information");

            int ly = 52;
            txtFirst   = AddField(_pnlLeft, "First Name",  ref ly);
            txtLast    = AddField(_pnlLeft, "Last Name",   ref ly);
            txtEmail   = AddField(_pnlLeft, "Email",       ref ly);
            lblEmailErr = new Label { Location = new Point(16, ly - 8), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, Visible = false };
            _pnlLeft.Controls.Add(lblEmailErr);
            txtPhone   = AddField(_pnlLeft, "Phone",   ref ly);
            txtAddress = AddField(_pnlLeft, "Address", ref ly);

            var btnSave = AppTheme.CreateButton("💾 Save Profile", AppTheme.Primary, 160, 38);
            btnSave.Location = new Point(16, ly + 8);
            btnSave.Click   += SaveProfile_Click;
            _pnlLeft.Controls.Add(btnSave);
            _pnlLeft.Height = ly + 8 + 38 + 18;  // exact fit
            Controls.Add(_pnlLeft);

            // ── RIGHT: Change Password ───────────────────────────────
            _pnlRight = MakeCard(_pnlLeft.Right + 16, bodyY, bodyW, 300, "🔒  Change Password");

            int ry = 52;
            txtOldPwd     = AddField(_pnlRight, "Current Password",     ref ry, isPassword: true);
            txtNewPwd     = AddField(_pnlRight, "New Password",         ref ry, isPassword: true);
            txtConfirmPwd = AddField(_pnlRight, "Confirm New Password", ref ry, isPassword: true);

            // Tip note — inside the card so it always shows correctly
            var lblTip = new Label
            {
                Text      = "ℹ  Password must be 8+ characters with uppercase, lowercase, number and special character.",
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = false,
                Size      = new Size(_pnlRight.Width - 32, 34),
                Location  = new Point(16, ry + 2),
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            _pnlRight.Controls.Add(lblTip);

            lblPwdMsg = new Label { Location = new Point(16, ry + 40), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, Visible = false };
            _pnlRight.Controls.Add(lblPwdMsg);

            var btnPwd = AppTheme.CreateButton("🔒 Change Password", AppTheme.Danger, 180, 38);
            btnPwd.Location = new Point(16, ry + 58);
            btnPwd.Click   += ChangePassword_Click;
            _pnlRight.Controls.Add(btnPwd);
            _pnlRight.Height = ry + 58 + 38 + 18;
            Controls.Add(_pnlRight);
        }

        // ── Helpers ───────────────────────────────────────────────────
        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            // Header strip
            var hdr = new Panel { Location = new Point(0, 0), Size = new Size(w, 44), BackColor = Color.White, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            hdr.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
                // accent left bar
                using var br = new SolidBrush(AppTheme.Primary);
                e.Graphics.FillRectangle(br, 0, 0, 4, hdr.Height);
            };
            hdr.Controls.Add(new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(18, 13),
                BackColor = Color.Transparent
            });
            card.Controls.Add(hdr);
            return card;
        }

        private TextBox AddField(Panel panel, string label, ref int y, bool isPassword = false)
        {
            panel.Controls.Add(new Label
            {
                Text      = label,
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(16, y)
            });
            y += 20;
            var wrap = new Panel
            {
                Location    = new Point(16, y),
                Size        = new Size(panel.Width - 32, 30),
                BackColor   = AppTheme.InputBg,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor      = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            var txt = new TextBox
            {
                BackColor            = AppTheme.InputBg,
                ForeColor            = AppTheme.TextPrimary,
                Font                 = AppTheme.FontInput,
                Location             = new Point(4, 4),
                Width                = wrap.Width - 10,
                BorderStyle          = BorderStyle.None,
                UseSystemPasswordChar = isPassword,
                Anchor               = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            wrap.Controls.Add(txt);
            panel.Controls.Add(wrap);
            y += 44;
            return txt;
        }

        private void DrawAvatar(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            var pnl = (Panel)sender;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var img = pnl.Tag as Image;
            using var br = new SolidBrush(Color.White);
            g.FillEllipse(br, 0, 0, pnl.Width - 1, pnl.Height - 1);

            if (img != null)
            {
                var path = new GraphicsPath();
                path.AddEllipse(0, 0, pnl.Width - 1, pnl.Height - 1);
                g.SetClip(path);
                g.DrawImage(img, 0, 0, pnl.Width, pnl.Height);
                g.ResetClip();
            }

            using var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 2);
            g.DrawEllipse(pen, 1, 1, pnl.Width - 3, pnl.Height - 3);
        }

        private static string GetInitials(SmartMed.Models.Customer c)
        {
            if (c == null) return "?";
            string f = c.FirstName?.Length > 0 ? c.FirstName[0].ToString() : "";
            string l = c.LastName?.Length  > 0 ? c.LastName[0].ToString()  : "";
            return (f + l).ToUpper();
        }

        private void Populate()
        {
            var c = AppState.CurrentCustomer;
            if (c == null) return;
            txtFirst.Text   = c.FirstName;
            txtLast.Text    = c.LastName;
            txtEmail.Text   = c.Email;
            txtPhone.Text   = c.Phone;
            txtAddress.Text = c.Address;
            if (!string.IsNullOrEmpty(c.ProfileImage) && File.Exists(c.ProfileImage))
            {
                pbAvatar.Tag = Image.FromFile(c.ProfileImage);
                lblAvatarInitials.Visible = false;
                pbAvatar.Invalidate();
            }
        }

        private void SaveProfile_Click(object sender, EventArgs e)
        {
            lblEmailErr.Visible = false;
            if (!Validation.IsValidEmail(txtEmail.Text))
            {
                lblEmailErr.Text = "⚠ Invalid email"; lblEmailErr.Visible = true; return;
            }
            var c = AppState.CurrentCustomer;
            if (c == null) return;
            c.FirstName = txtFirst.Text.Trim();
            c.LastName  = txtLast.Text.Trim();
            c.Email     = txtEmail.Text.Trim();
            c.Phone     = txtPhone.Text.Trim();
            c.Address   = txtAddress.Text.Trim();
            bool ok = _repo.Update(c);
            MessageBox.Show(ok ? "Profile updated!" : "Failed.", ok ? "Success" : "Error",
                MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void ChangePassword_Click(object sender, EventArgs e)
        {
            lblPwdMsg.ForeColor = AppTheme.Danger;
            lblPwdMsg.Visible   = false;
            var c = AppState.CurrentCustomer;
            if (c == null) return;
            if (!PasswordHasher.Verify(txtOldPwd.Text, c.Password))
            {
                lblPwdMsg.Text = "⚠ Current password is incorrect"; lblPwdMsg.Visible = true; return;
            }
            if (!Validation.IsStrongPassword(txtNewPwd.Text))
            {
                lblPwdMsg.Text = "⚠ Too weak — 8+ chars, upper, lower, number, symbol"; lblPwdMsg.Visible = true; return;
            }
            if (txtNewPwd.Text != txtConfirmPwd.Text)
            {
                lblPwdMsg.Text = "⚠ Passwords do not match"; lblPwdMsg.Visible = true; return;
            }
            string hashed = PasswordHasher.Hash(txtNewPwd.Text);
            bool ok = _repo.UpdatePassword(c.CustomerID, hashed);
            if (ok)
            {
                c.Password = hashed;
                txtOldPwd.Clear(); txtNewPwd.Clear(); txtConfirmPwd.Clear();
                lblPwdMsg.ForeColor = AppTheme.Success;
                lblPwdMsg.Text      = "✓ Password changed successfully!";
                lblPwdMsg.Visible   = true;
            }
            else
            {
                lblPwdMsg.Text = "⚠ Failed to change password"; lblPwdMsg.Visible = true;
            }
        }
    }
}
