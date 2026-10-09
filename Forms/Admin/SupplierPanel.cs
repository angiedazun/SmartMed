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
    public class SupplierPanel : UserControl
    {
        private readonly SupplierRepository _repo = new SupplierRepository();
        private DataGridView dgv;
        private TextBox txtSearch;
        private Label lblTotal;
        private List<Supplier> _suppliers = new List<Supplier>();
        private Supplier _selected;

        public SupplierPanel()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            txtSearch = new TextBox { Location = new Point(20, 15), Size = new Size(280, 32), BackColor = AppTheme.Surface, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (s, e) => LoadData();
            lblTotal = new Label { Location = new Point(20, 44), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMuted };
            var btnAdd = AppTheme.CreateButton("+ Add Supplier", AppTheme.Primary, 140, 36);
            btnAdd.Location = new Point(700, 13);
            btnAdd.Click += (s, e) => OpenForm(null);
            pnlTop.Controls.AddRange(new Control[] { txtSearch, lblTotal, btnAdd });

            dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(dgv);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Index < _suppliers.Count)
                    _selected = _suppliers[dgv.SelectedRows[0].Index];
            };

            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = AppTheme.Surface };
            var btnEdit   = AppTheme.CreateButton("✏ Edit",   AppTheme.Primary, 110, 36);
            var btnDelete = AppTheme.CreateButton("🗑 Delete", AppTheme.Danger,  110, 36);
            btnEdit.Location   = new Point(20, 7);
            btnDelete.Location = new Point(140, 7);
            btnEdit.Click   += (s, e) => { if (_selected != null) OpenForm(_selected); };
            btnDelete.Click += Delete_Click;
            pnlActions.Controls.AddRange(new Control[] { btnEdit, btnDelete });

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            Controls.Add(pnlActions);
        }

        private void LoadData()
        {
            _suppliers = _repo.GetAll(txtSearch.Text.Trim());
            lblTotal.Text = $"Total: {_suppliers.Count}";
            var dt = new DataTable();
            dt.Columns.AddRange(new[] { new DataColumn("ID"), new DataColumn("Name"), new DataColumn("Company"), new DataColumn("Phone"), new DataColumn("Email"), new DataColumn("Address") });
            foreach (var s in _suppliers)
                dt.Rows.Add(s.SupplierID, s.SupplierName, s.Company, s.Phone, s.Email, s.Address);
            dgv.DataSource = dt;
        }

        private void OpenForm(Supplier s)
        {
            var form = new SupplierForm(s);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (_selected == null) return;
            if (MessageBox.Show($"Delete '{_selected.SupplierName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool ok = _repo.Delete(_selected.SupplierID);
                MessageBox.Show(ok ? "Supplier deactivated." : "Failed.", ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                LoadData();
            }
        }
    }

    public class SupplierForm : Form
    {
        private readonly SupplierRepository _repo = new SupplierRepository();
        private readonly Supplier _supplier;
        private TextBox txtName, txtCompany, txtPhone, txtEmail, txtAddress;

        public SupplierForm(Supplier supplier)
        {
            _supplier = supplier;
            Text = supplier == null ? "Add Supplier" : "Edit Supplier";
            Size = new Size(460, 490);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Build();
            if (supplier != null) Populate();
        }

        private void Build()
        {
            var lbl = new Label { Text = Text, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(25, 20) };
            Controls.Add(lbl);
            int y = 62;
            txtName    = AddField("Supplier Name *", ref y);
            txtCompany = AddField("Company",         ref y);
            txtPhone   = AddField("Phone",           ref y);
            txtEmail   = AddField("Email",           ref y);
            txtAddress = AddField("Address",         ref y);
            var btnSave   = AppTheme.CreateButton("💾 Save", AppTheme.Primary, 140, 38);
            var btnCancel = AppTheme.CreateButton("Cancel",  AppTheme.Surface,  110, 38);
            btnSave.Location   = new Point(25, y + 25);
            btnCancel.Location = new Point(175, y + 25);
            btnCancel.ForeColor = AppTheme.TextMuted;
            btnSave.Click   += Save_Click;
            btnCancel.Click += (s, e) => Close();
            Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private TextBox AddField(string label, ref int y)
        {
            Controls.Add(new Label { Text = label, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(25, y) });
            y += 18;
            var pnl = new Panel { Location = new Point(25, y), Size = new Size(390, 30), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 4), Width = 380, BorderStyle = BorderStyle.None };
            pnl.Controls.Add(txt);
            Controls.Add(pnl);
            y += 42;
            return txt;
        }

        private void Populate()
        {
            txtName.Text    = _supplier.SupplierName;
            txtCompany.Text = _supplier.Company;
            txtPhone.Text   = _supplier.Phone;
            txtEmail.Text   = _supplier.Email;
            txtAddress.Text = _supplier.Address;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Name required."); return; }
            var s = new Supplier
            {
                SupplierID   = _supplier?.SupplierID ?? 0,
                SupplierName = txtName.Text.Trim(),
                Company      = txtCompany.Text.Trim(),
                Phone        = txtPhone.Text.Trim(),
                Email        = txtEmail.Text.Trim(),
                Address      = txtAddress.Text.Trim()
            };
            bool ok = _supplier == null ? _repo.Create(s) > 0 : _repo.Update(s);
            MessageBox.Show(ok ? "Saved!" : "Failed.", ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (ok) DialogResult = DialogResult.OK;
        }
    }
}


