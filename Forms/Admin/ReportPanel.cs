using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmartMed.Repository;
using SmartMed.UI;
using SmartMed.Utilities;

namespace SmartMed.Forms.Admin
{
    public class ReportPanel : UserControl
    {
        private readonly OrderRepository    _orderRepo = new OrderRepository();
        private readonly MedicineRepository _medRepo   = new MedicineRepository();
        private readonly SupplierRepository _supRepo   = new SupplierRepository();
        private readonly CustomerRepository _custRepo  = new CustomerRepository();

        private DateTimePicker _dtpFrom, _dtpTo;
        private ComboBox       _cmbType;
        private Chart          _chart;
        private Panel          _chartCard;
        private DataGridView   _dgv;
        private Label          _lblSummary;

        public ReportPanel()
        {
            BackColor  = AppTheme.Background;
            Dock       = DockStyle.Fill;
            AutoScroll = false;
            Build();
        }

        // ─────────────────────────────────────────────────────────
        private void Build()
        {
            // ── Title bar ────────────────────────────────────────
            var pnlTitle = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = Color.White
            };
            pnlTitle.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, pnlTitle.Height - 1,
                                         pnlTitle.Width, pnlTitle.Height - 1);
            };
            pnlTitle.Controls.Add(new Label
            {
                Text      = "Reports & Analytics",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(20, 16),
                BackColor = Color.Transparent
            });

            // ── Filter toolbar ───────────────────────────────────
            var pnlFilter = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 62,
                BackColor = Color.White
            };
            pnlFilter.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawLine(pen, 0, pnlFilter.Height - 1,
                                         pnlFilter.Width, pnlFilter.Height - 1);
            };

            int x = 20, cy = 15;

            AddFilterLabel(pnlFilter, "From:", x, cy + 4);    x += 46;
            _dtpFrom = MakePicker(x, cy, DateTime.Today.AddDays(-30));
            pnlFilter.Controls.Add(_dtpFrom);                  x += 140;

            AddFilterLabel(pnlFilter, "To:", x, cy + 4);      x += 30;
            _dtpTo = MakePicker(x, cy, DateTime.Today);
            pnlFilter.Controls.Add(_dtpTo);                    x += 148;

            AddFilterLabel(pnlFilter, "Report Type:", x, cy + 4); x += 100;
            _cmbType = new ComboBox
            {
                Location      = new Point(x, cy),
                Width         = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = AppTheme.FontLabel,
                BackColor     = AppTheme.Surface,
                ForeColor     = AppTheme.TextPrimary
            };
            _cmbType.Items.AddRange(new object[]
                { "Sales Report", "Stock Report", "Top Medicines",
                  "Medicines Report", "Orders Report", "Suppliers Report", "Customers Report",
                  "Expired Medicines" });
            _cmbType.SelectedIndex = 0;
            pnlFilter.Controls.Add(_cmbType);
            x += 172;

            var btnGen = AppTheme.CreateButton("Generate", AppTheme.Primary, 110, 34);
            btnGen.Location = new Point(x, cy);
            btnGen.Click   += Generate_Click;
            pnlFilter.Controls.Add(btnGen);
            x += 118;

            var btnExcel = AppTheme.CreateButton("Excel", AppTheme.Success, 80, 34);
            btnExcel.Location = new Point(x, cy);
            btnExcel.Click   += (s, e) =>
            {
                if (_dgv.DataSource is DataTable dt && dt.Rows.Count > 0)
                    ExcelExporter.Export(dt,
                        _cmbType.SelectedItem.ToString(),
                        _cmbType.SelectedItem.ToString().Replace(" ", ""));
                else
                    MessageBox.Show("Generate a report first.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            pnlFilter.Controls.Add(btnExcel);
            x += 88;

            var btnPdf = AppTheme.CreateButton("PDF", AppTheme.Danger, 70, 34);
            btnPdf.Location = new Point(x, cy);
            btnPdf.Click   += (s, e) =>
            {
                if (_dgv.DataSource is DataTable dt && dt.Rows.Count > 0)
                {
                    if (_cmbType.SelectedIndex == 0)
                        PDFExporter.ExportSalesReport(dt, _dtpFrom.Value, _dtpTo.Value);
                    else
                        PDFExporter.ExportGenericReport(dt, _cmbType.SelectedItem.ToString(),
                            _dtpFrom.Value, _dtpTo.Value);
                }
                else
                    MessageBox.Show("Generate a report first.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            pnlFilter.Controls.Add(btnPdf);

            // ── Chart card ───────────────────────────────────────
            _chartCard = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 240,
                BackColor = Color.White,
                Padding   = new Padding(16, 10, 16, 10)
            };
            _chartCard.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    _chartCard.Width - 1, _chartCard.Height - 1);
            };

            try
            {
                _chart = new Chart
                {
                    Dock      = DockStyle.Fill,
                    BackColor = Color.White
                };

                var area = new ChartArea("main")
                {
                    BackColor  = Color.White,
                    BorderColor = Color.Transparent
                };
                area.AxisX.LabelStyle.ForeColor    = AppTheme.TextMuted;
                area.AxisX.LabelStyle.Font         = new Font("Segoe UI", 8f);
                area.AxisX.LineColor               = AppTheme.Border;
                area.AxisX.MajorGrid.LineColor     = Color.FromArgb(240, 242, 245);
                area.AxisX.LabelStyle.Angle        = -35;
                area.AxisX.Interval                = 1;

                area.AxisY.LabelStyle.ForeColor    = AppTheme.TextMuted;
                area.AxisY.LabelStyle.Font         = new Font("Segoe UI", 8f);
                area.AxisY.LineColor               = AppTheme.Border;
                area.AxisY.MajorGrid.LineColor     = Color.FromArgb(240, 242, 245);
                area.AxisY.LabelStyle.Format       = "#,##0";

                area.InnerPlotPosition             = new ElementPosition(8, 5, 88, 82);
                _chart.ChartAreas.Add(area);

                _chart.Legends.Add(new Legend("leg")
                {
                    Docking          = Docking.Top,
                    Alignment        = StringAlignment.Far,
                    BackColor        = Color.Transparent,
                    Font             = new Font("Segoe UI", 8.5f),
                    ForeColor        = AppTheme.TextMuted,
                    BorderColor      = Color.Transparent
                });

                _chartCard.Controls.Add(_chart);
            }
            catch { _chartCard.Visible = false; }

            // ── Summary label ────────────────────────────────────
            _lblSummary = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.FromArgb(248, 249, 250),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0),
                Text      = "Generate a report to see results."
            };

            // ── Data grid ────────────────────────────────────────
            _dgv = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.StyleDataGrid(_dgv);
            _dgv.RowPrePaint += DgvRowPrePaint;

            // ── Compose (reverse order for DockStyle.Top) ────────
            Controls.Add(_dgv);
            Controls.Add(_lblSummary);
            Controls.Add(_chartCard);
            Controls.Add(pnlFilter);
            Controls.Add(pnlTitle);

            Generate_Click(null, null);
        }

        // ─────────────────────────────────────────────────────────
        private void Generate_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                DataTable dt;
                switch (_cmbType.SelectedIndex)
                {
                    case 0: // Sales Report
                        dt = _orderRepo.GetSalesReport(_dtpFrom.Value, _dtpTo.Value);
                        _dgv.DataSource = dt;
                        StyleSalesGrid();
                        BuildBarChart(dt, "Date",         "NetRevenue",
                                      "Net Revenue (Rs.)", AppTheme.Primary);
                        ShowChart(true);
                        SetSummary(dt, "Sales");
                        break;

                    case 1: // Stock Report
                        dt = BuildStockReport();
                        _dgv.DataSource = dt;
                        StyleStockGrid();
                        ShowChart(false);
                        SetSummary(dt, "Stock");
                        break;

                    case 2: // Top Medicines
                        dt = _medRepo.GetTopSelling(20);
                        _dgv.DataSource = dt;
                        StyleTopGrid();
                        BuildBarChart(dt, "MedicineName", "TotalSold",
                                      "Units Sold", AppTheme.Accent);
                        ShowChart(true);
                        SetSummary(dt, "Top");
                        break;

                    case 3: // Medicines Report
                        dt = BuildMedicinesReport();
                        _dgv.DataSource = dt;
                        StyleMedicinesGrid();
                        ShowChart(false);
                        SetSummary(dt, "Medicines");
                        break;

                    case 4: // Orders Report
                        dt = BuildOrdersReport(_dtpFrom.Value, _dtpTo.Value);
                        _dgv.DataSource = dt;
                        StyleOrdersGrid();
                        BuildBarChart(dt, "Date", "FinalAmount",
                                      "Order Value (Rs.)", AppTheme.Primary);
                        ShowChart(true);
                        SetSummary(dt, "Orders");
                        break;

                    case 5: // Suppliers Report
                        dt = BuildSuppliersReport();
                        _dgv.DataSource = dt;
                        StyleSuppliersGrid();
                        ShowChart(false);
                        SetSummary(dt, "Suppliers");
                        break;

                    case 6: // Customers Report
                        dt = BuildCustomersReport();
                        _dgv.DataSource = dt;
                        StyleCustomersGrid();
                        ShowChart(false);
                        SetSummary(dt, "Customers");
                        break;

                    case 7: // Expired Medicines
                        dt = _medRepo.GetExpired(1000);
                        _dgv.DataSource = dt;
                        StyleExpiredGrid();
                        ShowChart(false);
                        SetSummary(dt, "Expired");
                        break;
                }
            }
            finally { Cursor = Cursors.Default; }
        }

        // ─────────────────────────────────────────────────────────
        // Grid styling helpers
        // ─────────────────────────────────────────────────────────
        private void StyleSalesGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "Date":
                        col.HeaderText = "Date";
                        col.Width = 110;
                        col.DefaultCellStyle.Format = "dd/MM/yyyy";
                        break;
                    case "TotalOrders":
                        col.HeaderText = "Orders";
                        col.Width = 80;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "TotalSales":
                        col.HeaderText = "Gross Sales";
                        col.Width = 120;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "TotalDiscount":
                        col.HeaderText = "Discounts";
                        col.Width = 110;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.DefaultCellStyle.ForeColor = AppTheme.Warning;
                        break;
                    case "NetRevenue":
                        col.HeaderText = "Net Revenue";
                        col.Width = 120;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.DefaultCellStyle.ForeColor = AppTheme.Success;
                        col.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                }
            }
        }

        private void StyleStockGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            _dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
                string colName = _dgv.Columns[e.ColumnIndex].Name;
                if (colName == "Status" && e.Value != null)
                {
                    switch (e.Value.ToString())
                    {
                        case "Low Stock":
                            e.CellStyle.ForeColor = AppTheme.Warning;
                            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                            break;
                        case "Expired":
                            e.CellStyle.ForeColor = AppTheme.Danger;
                            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                            break;
                        case "OK":
                            e.CellStyle.ForeColor = AppTheme.Success;
                            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                            break;
                    }
                }
                if (colName == "Stock" || colName == "Min Stock")
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            };

            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                if (col.Name == "Code")    col.Width = 90;
                if (col.Name == "Medicine") col.Width = 180;
                if (col.Name == "Category") col.Width = 120;
                if (col.Name == "Stock" || col.Name == "Min Stock") col.Width = 85;
                if (col.Name == "Expiry")  col.Width = 100;
                if (col.Name == "Status")  col.Width = 90;
            }
        }

        private void StyleTopGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "MedicineName":
                        col.HeaderText = "Medicine Name";
                        col.Width = 220;
                        break;
                    case "TotalSold":
                        col.HeaderText = "Units Sold";
                        col.Width = 110;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        col.DefaultCellStyle.ForeColor = AppTheme.Primary;
                        break;
                    case "TotalRevenue":
                        col.HeaderText = "Total Revenue";
                        col.Width = 130;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.DefaultCellStyle.ForeColor = AppTheme.Success;
                        col.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                }
            }

            // Add rank column
            if (!_dgv.Columns.Contains("Rank"))
            {
                var rankCol = new DataGridViewTextBoxColumn
                {
                    Name             = "Rank",
                    HeaderText       = "#",
                    Width            = 44,
                    DisplayIndex     = 0,
                    ReadOnly         = true,
                    SortMode         = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter,
                                         Font = new Font("Segoe UI", 9f, FontStyle.Bold) }
                };
                _dgv.Columns.Insert(0, rankCol);
                for (int i = 0; i < _dgv.Rows.Count; i++)
                    _dgv.Rows[i].Cells["Rank"].Value = i + 1;
            }
        }

        private void StyleMedicinesGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "Code":     col.HeaderText = "Code";     col.Width = 90;  break;
                    case "Medicine": col.HeaderText = "Medicine"; col.Width = 180; break;
                    case "Category": col.HeaderText = "Category"; col.Width = 120; break;
                    case "Supplier": col.HeaderText = "Supplier"; col.Width = 140; break;
                    case "Price":
                        col.HeaderText = "Price";
                        col.Width = 90;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "Stock":
                        col.HeaderText = "Stock";
                        col.Width = 80;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "Expiry":   col.HeaderText = "Expiry";   col.Width = 100; break;
                    case "Status":   col.HeaderText = "Status";   col.Width = 90;  break;
                }
            }
        }

        private void StyleOrdersGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "OrderID":  col.HeaderText = "Order #"; col.Width = 80; break;
                    case "Date":
                        col.HeaderText = "Date";
                        col.Width = 100;
                        col.DefaultCellStyle.Format = "dd/MM/yyyy";
                        break;
                    case "Customer": col.HeaderText = "Customer"; col.Width = 160; break;
                    case "Items":
                        col.HeaderText = "Items";
                        col.Width = 70;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "Total":
                        col.HeaderText = "Total";
                        col.Width = 100;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "Discount":
                        col.HeaderText = "Discount";
                        col.Width = 100;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.DefaultCellStyle.ForeColor = AppTheme.Warning;
                        break;
                    case "FinalAmount":
                        col.HeaderText = "Final Amount";
                        col.Width = 110;
                        col.DefaultCellStyle.Format = "Rs. #,##0.00";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        col.DefaultCellStyle.ForeColor = AppTheme.Success;
                        col.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                    case "Status":   col.HeaderText = "Status"; col.Width = 90; break;
                }
            }
        }

        private void StyleSuppliersGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "Name":    col.HeaderText = "Supplier Name"; col.Width = 160; break;
                    case "Company": col.HeaderText = "Company";       col.Width = 160; break;
                    case "Phone":   col.HeaderText = "Phone";         col.Width = 110; break;
                    case "Email":   col.HeaderText = "Email";         col.Width = 180; break;
                    case "Address": col.HeaderText = "Address";       col.Width = 200; break;
                    case "Since":   col.HeaderText = "Since";         col.Width = 100; break;
                }
            }
        }

        private void StyleCustomersGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "Name":     col.HeaderText = "Customer Name"; col.Width = 170; break;
                    case "Email":    col.HeaderText = "Email";         col.Width = 190; break;
                    case "Phone":    col.HeaderText = "Phone";         col.Width = 110; break;
                    case "Address":  col.HeaderText = "Address";       col.Width = 200; break;
                    case "Status":   col.HeaderText = "Status";        col.Width = 90;  break;
                    case "Registered": col.HeaderText = "Registered";  col.Width = 100; break;
                }
            }
        }

        private void StyleExpiredGrid()
        {
            if (_dgv.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in _dgv.Columns)
            {
                switch (col.Name)
                {
                    case "MedicineCode": col.HeaderText = "Code";     col.Width = 90;  break;
                    case "MedicineName": col.HeaderText = "Medicine"; col.Width = 200; break;
                    case "CategoryName": col.HeaderText = "Category"; col.Width = 130; break;
                    case "Stock":
                        col.HeaderText = "Stock";
                        col.Width = 80;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "ExpiryDate":
                        col.HeaderText = "Expired On";
                        col.Width = 120;
                        col.DefaultCellStyle.Format = "dd/MM/yyyy";
                        col.DefaultCellStyle.ForeColor = AppTheme.Danger;
                        col.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // Row color coding
        // ─────────────────────────────────────────────────────────
        private void DgvRowPrePaint(object sender,
            DataGridViewRowPrePaintEventArgs e) { }

        // ─────────────────────────────────────────────────────────
        // Chart helpers
        // ─────────────────────────────────────────────────────────
        private void ShowChart(bool visible) => _chartCard.Visible = visible;

        private void BuildBarChart(DataTable dt,
            string xCol, string yCol, string seriesName, Color color)
        {
            if (_chart == null || !dt.Columns.Contains(xCol) ||
                !dt.Columns.Contains(yCol)) return;
            try
            {
                _chart.Series.Clear();
                _chart.Titles.Clear();

                _chart.Titles.Add(new Title
                {
                    Text      = $"{_cmbType.SelectedItem}  —  " +
                                $"{_dtpFrom.Value:dd MMM yyyy} to {_dtpTo.Value:dd MMM yyyy}",
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = AppTheme.TextPrimary,
                    Docking   = Docking.Top,
                    Alignment = ContentAlignment.MiddleLeft
                });

                var series = new Series(seriesName)
                {
                    ChartType    = SeriesChartType.Column,
                    Color        = color,
                    BorderColor  = Color.FromArgb(200, color),
                    BorderWidth  = 1,
                    IsValueShownAsLabel = dt.Rows.Count <= 10,
                    Font         = new Font("Segoe UI", 7.5f),
                    LabelForeColor = AppTheme.TextMuted
                };

                foreach (DataRow row in dt.Rows)
                {
                    string xLabel = row[xCol] is DateTime d
                        ? d.ToString("dd/MM")
                        : row[xCol].ToString();
                    if (xLabel.Length > 14) xLabel = xLabel.Substring(0, 12) + "…";
                    double yVal = 0;
                    double.TryParse(row[yCol].ToString(), out yVal);
                    series.Points.AddXY(xLabel, yVal);
                }

                _chart.Series.Add(series);

                // Y-axis format
                var area = _chart.ChartAreas["main"];
                area.AxisY.LabelStyle.Format = yCol.ToLower().Contains("revenue")
                    ? "Rs.#,##0" : "#,##0";

                // Recalculate interval
                area.AxisX.Interval = dt.Rows.Count > 20 ? 2 : 1;
            }
            catch { }
        }

        // ─────────────────────────────────────────────────────────
        // Summary bar
        // ─────────────────────────────────────────────────────────
        private void SetSummary(DataTable dt, string mode)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                _lblSummary.Text = "  No data found for the selected period.";
                return;
            }

            switch (mode)
            {
                case "Sales":
                    decimal net = 0, gross = 0;
                    int orders = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        net    += Convert.ToDecimal(r["NetRevenue"]);
                        gross  += Convert.ToDecimal(r["TotalSales"]);
                        orders += Convert.ToInt32(r["TotalOrders"]);
                    }
                    _lblSummary.Text =
                        $"  Total Records: {dt.Rows.Count}   |   " +
                        $"Total Orders: {orders}   |   " +
                        $"Gross Sales: Rs. {gross:N2}   |   " +
                        $"Net Revenue: Rs. {net:N2}";
                    break;

                case "Stock":
                    int ok = 0, low = 0, exp = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        switch (r["Status"].ToString())
                        {
                            case "OK":       ok++;  break;
                            case "Low Stock": low++; break;
                            case "Expired":  exp++; break;
                        }
                    }
                    _lblSummary.Text =
                        $"  Total Medicines: {dt.Rows.Count}   |   " +
                        $"OK: {ok}   |   " +
                        $"Low Stock: {low}   |   " +
                        $"Expired: {exp}";
                    break;

                case "Top":
                    decimal rev = 0; int sold = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        sold += Convert.ToInt32(r["TotalSold"]);
                        rev  += Convert.ToDecimal(r["TotalRevenue"]);
                    }
                    _lblSummary.Text =
                        $"  Top {dt.Rows.Count} Medicines   |   " +
                        $"Total Units Sold: {sold:N0}   |   " +
                        $"Total Revenue: Rs. {rev:N2}";
                    break;

                case "Medicines":
                    int lowStock = 0, expired = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r["Status"].ToString() == "Low Stock") lowStock++;
                        if (r["Status"].ToString() == "Expired") expired++;
                    }
                    _lblSummary.Text =
                        $"  Total Medicines: {dt.Rows.Count}   |   " +
                        $"Low Stock: {lowStock}   |   " +
                        $"Expired: {expired}";
                    break;

                case "Orders":
                    decimal ordersTotal = 0, ordersFinal = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        ordersTotal += Convert.ToDecimal(r["Total"]);
                        ordersFinal += Convert.ToDecimal(r["FinalAmount"]);
                    }
                    _lblSummary.Text =
                        $"  Total Orders: {dt.Rows.Count}   |   " +
                        $"Gross Total: Rs. {ordersTotal:N2}   |   " +
                        $"Final Amount: Rs. {ordersFinal:N2}";
                    break;

                case "Suppliers":
                    _lblSummary.Text = $"  Total Active Suppliers: {dt.Rows.Count}";
                    break;

                case "Customers":
                    int active = 0;
                    foreach (DataRow r in dt.Rows)
                        if (r["Status"].ToString() == "Active") active++;
                    _lblSummary.Text =
                        $"  Total Customers: {dt.Rows.Count}   |   " +
                        $"Active: {active}";
                    break;

                case "Expired":
                    int totalExpiredStock = 0;
                    foreach (DataRow r in dt.Rows)
                        totalExpiredStock += Convert.ToInt32(r["Stock"]);
                    _lblSummary.Text =
                        $"  Total Expired Medicines: {dt.Rows.Count}   |   " +
                        $"Units in Expired Stock: {totalExpiredStock:N0}";
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────
        // Stock report builder
        // ─────────────────────────────────────────────────────────
        private DataTable BuildStockReport()
        {
            var medicines = _medRepo.GetAll();
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Code"),
                new DataColumn("Medicine"),
                new DataColumn("Category"),
                new DataColumn("Stock",    typeof(int)),
                new DataColumn("Min Stock",typeof(int)),
                new DataColumn("Expiry"),
                new DataColumn("Status")
            });
            foreach (var m in medicines)
                dt.Rows.Add(
                    m.MedicineCode,
                    m.MedicineName,
                    m.CategoryName,
                    m.Stock,
                    m.MinimumStock,
                    m.ExpiryDate.ToString("dd/MM/yyyy"),
                    m.IsExpired  ? "Expired"   :
                    m.IsLowStock ? "Low Stock" : "OK");
            return dt;
        }

        // ─────────────────────────────────────────────────────────
        // Medicines report builder
        // ─────────────────────────────────────────────────────────
        private DataTable BuildMedicinesReport()
        {
            var medicines = _medRepo.GetAll();
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Code"),
                new DataColumn("Medicine"),
                new DataColumn("Category"),
                new DataColumn("Supplier"),
                new DataColumn("Price",  typeof(decimal)),
                new DataColumn("Stock",  typeof(int)),
                new DataColumn("Expiry"),
                new DataColumn("Status")
            });
            foreach (var m in medicines)
                dt.Rows.Add(
                    m.MedicineCode,
                    m.MedicineName,
                    m.CategoryName,
                    m.SupplierName,
                    m.Price,
                    m.Stock,
                    m.ExpiryDate.ToString("dd/MM/yyyy"),
                    m.IsExpired  ? "Expired"   :
                    m.IsLowStock ? "Low Stock" : "OK");
            return dt;
        }

        // ─────────────────────────────────────────────────────────
        // Orders report builder
        // ─────────────────────────────────────────────────────────
        private DataTable BuildOrdersReport(DateTime from, DateTime to)
        {
            var orders = _orderRepo.GetAll()
                .Where(o => o.OrderDate.Date >= from.Date && o.OrderDate.Date <= to.Date)
                .OrderByDescending(o => o.OrderDate);

            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("OrderID",     typeof(int)),
                new DataColumn("Date",        typeof(DateTime)),
                new DataColumn("Customer"),
                new DataColumn("Items",       typeof(int)),
                new DataColumn("Total",       typeof(decimal)),
                new DataColumn("Discount",    typeof(decimal)),
                new DataColumn("FinalAmount", typeof(decimal)),
                new DataColumn("Status")
            });
            foreach (var o in orders)
                dt.Rows.Add(
                    o.OrderID,
                    o.OrderDate,
                    o.CustomerName,
                    o.ItemCount,
                    o.Total,
                    o.Discount,
                    o.FinalAmount,
                    o.Status);
            return dt;
        }

        // ─────────────────────────────────────────────────────────
        // Suppliers report builder
        // ─────────────────────────────────────────────────────────
        private DataTable BuildSuppliersReport()
        {
            var suppliers = _supRepo.GetAll();
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Name"),
                new DataColumn("Company"),
                new DataColumn("Phone"),
                new DataColumn("Email"),
                new DataColumn("Address"),
                new DataColumn("Since")
            });
            foreach (var s in suppliers)
                dt.Rows.Add(
                    s.SupplierName,
                    s.Company,
                    s.Phone,
                    s.Email,
                    s.Address,
                    s.CreatedDate.ToString("dd/MM/yyyy"));
            return dt;
        }

        // ─────────────────────────────────────────────────────────
        // Customers report builder
        // ─────────────────────────────────────────────────────────
        private DataTable BuildCustomersReport()
        {
            var customers = _custRepo.GetAll();
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Name"),
                new DataColumn("Email"),
                new DataColumn("Phone"),
                new DataColumn("Address"),
                new DataColumn("Registered"),
                new DataColumn("Status")
            });
            foreach (var c in customers)
                dt.Rows.Add(
                    $"{c.FirstName} {c.LastName}",
                    c.Email,
                    c.Phone,
                    c.Address,
                    c.RegisteredDate.ToString("dd/MM/yyyy"),
                    c.Status);
            return dt;
        }

        // ─────────────────────────────────────────────────────────
        // Mini helpers
        // ─────────────────────────────────────────────────────────
        private static DateTimePicker MakePicker(int x, int y, DateTime val) =>
            new DateTimePicker
            {
                Location = new Point(x, y),
                Width    = 130,
                Format   = DateTimePickerFormat.Short,
                Value    = val
            };

        private static void AddFilterLabel(Panel p, string text, int x, int y) =>
            p.Controls.Add(new Label
            {
                Text      = text,
                Font      = AppTheme.FontBold,
                ForeColor = AppTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(x, y),
                BackColor = Color.Transparent
            });
    }
}
