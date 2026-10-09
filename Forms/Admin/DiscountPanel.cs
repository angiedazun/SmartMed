using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Models;
using SmartMed.Repository;
using SmartMed.UI;

namespace SmartMed.Forms.Admin
{
    public class DiscountPanel : UserControl
    {
        private readonly DiscountRepository _repo = new DiscountRepository();
        private DataGridView dgv;
        private List<Discount> _discounts = new List<Discount>();
        private Discount _selected;

        public DiscountPanel()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.Transparent };
            var lblTitle = new Label { Text = "Discount Management", Font = AppTheme.FontHeader, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(20, 14) };
            var btnAdd = AppTheme.CreateButton("+ Add Discount", AppTheme.Primary, 140, 34);
            btnAdd.Location = new Point(700, 10);
            btnAdd.Click += (s, e) => OpenForm(null);
            pnlTop.Controls.AddRange(new Control[] { lblTitle, btnAdd });

            dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(dgv);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Index < _discounts.Count)
                    _selected = _discounts[dgv.SelectedRows[0].Index];
            };
            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _discounts.Count) return;
                if (dgv.Columns[e.ColumnIndex].Name == "Active")
                    e.CellStyle.ForeColor = _discounts[e.RowIndex].IsActive ? AppTheme.Success : AppTheme.TextMuted;
            };

            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = AppTheme.Surface };
            var btnEdit   = AppTheme.CreateButton("✏ Edit",   AppTheme.Primary, 110, 36);
            var btnDelete = AppTheme.CreateButton("🗑 Delete", AppTheme.Danger,  110, 36);
            btnEdit.Location = new Point(20, 7); btnDelete.Location = new Point(140, 7);
            btnEdit.Click   += (s, e) => { if (_selected != null) OpenForm(_selected); };
            btnDelete.Click += (s, e) =>
            {
                if (_selected == null) return;
                if (MessageBox.Show($"Delete '{_selected.DiscountName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _repo.Delete(_selected.DiscountID);
                    LoadData();
                }
            };
            pnlActions.Controls.AddRange(new Control[] { btnEdit, btnDelete });
            Controls.Add(dgv);
            Controls.Add(pnlTop);
            Controls.Add(pnlActions);
        }

        private void LoadData()
        {
            _discounts = _repo.GetAll();
            var dt = new DataTable();
            dt.Columns.AddRange(new[] { new DataColumn("ID"), new DataColumn("Name"), new DataColumn("Percentage"), new DataColumn("Start Date"), new DataColumn("End Date"), new DataColumn("Active") });
            foreach (var d in _discounts)
                dt.Rows.Add(d.DiscountID, d.DiscountName, $"{d.Percentage:F1}%", d.StartDate.ToString("dd/MM/yyyy"), d.EndDate.ToString("dd/MM/yyyy"), d.IsActive ? "Yes" : "No");
            dgv.DataSource = dt;
        }

        private void OpenForm(Discount d)
        {
            var form = new DiscountForm(d);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }
    }

    public class DiscountForm : Form
    {
        private readonly DiscountRepository _repo = new DiscountRepository();
        private readonly Discount _discount;
        private TextBox txtName, txtPct;
        private DateTimePicker dtpStart, dtpEnd;
        private CheckBox chkActive;

        public DiscountForm(Discount d)
        {
            _discount = d;
            Text = d == null ? "Add Discount" : "Edit Discount";
            Size = new Size(420, 430);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Build();
            if (d != null)
            {
                txtName.Text   = d.DiscountName;
                txtPct.Text    = d.Percentage.ToString("F1");
                dtpStart.Value = d.StartDate;
                dtpEnd.Value   = d.EndDate;
                chkActive.Checked = d.IsActive;
            }
        }

        private void Build()
        {
            Controls.Add(new Label { Text = Text, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(25, 20) });
            int y = 65;
            txtName  = AddTxt("Discount Name *", 25, ref y);
            txtPct   = AddTxt("Percentage (e.g. 10)", 25, ref y);

            Controls.Add(new Label { Text = "Start Date", Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(25, y) });
            y += 18;
            dtpStart = new DateTimePicker { Location = new Point(25, y), Width = 360, Format = DateTimePickerFormat.Short };
            Controls.Add(dtpStart);
            y += 34;
            Controls.Add(new Label { Text = "End Date", Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(25, y) });
            y += 18;
            dtpEnd = new DateTimePicker { Location = new Point(25, y), Width = 360, Format = DateTimePickerFormat.Short };
            Controls.Add(dtpEnd);
            y += 36;
            chkActive = new CheckBox { Text = "Active", Location = new Point(25, y), AutoSize = true, ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent, Checked = true, Cursor = Cursors.Hand };
            Controls.Add(chkActive);
            y += 28;
            var btnSave = AppTheme.CreateButton("💾 Save", AppTheme.Primary, 130, 38);
            var btnCnl  = AppTheme.CreateButton("Cancel",  AppTheme.Surface,  110, 38);
            btnSave.Location = new Point(25, y + 25); btnCnl.Location = new Point(165, y + 25);
            btnCnl.ForeColor = AppTheme.TextMuted;
            btnSave.Click += (s, e) =>
            {
                string pctText = txtPct.Text.Trim().TrimEnd('%').Trim();
                if (string.IsNullOrWhiteSpace(txtName.Text) || !decimal.TryParse(pctText, out decimal pct) || pct < 0 || pct > 100)
                {
                    MessageBox.Show("Fill required fields correctly. Percentage must be a number between 0 and 100.");
                    return;
                }
                if (dtpEnd.Value.Date < dtpStart.Value.Date)
                {
                    MessageBox.Show("End Date cannot be before Start Date.");
                    return;
                }
                var disc = new Discount { DiscountID = _discount?.DiscountID ?? 0, DiscountName = txtName.Text.Trim(), Percentage = pct, StartDate = dtpStart.Value.Date, EndDate = dtpEnd.Value.Date, IsActive = chkActive.Checked };
                bool ok = _discount == null ? _repo.Create(disc) > 0 : _repo.Update(disc);
                MessageBox.Show(ok ? "Saved!" : "Failed.", ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (ok) DialogResult = DialogResult.OK;
            };
            btnCnl.Click += (s, e) => Close();
            Controls.AddRange(new Control[] { btnSave, btnCnl });
        }

        private TextBox AddTxt(string lbl, int x, ref int y)
        {
            Controls.Add(new Label { Text = lbl, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(x, y) });
            y += 18;
            var pnl = new Panel { Location = new Point(x, y), Size = new Size(360, 30), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 4), Width = 350, BorderStyle = BorderStyle.None };
            pnl.Controls.Add(txt);
            Controls.Add(pnl);
            y += 40;
            return txt;
        }
    }
}
