using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SmartMed.Models;
using SmartMed.Repository;
using SmartMed.Services;
using SmartMed.UI;
using SmartMed.Utilities;

namespace SmartMed.Forms.Admin
{
    public class MedicinePanel : UserControl
    {
        private readonly MedicineRepository _repo = new MedicineRepository();
        private readonly MedicineService _service = new MedicineService();
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbCategory, cmbStatus;
        private Label lblTotal;
        private List<Medicine> _medicines = new List<Medicine>();

        public MedicinePanel()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            // ── Top Bar ──────────────────────────────────────────
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

            cmbCategory = new ComboBox
            {
                Location = new Point(315, 15),
                Size = new Size(160, 32),
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.FontLabel,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadData();

            cmbStatus = new ComboBox
            {
                Location = new Point(490, 15),
                Size = new Size(130, 32),
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.FontLabel,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbStatus.Items.AddRange(new object[] { "All Status", "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => LoadData();

            var btnAdd = AppTheme.CreateButton("+ Add Medicine", AppTheme.Primary, 140, 36);
            btnAdd.Location = new Point(700, 13);
            btnAdd.Click += (s, e) => OpenForm(null);

            var btnExcel = AppTheme.CreateButton("Excel", AppTheme.Success, 80, 36);
            btnExcel.Location = new Point(850, 13);
            btnExcel.Click += ExportExcel_Click;

            var btnPdf = AppTheme.CreateButton("PDF", AppTheme.Danger, 70, 36);
            btnPdf.Location = new Point(940, 13);
            btnPdf.Click += ExportPdf_Click;

            lblTotal = new Label
            {
                Text = "Total: 0",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(20, 44)
            };

            pnlTop.Controls.AddRange(new Control[] { txtSearch, cmbCategory, cmbStatus, btnAdd, btnExcel, btnPdf, lblTotal });

            // ── DataGrid ─────────────────────────────────────────
            dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0 && e.RowIndex < _medicines.Count) OpenForm(_medicines[e.RowIndex]); };
            dgv.CellFormatting += Dgv_CellFormatting;

            // ── Action Row ────────────────────────────────────────
            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = AppTheme.Surface };
            var btnEdit   = AppTheme.CreateButton("✏ Edit",   AppTheme.Primary, 110, 36);
            var btnDelete = AppTheme.CreateButton("🗑 Delete", AppTheme.Danger,  110, 36);
            var btnView   = AppTheme.CreateButton("👁 View",   AppTheme.Accent,  110, 36);
            btnEdit.Location   = new Point(20, 7);
            btnDelete.Location = new Point(140, 7);
            btnView.Location   = new Point(260, 7);
            btnEdit.Click   += (s, e) => { var m = GetSelected(); if (m != null) OpenForm(m); };
            btnDelete.Click += Delete_Click;
            btnView.Click   += (s, e) => { var m = GetSelected(); if (m != null) OpenForm(m, readOnly: true); };
            pnlActions.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnView });

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            Controls.Add(pnlActions);

