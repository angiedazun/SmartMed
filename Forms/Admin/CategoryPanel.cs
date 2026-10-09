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
    public class CategoryPanel : UserControl
    {
        private readonly CategoryRepository _repo = new CategoryRepository();
        private DataGridView dgv;
        private TextBox txtSearch;
        private List<Category> _categories = new List<Category>();
        private Category _selected;

        public CategoryPanel()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.Transparent };
            txtSearch = new TextBox { Location = new Point(20, 12), Size = new Size(250, 32), BackColor = AppTheme.Surface, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (s, e) => LoadData();
            var btnAdd = AppTheme.CreateButton("+ Add Category", AppTheme.Primary, 140, 34);
            btnAdd.Location = new Point(700, 10);
            btnAdd.Click += (s, e) => OpenForm(null);
            pnlTop.Controls.AddRange(new Control[] { txtSearch, btnAdd });

            dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(dgv);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Index < _categories.Count)
                    _selected = _categories[dgv.SelectedRows[0].Index];
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
            _categories = _repo.GetAll(txtSearch.Text.Trim());
            var dt = new DataTable();
            dt.Columns.AddRange(new[] { new DataColumn("ID"), new DataColumn("Category Name"), new DataColumn("Description"), new DataColumn("Medicine Count") });
            foreach (var c in _categories)
                dt.Rows.Add(c.CategoryID, c.CategoryName, c.Description, c.MedicineCount);
            dgv.DataSource = dt;
        }

        private void OpenForm(Category c)
        {
            var form = new CategoryForm(c);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (_selected == null) return;
            if (_selected.MedicineCount > 0) { MessageBox.Show("Cannot delete category with medicines.", "Info"); return; }
            if (MessageBox.Show($"Delete '{_selected.CategoryName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _repo.Delete(_selected.CategoryID);
                LoadData();
            }
        }
    }

    public class CategoryForm : Form
    {
        private readonly CategoryRepository _repo = new CategoryRepository();
        private readonly Category _category;
        private TextBox txtName, txtDesc;
        private Label lblNameErr;

        public CategoryForm(Category cat)
        {
            _category = cat;
            Text = cat == null ? "Add Category" : "Edit Category";
            Size = new Size(440, 340);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Background;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Build();
            if (cat != null) { txtName.Text = cat.CategoryName; txtDesc.Text = cat.Description; }
        }

        private void Build()
        {
            Controls.Add(new Label { Text = Text, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(25, 20) });
            int y = 65;
            Controls.Add(new Label { Text = "Category Name *", Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(25, y) });
            y += 22;
            var pnlN = new Panel { Location = new Point(25, y), Size = new Size(380, 36), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            txtName = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontInput, Location = new Point(4, 5), Width = 370, BorderStyle = BorderStyle.None };
            pnlN.Controls.Add(txtName);
            Controls.Add(pnlN);
            y += 40;
            lblNameErr = new Label { Location = new Point(25, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, Visible = false };
            Controls.Add(lblNameErr);
            y += 16;
            Controls.Add(new Label { Text = "Description", Font = AppTheme.FontBold, ForeColor = AppTheme.TextMuted, AutoSize = true, Location = new Point(25, y) });
            y += 22;
            var pnlD = new Panel { Location = new Point(25, y), Size = new Size(380, 50), BackColor = AppTheme.InputBg, BorderStyle = BorderStyle.FixedSingle };
            txtDesc = new TextBox { BackColor = AppTheme.InputBg, ForeColor = AppTheme.TextPrimary, Font = AppTheme.FontLabel, Location = new Point(4, 4), Width = 370, Height = 42, BorderStyle = BorderStyle.None, Multiline = true };
            pnlD.Controls.Add(txtDesc);
            Controls.Add(pnlD);
            y += 60;
            var btnSave = AppTheme.CreateButton("💾 Save", AppTheme.Primary, 130, 38);
            var btnCnl  = AppTheme.CreateButton("Cancel",  AppTheme.Surface,  110, 38);
            btnSave.Location = new Point(25, y + 25); btnCnl.Location = new Point(165, y + 25);
            btnCnl.ForeColor = AppTheme.TextMuted;
            btnSave.Click += (s, e) =>
            {
                lblNameErr.Visible = false;
                if (string.IsNullOrWhiteSpace(txtName.Text)) { lblNameErr.Text = "⚠ Name required"; lblNameErr.Visible = true; return; }
                if (_repo.NameExists(txtName.Text.Trim(), _category?.CategoryID ?? 0)) { lblNameErr.Text = "⚠ Name already exists"; lblNameErr.Visible = true; return; }
                var c = new Category { CategoryID = _category?.CategoryID ?? 0, CategoryName = txtName.Text.Trim(), Description = txtDesc.Text.Trim() };
                bool ok = _category == null ? _repo.Create(c) > 0 : _repo.Update(c);
                MessageBox.Show(ok ? "Saved!" : "Failed.", ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (ok) DialogResult = DialogResult.OK;
            };
            btnCnl.Click += (s, e) => Close();
            Controls.AddRange(new Control[] { btnSave, btnCnl });
        }
    }
}


