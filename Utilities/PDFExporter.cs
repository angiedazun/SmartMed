using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using SmartMed.Models;

namespace SmartMed.Utilities
{
    public static class PDFExporter
    {
        private static readonly BaseColor HeaderColor = new BaseColor(37, 99, 235);
        private static readonly BaseColor LightBlue = new BaseColor(219, 234, 254);
        private static readonly BaseColor Dark = new BaseColor(15, 23, 42);

        public static void ExportInvoice(Order order)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Files|*.pdf";
                sfd.FileName = $"Invoice_Order{order.OrderID:D5}";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var doc = new Document(PageSize.A4, 40, 40, 60, 40))
                using (var writer = PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create)))
                {
                    doc.Open();
                    AddInvoiceContent(doc, order);
                    doc.Close();
                }
                System.Diagnostics.Process.Start(sfd.FileName);
            }
        }

        private static void AddInvoiceContent(Document doc, Order order)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, new BaseColor(37, 99, 235));
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.WHITE);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, Dark);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, Dark);

            // Header
            doc.Add(new Paragraph("SmartMed Pharmacy", titleFont) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph("Tax Invoice", FontFactory.GetFont(FontFactory.HELVETICA, 12, Dark)) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new LineSeparator(1f, 100f, HeaderColor, Element.ALIGN_CENTER, -5));
            doc.Add(Chunk.NEWLINE);

            // Order Info
            var info = new PdfPTable(2) { WidthPercentage = 100 };
            info.SetWidths(new float[] { 1, 1 });
            info.AddCell(CreateCell($"Order #: {order.OrderID:D5}", normalFont, false));
            info.AddCell(CreateCell($"Date: {order.OrderDate:dd/MM/yyyy HH:mm}", normalFont, false, Element.ALIGN_RIGHT));
            info.AddCell(CreateCell($"Customer: {order.CustomerName}", normalFont, false));
            info.AddCell(CreateCell($"Status: {order.Status}", boldFont, false, Element.ALIGN_RIGHT));
            info.AddCell(CreateCell($"Payment: {order.PaymentMethod}", normalFont, false));
            info.AddCell(CreateCell(order.PickupDate.HasValue ? $"Pickup: {order.PickupDate:dd/MM/yyyy}" : "", normalFont, false, Element.ALIGN_RIGHT));
            doc.Add(info);
            doc.Add(Chunk.NEWLINE);

            // Items table
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 3, 1, 2, 2, 2 });
            foreach (string header in new[] { "Medicine", "Qty", "Unit Price", "Subtotal", "Code" })
                table.AddCell(CreateCell(header, headerFont, true, Element.ALIGN_CENTER, HeaderColor));

            foreach (var item in order.Items)
            {
                table.AddCell(CreateCell(item.MedicineName, normalFont, false));
                table.AddCell(CreateCell(item.Quantity.ToString(), normalFont, false, Element.ALIGN_CENTER));
                table.AddCell(CreateCell($"Rs. {item.Price:F2}", normalFont, false, Element.ALIGN_RIGHT));
                table.AddCell(CreateCell($"Rs. {item.Subtotal:F2}", normalFont, false, Element.ALIGN_RIGHT));
                table.AddCell(CreateCell(item.MedicineCode, normalFont, false, Element.ALIGN_CENTER));
            }
            doc.Add(table);
            doc.Add(Chunk.NEWLINE);

            // Totals
            var totals = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_RIGHT };
            totals.AddCell(CreateCell("Subtotal:", normalFont, false));
            totals.AddCell(CreateCell($"Rs. {order.Total:F2}", normalFont, false, Element.ALIGN_RIGHT));
            totals.AddCell(CreateCell("Discount:", normalFont, false));
            totals.AddCell(CreateCell($"Rs. {order.Discount:F2}", normalFont, false, Element.ALIGN_RIGHT));
            totals.AddCell(CreateCell("TOTAL:", boldFont, false));
            totals.AddCell(CreateCell($"Rs. {order.FinalAmount:F2}", boldFont, false, Element.ALIGN_RIGHT));
            doc.Add(totals);

            doc.Add(Chunk.NEWLINE);
            doc.Add(new Paragraph("Thank you for choosing SmartMed Pharmacy!",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, HeaderColor))
            { Alignment = Element.ALIGN_CENTER });
        }

        private static PdfPCell CreateCell(string text, Font font, bool isHeader,
            int align = Element.ALIGN_LEFT, BaseColor bg = null)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = align,
                Padding = 8,
                BackgroundColor = bg ?? (isHeader ? HeaderColor : BaseColor.WHITE),
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = new BaseColor(226, 232, 240)
            };
            return cell;
        }

        // ── User Guide ────────────────────────────────────────────
        public static void ExportUserGuide()
        {
            using var sfd = new SaveFileDialog
            {
                Filter   = "PDF Files|*.pdf",
                FileName = "SmartMed_UserGuide"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            using var doc = new Document(PageSize.A4, 50, 50, 60, 50);
            using var writer = PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
            doc.Open();
            BuildUserGuide(doc, writer);
            doc.Close();
            System.Diagnostics.Process.Start(sfd.FileName);
        }

        private static void BuildUserGuide(Document doc, PdfWriter writer)
        {
            // ── Fonts ──────────────────────────────────────────────
            var fCover    = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  28, BaseColor.WHITE);
            var fCoverSub = FontFactory.GetFont(FontFactory.HELVETICA,       13, new BaseColor(219,234,254));
            var fH1       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  16, new BaseColor(37,99,235));
            var fH2       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  12, new BaseColor(15,23,42));
            var fBody     = FontFactory.GetFont(FontFactory.HELVETICA,       10, new BaseColor(30,41,59));
            var fBold     = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  10, new BaseColor(15,23,42));
            var fStep     = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  10, BaseColor.WHITE);
            var fNote     = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, new BaseColor(100,116,139));
            var fTip      = FontFactory.GetFont(FontFactory.HELVETICA,        9, new BaseColor(21,128,61));
            var fWarn     = FontFactory.GetFont(FontFactory.HELVETICA,        9, new BaseColor(180,35,35));
            var fTh       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  9, BaseColor.WHITE);
            var fTd       = FontFactory.GetFont(FontFactory.HELVETICA,        9, new BaseColor(30,41,59));

            var Blue   = new BaseColor(37,  99,  235);
            var Green  = new BaseColor(22, 163,  74);
            var Orange = new BaseColor(234,179,   8);
            var Red    = new BaseColor(220,  38,  38);
            var Teal   = new BaseColor( 13,148, 136);
            var Light  = new BaseColor(248,250,252);
            var Border = new BaseColor(226,232,240);

            // ══════════════════════════════════════════════════════
            // COVER PAGE
            // ══════════════════════════════════════════════════════
            var cover = new PdfPTable(1) { WidthPercentage = 100 };
            var coverCell = new PdfPCell
            {
                BackgroundColor    = Blue,
                Border             = Rectangle.NO_BORDER,
                Padding            = 60,
                VerticalAlignment  = Element.ALIGN_MIDDLE,
                HorizontalAlignment= Element.ALIGN_CENTER,
                MinimumHeight      = 500
            };
            var coverContent = new Phrase();
            coverContent.Add(new Chunk("\n\n\n", fCover));
            coverContent.Add(new Chunk("SmartMed Pharmacy\n", fCover));
            coverContent.Add(new Chunk("Management System\n\n", fCoverSub));
            coverContent.Add(new Chunk("Complete User Guide\n", fCoverSub));
            coverContent.Add(new Chunk($"Version 1.0  |  {DateTime.Now:MMMM yyyy}", fCoverSub));
            coverCell.AddElement(new Paragraph(coverContent) { Alignment = Element.ALIGN_CENTER });
            cover.AddCell(coverCell);
            doc.Add(cover);
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // TABLE OF CONTENTS
            // ══════════════════════════════════════════════════════
            AddH1(doc, fH1, "Table of Contents");
            AddLine(doc, Blue);
            string[] toc =
            {
                "1.  Getting Started — Login & Registration",
                "2.  Admin Guide — Dashboard",
                "3.  Admin Guide — Medicine Management",
                "4.  Admin Guide — Order Management",
                "5.  Admin Guide — Customer Management",
                "6.  Admin Guide — Supplier Management",
                "7.  Admin Guide — Category Management",
                "8.  Admin Guide — Discount Management",
                "9.  Admin Guide — Reports",
                "10. Admin Guide — Settings & Profile",
                "11. Customer Guide — Dashboard",
                "12. Customer Guide — Search & Buy Medicines",
                "13. Customer Guide — Shopping Cart & Checkout",
                "14. Customer Guide — My Orders",
                "15. Customer Guide — Profile Management",
                "16. Supplier Management Reference",
                "17. PDF & Excel Export Guide",
                "18. Troubleshooting & Tips"
            };
            foreach (var line in toc)
                doc.Add(new Paragraph("    " + line, fBody) { SpacingBefore = 4 });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 1 — LOGIN & REGISTRATION
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 1", "Getting Started — Login & Registration", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "1.1  Admin Login");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Launch SmartMed.exe — the Login window opens automatically.",
                "Click the  ADMIN  tab at the top of the login form.",
                "Enter your Username (default: admin) and Password (default: Admin@123).",
                "Click  Login  or press Enter.",
                "The Admin Dashboard opens. The top bar shows your name and live clock."
            });
            AddTip(doc, fTip, "Note: After 5 failed login attempts the account is locked for security.");

            AddH2(doc, fH2, "1.2  Customer Login");
            AddSteps(doc, fStep, fBody, Green, new[]
            {
                "On the Login window, click the  CUSTOMER  tab.",
                "Enter your registered Email and Password.",
                "Tick  Remember Me  to save credentials for next time.",
                "Click  Login — the Customer Dashboard opens."
            });

            AddH2(doc, fH2, "1.3  Customer Registration (New Account)");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "On the Login window (Customer tab) click  Register.",
                "Fill in: First Name, Last Name, Email, Phone, Address.",
                "Enter a strong Password — the strength meter must show Fair or above.",
                "Confirm your password in the Confirm Password field.",
                "Click  Create Account. A success message appears.",
                "Return to the Login window and sign in with your new credentials."
            });
            AddTip(doc, fTip, "Password must be 8+ characters with uppercase, lowercase, number, and special character (e.g. Admin@123).");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 2 — ADMIN DASHBOARD
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 2", "Admin Guide — Dashboard", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "2.1  Dashboard Overview");
            AddBody(doc, fBody, "The Dashboard is the first screen after Admin login. It shows a real-time summary of your pharmacy.");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Card", "What it Shows" },
                new[,]
                {
                    { "Today's Sales",     "Total revenue collected today (Rs.)" },
                    { "Monthly Revenue",   "Total revenue for the current month (Rs.)" },
                    { "Total Customers",   "Number of registered customer accounts" },
                    { "Total Medicines",   "Total active medicines in the system" },
                    { "Pending Orders",    "Orders waiting to be processed" },
                    { "Delivered Orders",  "Orders successfully delivered this period" },
                    { "Low Stock Items",   "Medicines below their minimum stock level" },
                    { "Expired Medicines", "Medicines past their expiry date" }
                });

            AddH2(doc, fH2, "2.2  Top Selling Medicines & Recent Orders");
            AddBody(doc, fBody, "Below the stat cards are two panels:\n• Top Selling Medicines — the 8 best-selling medicines by quantity.\n• Recent Orders — the 10 most recent customer orders with colour-coded status.\nClick  View All →  in either panel header to go to the full list.");

            AddH2(doc, fH2, "2.3  Quick Actions");
            AddBody(doc, fBody, "Four coloured buttons at the bottom navigate directly to:\nAdd Medicine  |  View Orders  |  Customers  |  View Reports");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 3 — MEDICINE MANAGEMENT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 3", "Admin Guide — Medicine Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "3.1  Adding a New Medicine");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Medicines  in the left sidebar.",
                "Click the  + Add Medicine  button (top-right).",
                "Fill in Medicine Code (or click  Auto  to generate) and Medicine Name.",
                "Select Category and Supplier from the dropdowns.",
                "Enter Price (Rs.), Stock Quantity, and Minimum Stock.",
                "Set the Expiry Date — must be a future date.",
                "Optionally fill Dosage, Unit, Description, Discount, and Status.",
                "Tick  Prescription Required  if the medicine needs a prescription.",
                "Click  Upload  to attach a medicine image (optional).",
                "Click  Save. The medicine appears in the list immediately."
            });

            AddH2(doc, fH2, "3.2  Editing a Medicine");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "Click on the medicine row in the grid to select it.",
                "Click  Edit  in the bottom action bar.",
                "Modify the required fields in the Edit Medicine form.",
                "Click  Save."
            });

            AddH2(doc, fH2, "3.3  Searching & Filtering");
            AddBody(doc, fBody, "• Type in the Search box to filter by name or code in real time.\n• Use the Category dropdown to show only one category.\n• Use the Status dropdown to show Active or Inactive medicines.");

            AddH2(doc, fH2, "3.4  Exporting Medicine Data");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Button", "Output" },
                new[,]
                {
                    { "Export Excel", "Saves a styled .xlsx file of the current medicine list" },
                    { "Export PDF",   "Saves a PDF sales report for today" }
                });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 4 — ORDER MANAGEMENT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 4", "Admin Guide — Order Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "4.1  Viewing Orders");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Orders  in the left sidebar.",
                "All customer orders are listed with Order ID, Customer, Date, Amount, Status.",
                "Use the search box to find orders by customer name.",
                "Use the Status dropdown to filter: Pending / Approved / Ready / Delivered / Cancelled."
            });

            AddH2(doc, fH2, "4.2  Order Status Workflow");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Status", "Meaning", "Next Action" },
                new[,]
                {
                    { "Pending",   "Customer placed order, awaiting admin review", "Approve or Reject" },
                    { "Approved",  "Admin confirmed — being prepared",             "Mark as Ready"     },
                    { "Ready",     "Medicine packed and ready for pickup",          "Mark as Delivered" },
                    { "Delivered", "Customer received the order",                  "Final state"       },
                    { "Cancelled", "Cancelled by customer (Pending only)",         "Final state"       },
                    { "Rejected",  "Rejected by admin",                            "Final state"       }
                });

            AddH2(doc, fH2, "4.3  Updating Order Status");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "Click the order row to select it.",
                "Click  Update Status  in the bottom bar.",
                "Select the new status from the dropdown.",
                "Click  Update — stock is adjusted automatically for Delivered orders."
            });

            AddH2(doc, fH2, "4.4  Generating an Invoice");
            AddSteps(doc, fStep, fBody, Green, new[]
            {
                "Select an order in the grid.",
                "Click  Invoice  in the bottom action bar.",
                "A Save File dialog appears — choose the save location.",
                "The PDF invoice opens automatically (includes itemised list, totals, discount)."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 5 — CUSTOMER MANAGEMENT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 5", "Admin Guide — Customer Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "5.1  Viewing Customers");
            AddBody(doc, fBody, "Click  Customers  in the sidebar. The grid shows all registered customers with ID, Name, Email, Phone, Status, and Registration Date. Type in the search box to filter in real time.");

            AddH2(doc, fH2, "5.2  Adding a Customer (Admin-created)");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  + Add Customer  (top-right).",
                "Fill in First Name, Last Name, Email, Phone, Address.",
                "Click  Save. A default password (Customer@123) is assigned.",
                "Tell the customer to log in and change their password immediately."
            });

            AddH2(doc, fH2, "5.3  Enabling / Disabling an Account");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "Select the customer row.",
                "Click  Enable  to restore access, or  Disable  to block login."
            });
            AddWarn(doc, fWarn, "A disabled customer cannot log in until an admin re-enables their account.");

            AddH2(doc, fH2, "5.4  Viewing a Customer's Orders");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "Select the customer row.",
                "Click  Orders  in the bottom bar.",
                "A popup shows all orders for that customer with summary stats (Total Orders, Total Spent, Pending, Delivered).",
                "Orders are colour-coded by status."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 6 — SUPPLIER MANAGEMENT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 6", "Admin Guide — Supplier Management", Blue, fStep, fCoverSub);
            AddBody(doc, fBody, "Suppliers are the companies or individuals who provide medicines. Each medicine must be linked to a supplier.");
            AddH2(doc, fH2, "6.1  Adding a Supplier");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Suppliers  in the sidebar.",
                "Click  + Add Supplier.",
                "Enter: Supplier Name (required), Company, Phone, Email, Address.",
                "Click  Save."
            });
            AddH2(doc, fH2, "6.2  Editing / Deleting a Supplier");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "Select the supplier row in the grid.",
                "Click  Edit  to modify details, or  Delete  to deactivate.",
                "Deleted suppliers are soft-deleted (marked Inactive) — their data is kept for medicine records."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 7 — CATEGORY MANAGEMENT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 7", "Admin Guide — Category Management", Blue, fStep, fCoverSub);
            AddBody(doc, fBody, "Categories group medicines (e.g. Analgesics, Antibiotics, Vitamins). Every medicine must have a category.");
            AddH2(doc, fH2, "7.1  Adding a Category");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Categories  in the sidebar.",
                "Click  + Add Category.",
                "Enter Category Name (required) and an optional Description.",
                "Click  Save."
            });
            AddH2(doc, fH2, "7.2  Deleting a Category");
            AddSteps(doc, fStep, fBody, Red, new[]
            {
                "Select the category row.",
                "Click  Delete.",
                "If the category has medicines linked to it, deletion is blocked — reassign those medicines first."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 8 — DISCOUNT MANAGEMENT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 8", "Admin Guide — Discount Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "8.1  Creating a Discount");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Discounts  in the sidebar.",
                "Click  + Add Discount.",
                "Enter Discount Name and Percentage (e.g. 10 for 10%).",
                "Set Start Date and End Date for the offer period.",
                "Tick  Active  to make it immediately available.",
                "Click  Save."
            });
            AddH2(doc, fH2, "8.2  Linking Discounts to Medicines");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "Go to  Medicines  and open the Add/Edit Medicine form.",
                "In the  Discount  dropdown, select the discount to apply.",
                "Save the medicine — the discount price is calculated automatically at checkout."
            });
            AddTip(doc, fTip, "Only active discounts within their date range are shown to customers at checkout.");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 9 — REPORTS
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 9", "Admin Guide — Reports", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "9.1  Generating a Sales Report");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Reports  in the sidebar.",
                "Set the  From Date  and  To Date  using the date pickers.",
                "Select report type: Sales Report, Stock Report, or Top Medicines.",
                "Click  Generate — the data table populates below.",
                "Click  Export PDF  to save as a landscape PDF.",
                "Click  Export Excel  to save as a styled spreadsheet."
            });
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Report Type", "Columns Shown" },
                new[,]
                {
                    { "Sales Report",  "Date | Total Orders | Sales | Discounts | Net Revenue" },
                    { "Stock Report",  "Medicine | Category | Stock | Min Stock | Expiry | Status" },
                    { "Top Medicines", "Medicine | Category | Total Sold | Total Revenue" }
                });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 10 — SETTINGS
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 10", "Admin Guide — Settings & Profile", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "10.1  Updating Admin Profile");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Settings  in the sidebar.",
                "Under  Profile Information, edit Full Name and Email.",
                "Click  Save Profile."
            });
            AddH2(doc, fH2, "10.2  Changing Admin Password");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "Scroll to  Change Password  section.",
                "Enter Current Password, New Password, and Confirm New Password.",
                "Click  Change Password.",
                "You will remain logged in — use the new password next time."
            });
            AddH2(doc, fH2, "10.3  Testing Database Connection");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "In the  Database Info  card (top-right of Settings), click  Test Connection.",
                "A message confirms whether the SQL Server connection is working."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 11 — CUSTOMER DASHBOARD
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 11", "Customer Guide — Dashboard", Green, fStep, fCoverSub);
            AddBody(doc, fBody, "After logging in as a Customer, the Customer Dashboard shows:");
            AddTable(doc, fTh, fTd, Green, Border, Light,
                new[] { "Card", "What it Shows" },
                new[,]
                {
                    { "Total Orders",      "All orders you have ever placed"            },
                    { "Active Orders",     "Orders currently being processed"            },
                    { "Completed Orders",  "Orders successfully delivered"               },
                    { "Total Spent",       "Your total spending (Rs.)"                  },
                    { "Cart Items",        "Number of medicines currently in your cart"  }
                });
            AddBody(doc, fBody, "\nThe sidebar contains: Dashboard | Search Medicines | My Cart | My Orders | My Profile | Logout");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 12 — SEARCH & BUY
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 12", "Customer Guide — Search & Buy Medicines", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "12.1  Searching for Medicines");
            AddSteps(doc, fStep, fBody, Green, new[]
            {
                "Click  Search Medicines  in the sidebar.",
                "Type a medicine name in the search box — results update instantly.",
                "Use the  Category  dropdown to filter by medicine type.",
                "Tick  In Stock Only  to hide out-of-stock medicines.",
                "Tick  Discounted  to show only medicines with active discounts."
            });
            AddH2(doc, fH2, "12.2  Adding to Cart");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "Click on a medicine card to view details.",
                "Enter the quantity you want.",
                "Click  Add to Cart.",
                "A confirmation message appears. The cart count in the sidebar updates."
            });
            AddWarn(doc, fWarn, "Medicines marked Prescription Required need a valid prescription uploaded at checkout.");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 13 — CART & CHECKOUT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 13", "Customer Guide — Shopping Cart & Checkout", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "13.1  Managing Your Cart");
            AddSteps(doc, fStep, fBody, Green, new[]
            {
                "Click  My Cart  in the sidebar.",
                "Review all items, quantities, and prices.",
                "Click  +  or  -  to adjust quantity, or  Remove  to delete an item.",
                "The Total updates automatically.",
                "Click  Clear Cart  to remove everything.",
                "Click  Proceed to Checkout  when ready."
            });
            AddH2(doc, fH2, "13.2  Checkout Process");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "Review the Order Summary (items, subtotal, discount, final total).",
                "Select Payment Method: Cash / Credit-Debit Card / Online Transfer.",
                "Set a Pickup Date (the date you will collect your order).",
                "If any medicine needs a prescription, click  Upload Prescription.",
                "Add any Notes (optional).",
                "Click  Place Order.",
                "A success message shows your Order ID — note it down.",
                "Your order now appears in  My Orders  with status  Pending."
            });
            AddTip(doc, fTip, "You will be notified (via the admin) when your order is Approved, Ready, or Delivered.");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 14 — MY ORDERS (CUSTOMER)
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 14", "Customer Guide — My Orders", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "14.1  Viewing Your Orders");
            AddSteps(doc, fStep, fBody, Green, new[]
            {
                "Click  My Orders  in the sidebar.",
                "All your orders are listed with Order ID, Date, Items, Amount, Status.",
                "Use the  Filter  dropdown (top-right) to view by status.",
                "Status colours: Orange=Pending, Blue=Approved, Teal=Ready, Green=Delivered, Red=Cancelled."
            });
            AddH2(doc, fH2, "14.2  Viewing Order Details");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click the order row to select it.",
                "Click  View Details.",
                "A popup shows the full item list, individual prices, and the total payable."
            });
            AddH2(doc, fH2, "14.3  Downloading Invoice");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "Select an order row.",
                "Click  Download Invoice.",
                "Choose a save location in the dialog.",
                "The PDF invoice opens automatically — it includes all items, prices, discount, and total."
            });
            AddH2(doc, fH2, "14.4  Cancelling an Order");
            AddSteps(doc, fStep, fBody, Red, new[]
            {
                "Select an order with status  Pending.",
                "Click  Cancel Order.",
                "Confirm the cancellation in the popup.",
                "The order status changes to  Cancelled  immediately."
            });
            AddWarn(doc, fWarn, "Only Pending orders can be cancelled. Approved, Ready, or Delivered orders cannot be cancelled by the customer.");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 15 — CUSTOMER PROFILE
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 15", "Customer Guide — Profile Management", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "15.1  Updating Profile Information");
            AddSteps(doc, fStep, fBody, Green, new[]
            {
                "Click  My Profile  in the sidebar.",
                "Edit First Name, Last Name, Phone, or Address.",
                "Click  Save Profile."
            });
            AddH2(doc, fH2, "15.2  Changing Password");
            AddSteps(doc, fStep, fBody, Orange, new[]
            {
                "In  My Profile, scroll to Change Password.",
                "Enter Current Password, New Password, Confirm New Password.",
                "Click  Change Password."
            });
            AddH2(doc, fH2, "15.3  Uploading a Profile Picture");
            AddSteps(doc, fStep, fBody, Teal, new[]
            {
                "Click the profile picture area or  Upload Photo  button.",
                "Select a JPG or PNG image (max recommended size: 2 MB).",
                "The new photo is saved and shown immediately."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 16 — SUPPLIER REFERENCE
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 16", "Supplier Management Reference", Blue, fStep, fCoverSub);
            AddBody(doc, fBody, "Suppliers do not have a separate login in SmartMed. All supplier data is managed by the Admin.");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Field", "Description", "Required" },
                new[,]
                {
                    { "Supplier Name", "Full name or business name of the supplier",    "Yes" },
                    { "Company",       "Company or organisation the supplier belongs to","No"  },
                    { "Phone",         "Contact phone number",                           "No"  },
                    { "Email",         "Contact email address",                          "No"  },
                    { "Address",       "Physical address of the supplier",               "No"  }
                });
            AddTip(doc, fTip, "Always add suppliers before adding medicines, since every medicine must be linked to a supplier.");
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 17 — PDF & EXCEL EXPORT
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 17", "PDF & Excel Export Guide", Blue, fStep, fCoverSub);
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Where", "Button", "Output File", "Format" },
                new[,]
                {
                    { "Admin → Orders",     "Invoice",        "Invoice_Order00001.pdf",              "A4 Portrait"   },
                    { "Customer → My Orders","Download Invoice","Invoice_Order00001.pdf",            "A4 Portrait"   },
                    { "Admin → Reports",    "Export PDF",     "SalesReport_YYYYMMDD_YYYYMMDD.pdf",  "A4 Landscape"  },
                    { "Admin → Reports",    "Export Excel",   "SalesReport_YYYYMMDD_YYYYMMDD.xlsx", "Excel (.xlsx)" },
                    { "Admin → Medicines",  "Export PDF",     "SalesReport_today.pdf",              "A4 Landscape"  },
                    { "Admin → Medicines",  "Export Excel",   "MedicineList.xlsx",                  "Excel (.xlsx)" },
                    { "Admin → Settings",   "User Guide PDF", "SmartMed_UserGuide.pdf",             "A4 Portrait"   }
                });
            AddH2(doc, fH2, "Steps for any export:");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click the Export button.",
                "A Save File dialog appears — navigate to your preferred folder.",
                "Click  Save.",
                "The file opens automatically in your default PDF viewer or Excel."
            });
            doc.NewPage();

            // ══════════════════════════════════════════════════════
            // SECTION 18 — TROUBLESHOOTING
            // ══════════════════════════════════════════════════════
            AddSectionBanner(doc, "Section 18", "Troubleshooting & Tips", Orange, fStep, fCoverSub);
            AddTable(doc, fTh, fTd, Orange, Border, Light,
                new[] { "Problem", "Solution" },
                new[,]
                {
                    { "Cannot login",                    "Check username/password. After 5 fails, restart the app to reset lock." },
                    { "PDF button does nothing",          "Ensure iTextSharp NuGet package is installed and the project is rebuilt." },
                    { "Excel export fails",               "Ensure EPPlus NuGet package is installed." },
                    { "Database connection error",        "Check SQL Server Express is running. Go to Settings → Test Connection." },
                    { "Medicine not showing in search",   "Check that medicine Status is set to Active." },
                    { "Cannot add medicine to cart",      "Medicine may be out of stock or inactive. Check stock quantity." },
                    { "Cannot delete category",           "Category has medicines linked to it. Reassign or delete medicines first." },
                    { "Order stuck in Pending",           "Admin must approve it via Orders → Update Status → Approved." },
                    { "Low stock alert at login",         "A medicine is below its minimum stock. Go to Medicines and restock." },
                    { "Images not showing",               "Check the Images folder exists in the app directory." }
                });

            // Final footer
            doc.Add(Chunk.NEWLINE);
            AddLine(doc, Blue);
            doc.Add(new Paragraph($"\nSmartMed Pharmacy Management System  |  User Guide v1.0  |  Generated {DateTime.Now:dd MMM yyyy}",
                fNote) { Alignment = Element.ALIGN_CENTER });
        }

        // ── Guide Helpers ─────────────────────────────────────────
        private static void AddSectionBanner(Document doc, string tag, string title,
            BaseColor color, Font tagFont, Font titleFont)
        {
            var table = new PdfPTable(1) { WidthPercentage = 100, SpacingBefore = 4, SpacingAfter = 10 };
            var cell  = new PdfPCell
            {
                BackgroundColor = color,
                Border          = Rectangle.NO_BORDER,
                Padding         = 14
            };
            cell.AddElement(new Paragraph(tag,   FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.WHITE)) { SpacingAfter = 2 });
            cell.AddElement(new Paragraph(title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15, BaseColor.WHITE)));
            table.AddCell(cell);
            doc.Add(table);
        }

        private static void AddH1(Document doc, Font f, string text) =>
            doc.Add(new Paragraph(text, f) { SpacingBefore = 10, SpacingAfter = 4 });

        private static void AddH2(Document doc, Font f, string text) =>
            doc.Add(new Paragraph(text, f) { SpacingBefore = 10, SpacingAfter = 4 });

        private static void AddBody(Document doc, Font f, string text) =>
            doc.Add(new Paragraph(text, f) { SpacingBefore = 2, SpacingAfter = 4, Leading = 16 });

        private static void AddLine(Document doc, BaseColor color) =>
            doc.Add(new LineSeparator(1f, 100f, color, Element.ALIGN_CENTER, -2));

        private static void AddSteps(Document doc, Font stepFont, Font bodyFont, BaseColor color, string[] steps)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                var row = new PdfPTable(2) { WidthPercentage = 100, SpacingBefore = 3 };
                row.SetWidths(new float[] { 0.06f, 0.94f });
                var numCell = new PdfPCell(new Phrase($"{i + 1}", stepFont))
                {
                    BackgroundColor     = color,
                    Border              = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment   = Element.ALIGN_MIDDLE,
                    Padding             = 5
                };
                var txtCell = new PdfPCell(new Phrase(steps[i], bodyFont))
                {
                    Border          = Rectangle.NO_BORDER,
                    Padding         = 5,
                    BackgroundColor = new BaseColor(248, 250, 252)
                };
                row.AddCell(numCell);
                row.AddCell(txtCell);
                doc.Add(row);
            }
            doc.Add(Chunk.NEWLINE);
        }

        private static void AddTip(Document doc, Font f, string text) =>
            doc.Add(new Paragraph($"  Tip:  {text}", f)
                { SpacingBefore = 3, SpacingAfter = 6, Leading = 14 });

        private static void AddWarn(Document doc, Font f, string text) =>
            doc.Add(new Paragraph($"  Warning:  {text}", f)
                { SpacingBefore = 3, SpacingAfter = 6, Leading = 14 });

        private static void AddTable(Document doc, Font th, Font td,
            BaseColor headerBg, BaseColor border, BaseColor altBg,
            string[] headers, string[,] rows)
        {
            var table = new PdfPTable(headers.Length)
            {
                WidthPercentage = 100,
                SpacingBefore   = 6,
                SpacingAfter    = 8
            };
            foreach (var h in headers)
                table.AddCell(new PdfPCell(new Phrase(h, th))
                {
                    BackgroundColor     = headerBg,
                    Border              = Rectangle.NO_BORDER,
                    Padding             = 7,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            for (int r = 0; r < rows.GetLength(0); r++)
                for (int c = 0; c < rows.GetLength(1); c++)
                    table.AddCell(new PdfPCell(new Phrase(rows[r, c], td))
                    {
                        BackgroundColor = r % 2 == 0 ? BaseColor.WHITE : altBg,
                        BorderColor     = border,
                        Border          = Rectangle.BOTTOM_BORDER,
                        Padding         = 6
                    });
            doc.Add(table);
        }

        // ── Individual guide exports ──────────────────────────────
        public static void ExportAdminGuide()    => RunGuide("SmartMed_Admin_Guide",    BuildAdminGuide);
        public static void ExportCustomerGuide() => RunGuide("SmartMed_Customer_Guide", BuildCustomerGuide);
        public static void ExportSupplierGuide() => RunGuide("SmartMed_Supplier_Guide", BuildSupplierGuide);

        private static void RunGuide(string fileName, Action<Document, PdfWriter> builder)
        {
            using var sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = fileName };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            using var doc    = new Document(PageSize.A4, 50, 50, 60, 50);
            using var writer = PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
            doc.Open();
            builder(doc, writer);
            doc.Close();
            System.Diagnostics.Process.Start(sfd.FileName);
        }

        // ══════════════════════════════════════════════════════════
        // ADMIN GUIDE
        // ══════════════════════════════════════════════════════════
        private static void BuildAdminGuide(Document doc, PdfWriter writer)
        {
            var Blue   = new BaseColor(37,  99, 235);
            var Green  = new BaseColor(22, 163,  74);
            var Orange = new BaseColor(234,179,   8);
            var Red    = new BaseColor(220,  38,  38);
            var Teal   = new BaseColor( 13,148, 136);
            var Light  = new BaseColor(248,250,252);
            var Border = new BaseColor(226,232,240);
            var fCover    = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 28, BaseColor.WHITE);
            var fCoverSub = FontFactory.GetFont(FontFactory.HELVETICA,      13, new BaseColor(219,234,254));
            var fH2       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(15,23,42));
            var fBody     = FontFactory.GetFont(FontFactory.HELVETICA,      10, new BaseColor(30,41,59));
            var fStep     = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var fTip      = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(21,128,61));
            var fWarn     = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(180,35,35));
            var fTh       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  9, BaseColor.WHITE);
            var fTd       = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(30,41,59));

            // Cover
            var cover = new PdfPTable(1) { WidthPercentage = 100 };
            var cc = new PdfPCell { BackgroundColor = Blue, Border = Rectangle.NO_BORDER, Padding = 60, MinimumHeight = 500 };
            cc.AddElement(new Paragraph("SmartMed Pharmacy\n", fCover) { Alignment = Element.ALIGN_CENTER });
            cc.AddElement(new Paragraph("Admin User Guide\n\n", fCoverSub) { Alignment = Element.ALIGN_CENTER });
            cc.AddElement(new Paragraph($"Step-by-step guide for system administrators\nVersion 1.0  |  {DateTime.Now:MMMM yyyy}", fCoverSub) { Alignment = Element.ALIGN_CENTER });
            cover.AddCell(cc); doc.Add(cover); doc.NewPage();

            // S1 Login
            AddSectionBanner(doc, "Section 1", "Admin Login", Blue, fStep, fCoverSub);
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Launch SmartMed.exe — the Login window opens.",
                "Click the  ADMIN  tab at the top of the login form.",
                "Enter Username (default: admin) and Password (default: Admin@123).",
                "Click  Login — the Admin Dashboard opens."
            });
            AddTip(doc, fTip, "After 5 wrong attempts the account is locked. Restart the application to reset.");
            doc.NewPage();

            // S2 Dashboard
            AddSectionBanner(doc, "Section 2", "Admin Dashboard", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "Stat Cards");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Card", "What it Shows" },
                new[,] {
                    { "Today's Sales",     "Total revenue collected today (Rs.)" },
                    { "Monthly Revenue",   "Total revenue for the current month (Rs.)" },
                    { "Total Customers",   "Number of registered customer accounts" },
                    { "Total Medicines",   "Total active medicines in inventory" },
                    { "Pending Orders",    "Orders waiting to be processed" },
                    { "Delivered Orders",  "Orders successfully delivered" },
                    { "Low Stock Items",   "Medicines below minimum stock level" },
                    { "Expired Medicines", "Medicines past their expiry date" }
                });
            AddH2(doc, fH2, "Quick Actions");
            AddBody(doc, fBody, "Four buttons at the bottom navigate to: Add Medicine | View Orders | Customers | View Reports\nThe two cards below the stat row show Top Selling Medicines and Recent Orders. Click  View All →  to open the full list.");
            doc.NewPage();

            // S3 Medicines
            AddSectionBanner(doc, "Section 3", "Medicine Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "3.1  Adding a Medicine");
            AddSteps(doc, fStep, fBody, Blue, new[]
            {
                "Click  Medicines  in the left sidebar.",
                "Click  + Add Medicine  (top-right).",
                "Fill in Medicine Code (or click  Auto  to generate) and Medicine Name.",
                "Select Category and Supplier from the dropdowns.",
                "Enter Price (Rs.), Stock Quantity, and Minimum Stock level.",
                "Set the Expiry Date (must be a future date).",
                "Optionally fill Dosage, Unit, Description, Discount, and Status.",
                "Tick  Prescription Required  if a prescription is needed.",
                "Click  Upload  to attach a medicine image (optional).",
                "Click  Save — the medicine appears in the list immediately."
            });
            AddH2(doc, fH2, "3.2  Editing a Medicine");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Click the medicine row to select it.",
                "Click  Edit  in the bottom action bar.",
                "Modify the required fields and click  Save."
            });
            AddH2(doc, fH2, "3.3  Search & Filter");
            AddBody(doc, fBody, "• Type in the Search box to filter by name or code in real time.\n• Use Category dropdown to show one category.\n• Use Status dropdown to show Active or Inactive medicines.");
            AddH2(doc, fH2, "3.4  Export");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Button", "Output" },
                new[,] {
                    { "Export Excel", "Saves a styled .xlsx file of the current medicine list" },
                    { "Export PDF",   "Saves a PDF sales report for today" }
                });
            doc.NewPage();

            // S4 Orders
            AddSectionBanner(doc, "Section 4", "Order Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "4.1  Order Status Workflow");
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Status", "Meaning", "Next Action" },
                new[,] {
                    { "Pending",   "Customer placed order, awaiting review",  "Approve or Reject"  },
                    { "Approved",  "Admin confirmed — being prepared",         "Mark as Ready"      },
                    { "Ready",     "Packed and ready for pickup",              "Mark as Delivered"  },
                    { "Delivered", "Customer received the order",              "Final state"        },
                    { "Cancelled", "Cancelled by customer (Pending only)",     "Final state"        },
                    { "Rejected",  "Rejected by admin",                        "Final state"        }
                });
            AddH2(doc, fH2, "4.2  Updating Order Status");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Click the order row to select it.",
                "Click  Update Status  in the bottom bar.",
                "Select the new status and click  Update."
            });
            AddH2(doc, fH2, "4.3  Generating an Invoice");
            AddSteps(doc, fStep, fBody, Green, new[] {
                "Select an order in the grid.",
                "Click  Invoice  in the bottom action bar.",
                "Choose a save location in the Save File dialog.",
                "The PDF invoice opens automatically."
            });
            doc.NewPage();

            // S5 Customers
            AddSectionBanner(doc, "Section 5", "Customer Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "5.1  Adding a Customer");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click  Customers  in the sidebar.",
                "Click  + Add Customer  (top-right).",
                "Fill in First Name, Last Name, Email, Phone, Address.",
                "Click  Save. Default password  Customer@123  is assigned.",
                "Ask the customer to log in and change their password."
            });
            AddH2(doc, fH2, "5.2  Enable / Disable Account");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Select the customer row.",
                "Click  Enable  to restore access, or  Disable  to block login."
            });
            AddWarn(doc, fWarn, "A disabled customer cannot log in until an admin re-enables their account.");
            AddH2(doc, fH2, "5.3  Viewing a Customer's Orders");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "Select the customer row.",
                "Click  Orders  in the bottom bar.",
                "A popup shows all orders with summary stats (Total Orders, Total Spent, Pending, Delivered)."
            });
            doc.NewPage();

            // S6 Suppliers
            AddSectionBanner(doc, "Section 6", "Supplier Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "6.1  Adding a Supplier");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click  Suppliers  in the sidebar.",
                "Click  + Add Supplier.",
                "Enter Supplier Name (required), Company, Phone, Email, Address.",
                "Click  Save."
            });
            AddH2(doc, fH2, "6.2  Editing / Deleting");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Select the supplier row.",
                "Click  Edit  to modify, or  Delete  to soft-delete.",
                "Deleted suppliers are kept in the database for medicine records."
            });
            AddTip(doc, fTip, "Add suppliers before adding medicines — every medicine must be linked to a supplier.");
            doc.NewPage();

            // S7 Categories
            AddSectionBanner(doc, "Section 7", "Category Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "7.1  Adding a Category");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click  Categories  in the sidebar.",
                "Click  + Add Category.",
                "Enter Category Name (required) and optional Description.",
                "Click  Save."
            });
            AddWarn(doc, fWarn, "A category with linked medicines cannot be deleted. Reassign those medicines first.");
            doc.NewPage();

            // S8 Discounts
            AddSectionBanner(doc, "Section 8", "Discount Management", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "8.1  Creating a Discount");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click  Discounts  in the sidebar.",
                "Click  + Add Discount.",
                "Enter Discount Name and Percentage (e.g. 10 for 10%).",
                "Set Start Date and End Date for the offer period.",
                "Tick  Active  to make it immediately available.",
                "Click  Save."
            });
            AddH2(doc, fH2, "8.2  Applying Discount to a Medicine");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Go to  Medicines  and open Add/Edit Medicine form.",
                "In the  Discount  dropdown, select the discount.",
                "Save — the discounted price is calculated at checkout automatically."
            });
            AddTip(doc, fTip, "Only active discounts within their date range appear to customers at checkout.");
            doc.NewPage();

            // S9 Reports
            AddSectionBanner(doc, "Section 9", "Reports", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "9.1  Generating a Report");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click  Reports  in the sidebar.",
                "Set  From Date  and  To Date.",
                "Select report type: Sales Report | Stock Report | Top Medicines.",
                "Click  Generate — the data table populates.",
                "Click  Export PDF  for a landscape PDF report.",
                "Click  Export Excel  for a styled spreadsheet."
            });
            AddTable(doc, fTh, fTd, Blue, Border, Light,
                new[] { "Report Type", "Columns" },
                new[,] {
                    { "Sales Report",  "Date | Orders | Sales | Discounts | Net Revenue" },
                    { "Stock Report",  "Medicine | Category | Stock | Min Stock | Expiry | Status" },
                    { "Top Medicines", "Medicine | Category | Total Sold | Total Revenue" }
                });
            doc.NewPage();

            // S10 Settings
            AddSectionBanner(doc, "Section 10", "Settings & Profile", Blue, fStep, fCoverSub);
            AddH2(doc, fH2, "10.1  Updating Profile");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click  Settings  in the sidebar.",
                "Edit Full Name and Email in the  Profile Information  section.",
                "Click  Save Profile."
            });
            AddH2(doc, fH2, "10.2  Changing Password");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Enter Current Password, New Password, and Confirm New Password.",
                "Click  Change Password."
            });
            AddH2(doc, fH2, "10.3  Testing Database Connection");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "In the  Database Info  card, click  Test Connection.",
                "A message confirms whether the SQL Server connection is working."
            });
            doc.NewPage();

            // Footer
            AddLine(doc, Blue);
            doc.Add(new Paragraph($"\nSmartMed Admin Guide  |  v1.0  |  Generated {DateTime.Now:dd MMM yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, new BaseColor(100,116,139)))
                { Alignment = Element.ALIGN_CENTER });
        }

        // ══════════════════════════════════════════════════════════
        // CUSTOMER GUIDE
        // ══════════════════════════════════════════════════════════
        private static void BuildCustomerGuide(Document doc, PdfWriter writer)
        {
            var Blue   = new BaseColor(37,  99, 235);
            var Green  = new BaseColor(22, 163,  74);
            var Orange = new BaseColor(234,179,   8);
            var Red    = new BaseColor(220,  38,  38);
            var Teal   = new BaseColor( 13,148, 136);
            var Light  = new BaseColor(248,250,252);
            var Border = new BaseColor(226,232,240);
            var fCover    = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 28, BaseColor.WHITE);
            var fCoverSub = FontFactory.GetFont(FontFactory.HELVETICA,      13, new BaseColor(187,247,208));
            var fH2       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(15,23,42));
            var fBody     = FontFactory.GetFont(FontFactory.HELVETICA,      10, new BaseColor(30,41,59));
            var fStep     = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var fTip      = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(21,128,61));
            var fWarn     = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(180,35,35));
            var fTh       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  9, BaseColor.WHITE);
            var fTd       = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(30,41,59));

            // Cover
            var cover = new PdfPTable(1) { WidthPercentage = 100 };
            var cc = new PdfPCell { BackgroundColor = Green, Border = Rectangle.NO_BORDER, Padding = 60, MinimumHeight = 500 };
            cc.AddElement(new Paragraph("SmartMed Pharmacy\n", fCover) { Alignment = Element.ALIGN_CENTER });
            cc.AddElement(new Paragraph("Customer User Guide\n\n", fCoverSub) { Alignment = Element.ALIGN_CENTER });
            cc.AddElement(new Paragraph($"Step-by-step guide for customers\nVersion 1.0  |  {DateTime.Now:MMMM yyyy}", fCoverSub) { Alignment = Element.ALIGN_CENTER });
            cover.AddCell(cc); doc.Add(cover); doc.NewPage();

            // S1 Registration & Login
            AddSectionBanner(doc, "Section 1", "Registration & Login", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "1.1  Creating a New Account");
            AddSteps(doc, fStep, fBody, Green, new[] {
                "Open SmartMed and click the  CUSTOMER  tab on the Login window.",
                "Click  Register  below the login fields.",
                "Fill in First Name, Last Name, Email, Phone, Address.",
                "Enter a strong Password — the strength meter must show  Fair  or above.",
                "Confirm your password in the  Confirm Password  field.",
                "Click  Create Account. A success message appears.",
                "Return to Login, enter your Email and Password, and click  Login."
            });
            AddTip(doc, fTip, "Password must be 8+ characters with uppercase, lowercase, number and special character (e.g. Pass@123).");
            AddH2(doc, fH2, "1.2  Logging In");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "Open SmartMed and click the  CUSTOMER  tab.",
                "Enter your registered Email and Password.",
                "Tick  Remember Me  to save credentials for next time.",
                "Click  Login — the Customer Dashboard opens."
            });
            doc.NewPage();

            // S2 Dashboard
            AddSectionBanner(doc, "Section 2", "Customer Dashboard", Green, fStep, fCoverSub);
            AddBody(doc, fBody, "After login the dashboard shows your account summary at a glance:");
            AddTable(doc, fTh, fTd, Green, Border, Light,
                new[] { "Card", "What it Shows" },
                new[,] {
                    { "Total Orders",   "All orders you have ever placed"           },
                    { "Active Orders",  "Orders currently being processed"           },
                    { "Completed",      "Orders successfully delivered"              },
                    { "Total Spent",    "Your total spending in Rs."                },
                    { "Cart Items",     "Number of medicines currently in your cart" }
                });
            AddBody(doc, fBody, "\nThe Quick Actions buttons navigate directly to:\nSearch Medicine  |  View Cart  |  My Orders  |  My Profile\n\nThe Recent Orders card shows your last 8 orders with colour-coded status.");
            doc.NewPage();

            // S3 Search & Buy
            AddSectionBanner(doc, "Section 3", "Search & Buy Medicines", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "3.1  Searching for a Medicine");
            AddSteps(doc, fStep, fBody, Green, new[] {
                "Click  Search Medicine  in the left sidebar.",
                "Type a medicine name in the search box — results update instantly.",
                "Use the  Category  dropdown to filter by medicine type.",
                "Tick  In Stock Only  to hide out-of-stock items.",
                "Tick  Discounted  to show only medicines with active discounts."
            });
            AddH2(doc, fH2, "3.2  Adding to Cart");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "Click on a medicine card to view its details.",
                "Enter the quantity you want.",
                "Click  Add to Cart.",
                "A confirmation message appears. The cart count in the sidebar updates."
            });
            AddWarn(doc, fWarn, "Medicines marked 'Prescription Required' need a valid prescription uploaded at checkout.");
            doc.NewPage();

            // S4 Cart & Checkout
            AddSectionBanner(doc, "Section 4", "Shopping Cart & Checkout", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "4.1  Managing Your Cart");
            AddSteps(doc, fStep, fBody, Green, new[] {
                "Click  My Cart  in the sidebar.",
                "Review all items, quantities, and prices.",
                "Click  +  or  -  to adjust quantity, or  Remove  to delete an item.",
                "Click  Clear Cart  to remove everything.",
                "Click  Proceed to Checkout  when ready."
            });
            AddH2(doc, fH2, "4.2  Checkout Process");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "Review the Order Summary (items, subtotal, discount, final total).",
                "Select Payment Method: Cash / Credit-Debit Card / Online Transfer.",
                "Set a Pickup Date (the date you will collect your order).",
                "If any medicine needs a prescription, click  Upload Prescription.",
                "Add any Notes (optional).",
                "Click  Place Order.",
                "A success message shows your Order ID — note it down.",
                "Your order now appears in  My Orders  with status  Pending."
            });
            AddTip(doc, fTip, "The admin will update your order status. Check  My Orders  regularly.");
            doc.NewPage();

            // S5 My Orders
            AddSectionBanner(doc, "Section 5", "My Orders", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "5.1  Order Status Colours");
            AddTable(doc, fTh, fTd, Green, Border, Light,
                new[] { "Status", "Colour", "Meaning" },
                new[,] {
                    { "Pending",   "Orange", "Waiting for admin to review"             },
                    { "Approved",  "Blue",   "Admin confirmed — being prepared"         },
                    { "Ready",     "Teal",   "Packed and ready for pickup"              },
                    { "Delivered", "Green",  "Successfully delivered"                   },
                    { "Cancelled", "Red",    "Cancelled by you (only when Pending)"     }
                });
            AddH2(doc, fH2, "5.2  View Order Details");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Click an order row to select it.",
                "Click  View Details — a popup shows items, prices, and total."
            });
            AddH2(doc, fH2, "5.3  Download Invoice");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "Select an order row.",
                "Click  Download Invoice.",
                "Choose a save location — the PDF invoice opens automatically."
            });
            AddH2(doc, fH2, "5.4  Cancel an Order");
            AddSteps(doc, fStep, fBody, Red, new[] {
                "Select an order with status  Pending.",
                "Click  Cancel Order and confirm in the popup.",
                "The status changes to  Cancelled  immediately."
            });
            AddWarn(doc, fWarn, "Only Pending orders can be cancelled. Approved, Ready, or Delivered orders cannot be cancelled.");
            doc.NewPage();

            // S6 Profile
            AddSectionBanner(doc, "Section 6", "My Profile", Green, fStep, fCoverSub);
            AddH2(doc, fH2, "6.1  Updating Personal Information");
            AddSteps(doc, fStep, fBody, Green, new[] {
                "Click  My Profile  in the sidebar.",
                "Edit First Name, Last Name, Email, Phone, or Address.",
                "Click  Save Profile."
            });
            AddH2(doc, fH2, "6.2  Changing Password");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "In  My Profile, find the  Change Password  section.",
                "Enter Current Password, New Password, and Confirm New Password.",
                "Click  Change Password."
            });
            AddH2(doc, fH2, "6.3  Changing Profile Photo");
            AddSteps(doc, fStep, fBody, Teal, new[] {
                "Click  📷 Change Photo  in the blue profile banner.",
                "Select a JPG or PNG image.",
                "The new photo is saved and shown immediately."
            });

            // Footer
            doc.NewPage();
            AddLine(doc, Green);
            doc.Add(new Paragraph($"\nSmartMed Customer Guide  |  v1.0  |  Generated {DateTime.Now:dd MMM yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, new BaseColor(100,116,139)))
                { Alignment = Element.ALIGN_CENTER });
        }

        // ══════════════════════════════════════════════════════════
        // SUPPLIER GUIDE
        // ══════════════════════════════════════════════════════════
        private static void BuildSupplierGuide(Document doc, PdfWriter writer)
        {
            var SupColor = new BaseColor(13, 148, 136);  // Teal
            var Blue     = new BaseColor(37,  99, 235);
            var Orange   = new BaseColor(234,179,   8);
            var Red      = new BaseColor(220,  38,  38);
            var Light    = new BaseColor(248,250,252);
            var Border   = new BaseColor(226,232,240);
            var fCover    = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 28, BaseColor.WHITE);
            var fCoverSub = FontFactory.GetFont(FontFactory.HELVETICA,      13, new BaseColor(204,251,241));
            var fH2       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(15,23,42));
            var fBody     = FontFactory.GetFont(FontFactory.HELVETICA,      10, new BaseColor(30,41,59));
            var fStep     = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var fTip      = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(21,128,61));
            var fWarn     = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(180,35,35));
            var fTh       = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  9, BaseColor.WHITE);
            var fTd       = FontFactory.GetFont(FontFactory.HELVETICA,       9, new BaseColor(30,41,59));

            // Cover
            var cover = new PdfPTable(1) { WidthPercentage = 100 };
            var cc = new PdfPCell { BackgroundColor = SupColor, Border = Rectangle.NO_BORDER, Padding = 60, MinimumHeight = 500 };
            cc.AddElement(new Paragraph("SmartMed Pharmacy\n", fCover) { Alignment = Element.ALIGN_CENTER });
            cc.AddElement(new Paragraph("Supplier Management Guide\n\n", fCoverSub) { Alignment = Element.ALIGN_CENTER });
            cc.AddElement(new Paragraph($"Complete guide to managing suppliers in SmartMed\nVersion 1.0  |  {DateTime.Now:MMMM yyyy}", fCoverSub) { Alignment = Element.ALIGN_CENTER });
            cover.AddCell(cc); doc.Add(cover); doc.NewPage();

            // S1 Overview
            AddSectionBanner(doc, "Section 1", "What are Suppliers?", SupColor, fStep, fCoverSub);
            AddBody(doc, fBody, "Suppliers are the companies or individuals who provide medicines to SmartMed Pharmacy. Every medicine in the system must be linked to a supplier so you always know where to reorder stock from.");
            AddBody(doc, fBody, "\nSupplier management is handled entirely by the Admin. Suppliers do not have a separate login to the system.");
            AddTip(doc, fTip, "Always add suppliers BEFORE adding medicines, because every medicine form requires a supplier to be selected.");
            doc.NewPage();

            // S2 Adding
            AddSectionBanner(doc, "Section 2", "Adding a New Supplier", SupColor, fStep, fCoverSub);
            AddH2(doc, fH2, "Step-by-step:");
            AddSteps(doc, fStep, fBody, SupColor, new[] {
                "Log in as Admin and click  Suppliers  in the left sidebar.",
                "Click the  + Add Supplier  button (top-right).",
                "Enter the Supplier Name — this field is required.",
                "Enter Company name (the organisation the supplier belongs to).",
                "Enter Phone number for contacting the supplier.",
                "Enter Email address.",
                "Enter Address (physical location of the supplier).",
                "Click  Save — the supplier appears in the list immediately."
            });
            AddH2(doc, fH2, "Supplier Fields Reference");
            AddTable(doc, fTh, fTd, SupColor, Border, Light,
                new[] { "Field", "Description", "Required" },
                new[,] {
                    { "Supplier Name", "Full name or business name",                    "Yes" },
                    { "Company",       "Organisation the supplier belongs to",          "No"  },
                    { "Phone",         "Contact phone number",                          "No"  },
                    { "Email",         "Contact email address",                         "No"  },
                    { "Address",       "Physical address of the supplier",              "No"  }
                });
            doc.NewPage();

            // S3 Editing
            AddSectionBanner(doc, "Section 3", "Editing a Supplier", SupColor, fStep, fCoverSub);
            AddBody(doc, fBody, "You can update any supplier's details at any time without affecting the medicines linked to them.");
            AddH2(doc, fH2, "Step-by-step:");
            AddSteps(doc, fStep, fBody, Orange, new[] {
                "Click  Suppliers  in the sidebar.",
                "Click the supplier row in the grid to select it.",
                "Click  Edit  in the bottom action bar.",
                "Update the fields you want to change.",
                "Click  Save."
            });
            doc.NewPage();

            // S4 Deleting
            AddSectionBanner(doc, "Section 4", "Deleting a Supplier", SupColor, fStep, fCoverSub);
            AddBody(doc, fBody, "Deleting a supplier performs a 'soft delete' — the supplier is marked as Inactive but their data is kept in the database so existing medicine records are not broken.");
            AddH2(doc, fH2, "Step-by-step:");
            AddSteps(doc, fStep, fBody, Red, new[] {
                "Click  Suppliers  in the sidebar.",
                "Select the supplier row.",
                "Click  Delete  in the bottom action bar.",
                "Confirm the deletion in the popup.",
                "The supplier is hidden from the active list but not permanently removed."
            });
            AddWarn(doc, fWarn, "A deleted supplier's name still appears on medicine records that were previously linked to them — no data is lost.");
            doc.NewPage();

            // S5 Linking to Medicines
            AddSectionBanner(doc, "Section 5", "Linking Suppliers to Medicines", SupColor, fStep, fCoverSub);
            AddBody(doc, fBody, "Every medicine must be linked to one supplier. This is done when adding or editing a medicine.");
            AddH2(doc, fH2, "Step-by-step:");
            AddSteps(doc, fStep, fBody, SupColor, new[] {
                "Go to  Medicines  in the sidebar.",
                "Click  + Add Medicine  (or select a medicine and click  Edit ).",
                "In the  Supplier  dropdown, select the supplier for this medicine.",
                "Complete the rest of the medicine form.",
                "Click  Save — the medicine is now linked to the selected supplier."
            });
            AddTip(doc, fTip, "If the supplier you need doesn't appear in the dropdown, go to Suppliers first and add them, then return to the medicine form.");
            doc.NewPage();

            // S6 Best Practices
            AddSectionBanner(doc, "Section 6", "Best Practices", SupColor, fStep, fCoverSub);
            AddTable(doc, fTh, fTd, SupColor, Border, Light,
                new[] { "Practice", "Why it Matters" },
                new[,] {
                    { "Add suppliers before medicines",       "The medicine form requires a supplier — add them first"          },
                    { "Keep phone and email up to date",      "Easy to contact for reordering when stock runs low"              },
                    { "Use Company field for group orders",   "Identify which company to contact for bulk orders"               },
                    { "Don't delete active suppliers",        "Deletion hides the supplier; reassign medicines first if needed" },
                    { "One supplier per medicine",            "Each medicine is linked to one supplier for clear traceability"  }
                });
            AddH2(doc, fH2, "Supplier — Low Stock Workflow");
            AddSteps(doc, fStep, fBody, Blue, new[] {
                "Go to  Dashboard — check the  Low Stock Items  card.",
                "Click  Medicines  and filter for low-stock items.",
                "Note the Supplier name shown in the medicine record.",
                "Go to  Suppliers, find the supplier, and get their contact details.",
                "Contact the supplier to place a restock order.",
                "When stock arrives, update the medicine's Stock Quantity in  Edit Medicine."
            });

            // Footer
            AddLine(doc, SupColor);
            doc.Add(new Paragraph($"\nSmartMed Supplier Guide  |  v1.0  |  Generated {DateTime.Now:dd MMM yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, new BaseColor(100,116,139)))
                { Alignment = Element.ALIGN_CENTER });
        }

        public static void ExportSalesReport(System.Data.DataTable dt, DateTime from, DateTime to)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Files|*.pdf";
                sfd.FileName = $"SalesReport_{from:yyyyMMdd}_{to:yyyyMMdd}";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var doc = new Document(PageSize.A4.Rotate(), 30, 30, 40, 30))
                using (var writer = PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create)))
                {
                    doc.Open();
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(37, 99, 235));
                    doc.Add(new Paragraph("SmartMed - Sales Report", titleFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph($"Period: {from:dd/MM/yyyy} to {to:dd/MM/yyyy}",
                        FontFactory.GetFont(FontFactory.HELVETICA, 11, Dark)) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(Chunk.NEWLINE);

                    var table = new PdfPTable(dt.Columns.Count) { WidthPercentage = 100 };
                    var hFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                    var nFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, Dark);
                    foreach (System.Data.DataColumn col in dt.Columns)
                        table.AddCell(CreateCell(col.ColumnName, hFont, true, Element.ALIGN_CENTER, HeaderColor));
                    foreach (System.Data.DataRow row in dt.Rows)
                        foreach (System.Data.DataColumn col in dt.Columns)
                            table.AddCell(CreateCell(row[col].ToString(), nFont, false));
                    doc.Add(table);
                    doc.Close();
                }
                System.Diagnostics.Process.Start(sfd.FileName);
            }
        }

        public static void ExportGenericReport(System.Data.DataTable dt, string title, DateTime from, DateTime to)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Files|*.pdf";
                sfd.FileName = $"{title.Replace(" ", "")}_{from:yyyyMMdd}_{to:yyyyMMdd}";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var doc = new Document(PageSize.A4.Rotate(), 30, 30, 40, 30))
                using (var writer = PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create)))
                {
                    doc.Open();
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(37, 99, 235));
                    doc.Add(new Paragraph($"SmartMed - {title}", titleFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph($"Period: {from:dd/MM/yyyy} to {to:dd/MM/yyyy}",
                        FontFactory.GetFont(FontFactory.HELVETICA, 11, Dark)) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(Chunk.NEWLINE);

                    var table = new PdfPTable(dt.Columns.Count) { WidthPercentage = 100 };
                    var hFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                    var nFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, Dark);
                    foreach (System.Data.DataColumn col in dt.Columns)
                        table.AddCell(CreateCell(col.ColumnName, hFont, true, Element.ALIGN_CENTER, HeaderColor));
                    foreach (System.Data.DataRow row in dt.Rows)
                        foreach (System.Data.DataColumn col in dt.Columns)
                            table.AddCell(CreateCell(row[col].ToString(), nFont, false));
                    doc.Add(table);
                    doc.Close();
                }
                System.Diagnostics.Process.Start(sfd.FileName);
            }
        }
    }
}
