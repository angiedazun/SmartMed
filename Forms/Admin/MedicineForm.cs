using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SmartMed.Models;
using SmartMed.Repository;
using SmartMed.Services;
using SmartMed.UI;
using SmartMed.Utilities;

namespace SmartMed.Forms.Admin
{
    public class MedicineForm : Form
    {
        private readonly MedicineService _service = new MedicineService();
        private readonly MedicineRepository _repo = new MedicineRepository();
        private readonly CategoryRepository _catRepo = new CategoryRepository();
        private readonly SupplierRepository _supRepo = new SupplierRepository();
        private readonly DiscountRepository _discRepo = new DiscountRepository();

        private readonly Medicine _medicine;
        private readonly bool _readOnly;
        private string _imagePath;

        private TextBox txtCode, txtName, txtDosage, txtUnit, txtPrice, txtStock, txtMinStock, txtDesc;
        private ComboBox cmbCategory, cmbSupplier, cmbDiscount, cmbStatus;
        private DateTimePicker dtpExpiry;
        private CheckBox chkPrescription;
        private PictureBox pbImage;
        private Button btnSave, btnCancel, btnUploadImg;
        private Label lblCodeErr, lblNameErr, lblPriceErr, lblStockErr, lblExpiryErr;

        public MedicineForm(Medicine medicine, bool readOnly = false)
        {
            _medicine = medicine;
            _readOnly = readOnly;
            _imagePath = medicine?.Image;
            InitForm();
            BuildLayout();
            if (medicine != null) PopulateFields();
        }

        private void InitForm()
        {
            Text = _medicine == null ? "Add Medicine" : (_readOnly ? "View Medicine" : "Edit Medicine");
            Size = new Size(740, 660);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = AppTheme.FontLabel;
        }

        private void BuildLayout()
        {
            var lblTitle = new Label
            {
                Text = Text,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 20)
            };
            Controls.Add(lblTitle);

            // Left column
            int lx = 25, ly = 58, fh = 28, gap = 44;

            txtCode = AddField("Medicine Code *", lx, ref ly, fh, gap);
            var btnGenCode = AppTheme.CreateButton("Auto", AppTheme.Surface, 60, 26);
            btnGenCode.Location = new Point(lx + 230, ly - gap + 1);
            btnGenCode.ForeColor = AppTheme.TextMuted;
            btnGenCode.Click += (s, e) => txtCode.Text = _service.GenerateCode();
            Controls.Add(btnGenCode);
            lblCodeErr = AddErrLabel(lx, ref ly);

            txtName = AddField("Medicine Name *", lx, ref ly, fh, gap);
            lblNameErr = AddErrLabel(lx, ref ly);

            cmbCategory = AddCombo("Category *", lx, ref ly, gap);
            cmbSupplier = AddCombo("Supplier *", lx, ref ly, gap);
            txtDosage = AddField("Dosage", lx, ref ly, fh, gap);
            txtUnit   = AddField("Unit (e.g. Tablet)", lx, ref ly, fh, gap);

            // Right column
            int rx = 390, ry = 58;

            txtPrice = AddField("Price (Rs.) *", rx, ref ry, fh, gap);
            lblPriceErr = AddErrLabel(rx, ref ry);
            txtStock = AddField("Stock Quantity *", rx, ref ry, fh, gap);
            lblStockErr = AddErrLabel(rx, ref ry);
            txtMinStock = AddField("Minimum Stock *", rx, ref ry, fh, gap);

            var lblExpiry = CreateLabel("Expiry Date *", rx, ry);
            ry += 18;
            dtpExpiry = new DateTimePicker
            {
                Location = new Point(rx, ry),
                Width = 290,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today.AddDays(1),
                CalendarForeColor = AppTheme.TextPrimary,
                CalendarMonthBackground = AppTheme.Surface,
                BackColor = AppTheme.InputBg,
                ForeColor = AppTheme.TextPrimary
            };
            Controls.AddRange(new Control[] { lblExpiry, dtpExpiry });
            ry += gap;

            lblExpiryErr = AddErrLabel(rx, ref ry);
            cmbDiscount = AddCombo("Discount", rx, ref ry, gap);
            cmbStatus   = AddCombo("Status", rx, ref ry, gap);
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;

