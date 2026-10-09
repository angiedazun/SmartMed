using System.Data;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace SmartMed.Utilities
{
    public static class ExcelExporter
    {
        static ExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public static void Export(DataTable dt, string sheetName = "Report", string fileName = "Report")
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files|*.xlsx";
                sfd.FileName = fileName;
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add(sheetName);

                    // Header row style
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        ws.Cells[1, c + 1].Value = dt.Columns[c].ColumnName;
                        ws.Cells[1, c + 1].Style.Font.Bold = true;
                        ws.Cells[1, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(37, 99, 235));
                        ws.Cells[1, c + 1].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[1, c + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    // Data rows
                    for (int r = 0; r < dt.Rows.Count; r++)
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            ws.Cells[r + 2, c + 1].Value = dt.Rows[r][c];
                            if (r % 2 == 0)
                            {
                                ws.Cells[r + 2, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[r + 2, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(241, 245, 249));
                            }
                        }

                    ws.Cells.AutoFitColumns();

                    var border = ws.Cells[1, 1, dt.Rows.Count + 1, dt.Columns.Count].Style.Border;
                    border.Top.Style = ExcelBorderStyle.Thin;
                    border.Bottom.Style = ExcelBorderStyle.Thin;
                    border.Left.Style = ExcelBorderStyle.Thin;
                    border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(new FileInfo(sfd.FileName));
                }
                System.Diagnostics.Process.Start(sfd.FileName);
            }
        }
    }
}