            LoadCategories();
        }

        private void LoadCategories()
        {
            var catRepo = new CategoryRepository();
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new ComboItem(0, "All Categories"));
            foreach (var cat in catRepo.GetAll())
                cmbCategory.Items.Add(new ComboItem(cat.CategoryID, cat.CategoryName));
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadData()
        {
            string search = txtSearch.Text.Trim();
            int catId = cmbCategory.SelectedItem is ComboItem ci ? ci.Id : 0;
            string status = cmbStatus.SelectedIndex == 0 ? "" : cmbStatus.SelectedItem.ToString();

            _medicines = _repo.GetAll(search, catId, status);
            lblTotal.Text = $"Total: {_medicines.Count} medicines";

            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Code"),
                new DataColumn("Name"),
                new DataColumn("Category"),
                new DataColumn("Supplier"),
                new DataColumn("Price"),
                new DataColumn("Stock"),
                new DataColumn("Expiry"),
                new DataColumn("Discount"),
                new DataColumn("Status"),
                new DataColumn("Rx", typeof(bool))
            });
            foreach (var m in _medicines)
                dt.Rows.Add(m.MedicineCode, m.MedicineName, m.CategoryName, m.SupplierName,
                    $"Rs. {m.FinalPrice:F2}", m.Stock,
                    m.ExpiryDate.ToString("dd/MM/yyyy"),
                    m.DiscountPercentage > 0 ? $"{m.DiscountPercentage:F0}%" : "-",
                    m.Status, m.PrescriptionRequired);

            dgv.DataSource = dt;
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["Code"].Width = 90;
                dgv.Columns["Name"].Width = 180;
                dgv.Columns["Category"].Width = 110;
                dgv.Columns["Supplier"].Width = 130;
                dgv.Columns["Price"].Width = 90;
                dgv.Columns["Stock"].Width = 60;
                dgv.Columns["Expiry"].Width = 90;
                dgv.Columns["Discount"].Width = 70;
                dgv.Columns["Status"].Width = 80;
                dgv.Columns["Rx"].Width = 40;
            }
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _medicines.Count) return;
            var med = _medicines[e.RowIndex];

            if (dgv.Columns[e.ColumnIndex].Name == "Status")
            {
                e.CellStyle.ForeColor = AppTheme.StatusColor(med.Status);
                e.CellStyle.Font = AppTheme.FontBold;
            }
            if (dgv.Columns[e.ColumnIndex].Name == "Stock")
            {
                if (med.IsLowStock) e.CellStyle.ForeColor = AppTheme.Danger;
                else if (med.Stock < med.MinimumStock * 2) e.CellStyle.ForeColor = AppTheme.Warning;
            }
        }

        private void OpenForm(Medicine med, bool readOnly = false)
        {
            var form = new MedicineForm(med, readOnly);
            if (form.ShowDialog() == DialogResult.OK)
                LoadData();
        }

        private Medicine GetSelected()
        {
            if (dgv.SelectedRows.Count == 0) return null;
            int idx = dgv.SelectedRows[0].Index;
            return (idx >= 0 && idx < _medicines.Count) ? _medicines[idx] : null;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var med = GetSelected();
            if (med == null) { MessageBox.Show("Select a medicine first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var result = MessageBox.Show($"Delete '{med.MedicineName}'?\nThis will mark the medicine as Inactive.", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var (ok, msg) = _service.DeleteMedicine(med.MedicineID);
                MessageBox.Show(msg, ok ? "Success" : "Error", MessageBoxButtons.OK,
                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                LoadData();
            }
        }

        private void ExportExcel_Click(object sender, EventArgs e)
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                new DataColumn("Code"), new DataColumn("Medicine Name"), new DataColumn("Category"),
                new DataColumn("Supplier"), new DataColumn("Price"), new DataColumn("Stock"),
                new DataColumn("Expiry Date"), new DataColumn("Status")
            });
            foreach (var m in _medicines)
                dt.Rows.Add(m.MedicineCode, m.MedicineName, m.CategoryName, m.SupplierName,
                    $"Rs. {m.Price:F2}", m.Stock, m.ExpiryDate.ToString("dd/MM/yyyy"), m.Status);
            ExcelExporter.Export(dt, "Medicines", "Medicine_Report");
        }

        private void ExportPdf_Click(object sender, EventArgs e)
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                new DataColumn("Code"), new DataColumn("Name"), new DataColumn("Price"),
                new DataColumn("Stock"), new DataColumn("Expiry"), new DataColumn("Status")
            });
            foreach (var m in _medicines)
                dt.Rows.Add(m.MedicineCode, m.MedicineName, $"Rs. {m.Price:F2}",
                    m.Stock, m.ExpiryDate.ToString("dd/MM/yyyy"), m.Status);
            PDFExporter.ExportSalesReport(dt, DateTime.Today, DateTime.Today);
        }
    }

}