            chkPrescription = new CheckBox
            {
                Text = "Prescription Required",
                Location = new Point(rx, ry),
                AutoSize = true,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Font = AppTheme.FontLabel,
                Cursor = Cursors.Hand
            };
            Controls.Add(chkPrescription);
            ry += 26;

            // Image
            var lblImg = CreateLabel("Medicine Image", rx, ry);
            ry += 18;
            pbImage = new PictureBox
            {
                Location = new Point(rx, ry),
                Size = new Size(80, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppTheme.Surface,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            btnUploadImg = AppTheme.CreateButton("Upload", AppTheme.Surface, 80, 28);
            btnUploadImg.Location = new Point(rx + 90, ry + 25);
            btnUploadImg.ForeColor = AppTheme.TextMuted;
            btnUploadImg.Click += UploadImage_Click;
            Controls.AddRange(new Control[] { lblImg, pbImage, btnUploadImg });

            // Description
            var lblDesc = CreateLabel("Description", lx, ly + 6);
            ly += 24;
            txtDesc = new TextBox
            {
                Location = new Point(lx, ly),
                Size = new Size(330, 60),
                BackColor = AppTheme.InputBg,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.FontLabel,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            Controls.AddRange(new Control[] { lblDesc, txtDesc });

            // Buttons — placed below whichever column is tallest
            int btnY = 566;
            if (!_readOnly)
            {
                btnSave = AppTheme.CreateButton("💾 Save", AppTheme.Primary, 150, 40);
                btnSave.Location = new Point(25, btnY);
                btnSave.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
                btnSave.Click += Save_Click;
                Controls.Add(btnSave);
            }
            btnCancel = AppTheme.CreateButton(_readOnly ? "Close" : "Cancel", AppTheme.Surface, 120, 40);
            btnCancel.Location = new Point(185, btnY);
            btnCancel.ForeColor = AppTheme.TextMuted;
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            LoadComboData();
            if (_readOnly) SetReadOnly();
        }

        private void LoadComboData()
        {
            cmbCategory.Items.Clear();
            foreach (var c in _catRepo.GetAll()) cmbCategory.Items.Add(new ComboItem(c.CategoryID, c.CategoryName));
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;

            cmbSupplier.Items.Clear();
            foreach (var s in _supRepo.GetAll()) cmbSupplier.Items.Add(new ComboItem(s.SupplierID, s.SupplierName));
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;

            cmbDiscount.Items.Clear();
            cmbDiscount.Items.Add(new ComboItem(0, "No Discount"));
            foreach (var d in _discRepo.GetAll()) cmbDiscount.Items.Add(new ComboItem(d.DiscountID, d.DiscountName));
            cmbDiscount.SelectedIndex = 0;
        }

        private void PopulateFields()
        {
            txtCode.Text    = _medicine.MedicineCode;
            txtName.Text    = _medicine.MedicineName;
            txtDosage.Text  = _medicine.Dosage;
            txtUnit.Text    = _medicine.Unit;
            txtPrice.Text   = _medicine.Price.ToString("F2");
            txtStock.Text   = _medicine.Stock.ToString();
            txtMinStock.Text= _medicine.MinimumStock.ToString();
            dtpExpiry.Value = _medicine.ExpiryDate > DateTime.Today ? _medicine.ExpiryDate : DateTime.Today.AddDays(1);
            txtDesc.Text    = _medicine.Description;
            chkPrescription.Checked = _medicine.PrescriptionRequired;

            foreach (ComboItem item in cmbCategory.Items)
                if (item.Id == _medicine.CategoryID) { cmbCategory.SelectedItem = item; break; }
            foreach (ComboItem item in cmbSupplier.Items)
                if (item.Id == _medicine.SupplierID) { cmbSupplier.SelectedItem = item; break; }
            foreach (ComboItem item in cmbDiscount.Items)
                if (item.Id == (_medicine.DiscountID ?? 0)) { cmbDiscount.SelectedItem = item; break; }

            cmbStatus.SelectedItem = _medicine.Status ?? "Active";

            if (!string.IsNullOrEmpty(_medicine.Image) && File.Exists(_medicine.Image))
                pbImage.Image = Image.FromFile(_medicine.Image);
        }

        private void SetReadOnly()
        {
            foreach (Control c in Controls)
            {
                if (c is TextBox t) t.ReadOnly = true;
                if (c is ComboBox cb) cb.Enabled = false;
                if (c is DateTimePicker dp) dp.Enabled = false;
                if (c is CheckBox chk) chk.Enabled = false;
            }
            if (btnUploadImg != null) btnUploadImg.Enabled = false;
        }

        private void UploadImage_Click(object sender, EventArgs e)
        {
            string path = ImageUploader.UploadMedicineImage();
            if (path != null)
            {
                _imagePath = path;
                pbImage.Image = Image.FromFile(path);
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            ClearErrors();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtCode.Text)) { ShowErr(lblCodeErr, "Code required"); valid = false; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { ShowErr(lblNameErr, "Name required"); valid = false; }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0) { ShowErr(lblPriceErr, "Valid price required"); valid = false; }
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0) { ShowErr(lblStockErr, "Valid stock required"); valid = false; }
            if (!valid) return;

            var med = new Medicine
            {
                MedicineID = _medicine?.MedicineID ?? 0,
                MedicineCode = txtCode.Text.Trim(),
                MedicineName = txtName.Text.Trim(),
                CategoryID   = (cmbCategory.SelectedItem as ComboItem)?.Id ?? 0,
                SupplierID   = (cmbSupplier.SelectedItem as ComboItem)?.Id ?? 0,
                Dosage       = txtDosage.Text.Trim(),
                Unit         = txtUnit.Text.Trim(),
                Price        = price,
                Stock        = stock,
                MinimumStock = int.TryParse(txtMinStock.Text, out int ms) ? ms : 10,
                ExpiryDate   = dtpExpiry.Value.Date,
                Description  = txtDesc.Text.Trim(),
                PrescriptionRequired = chkPrescription.Checked,
                Image        = _imagePath,
                DiscountID   = (cmbDiscount.SelectedItem as ComboItem)?.Id == 0 ? (int?)null : (cmbDiscount.SelectedItem as ComboItem)?.Id,
                Status       = cmbStatus.SelectedItem?.ToString() ?? "Active"
            };

            if (_medicine == null)
            {
                var (ok, msg, _) = _service.AddMedicine(med);
                MessageBox.Show(msg, ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (ok) DialogResult = DialogResult.OK;
            }
            else
            {
                var (ok, msg) = _service.UpdateMedicine(med);
                MessageBox.Show(msg, ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (ok) DialogResult = DialogResult.OK;
            }
        }

        private TextBox AddField(string label, int x, ref int y, int h, int gap)
        {
            var lbl = CreateLabel(label, x, y);
            y += 18;
            var pnl = new Panel { Location = new Point(x, y), Size = new Size(290, h), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            var txt = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 3), Width = 280, BorderStyle = BorderStyle.None };
            pnl.Controls.Add(txt);
            Controls.AddRange(new Control[] { lbl, pnl });
            y += gap;
            return txt;
        }

        private ComboBox AddCombo(string label, int x, ref int y, int gap)
        {
            var lbl = CreateLabel(label, x, y);
            y += 18;
            var cmb = new ComboBox { Location = new Point(x, y), Size = new Size(290, 28), BackColor = AppTheme.Surface, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontLabel, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            Controls.AddRange(new Control[] { lbl, cmb });
            y += gap;
            return cmb;
        }

        private Label AddErrLabel(int x, ref int y)
        {
            var lbl = new Label { Location = new Point(x, y - 12), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, Visible = false };
            Controls.Add(lbl);
            return lbl;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            var lbl = new Label { Text = text, Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(x, y) };
            Controls.Add(lbl);
            return lbl;
        }

        private void ShowErr(Label lbl, string msg) { lbl.Text = "⚠ " + msg; lbl.Visible = true; }

        private void ClearErrors()
        {
            if (lblCodeErr != null) lblCodeErr.Visible = false;
            if (lblNameErr != null) lblNameErr.Visible = false;
            if (lblPriceErr != null) lblPriceErr.Visible = false;
            if (lblStockErr != null) lblStockErr.Visible = false;
        }
    }
}
