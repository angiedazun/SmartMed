using System.Drawing;
using System.Windows.Forms;

namespace SmartMed.UI
{
    public static class AppTheme
    {
        // ── Light Palette ─────────────────────────────────────────
        public static readonly Color Background   = Color.FromArgb(243, 244, 246);  // #F3F4F6
        public static readonly Color Surface      = Color.White;
        public static readonly Color Border       = Color.FromArgb(209, 213, 219);  // #D1D5DB
        public static readonly Color Primary      = Color.FromArgb(37,  99,  235);  // #2563EB
        public static readonly Color PrimaryHover = Color.FromArgb(29,  78,  216);  // #1D4ED8
        public static readonly Color Success      = Color.FromArgb(22,  163, 74);   // #16A34A
        public static readonly Color Warning      = Color.FromArgb(217, 119, 6);    // #D97706
        public static readonly Color Danger       = Color.FromArgb(220, 38,  38);   // #DC2626
        public static readonly Color TextPrimary  = Color.FromArgb(17,  24,  39);   // #111827
        public static readonly Color TextMuted    = Color.FromArgb(107, 114, 128);  // #6B7280
        public static readonly Color CardBg       = Color.White;
        public static readonly Color InputBg      = Color.White;
        public static readonly Color Accent       = Color.FromArgb(14,  165, 233);  // #0EA5E9

        // Sidebar — vibrant blue (matches user-selected brand color)
        public static readonly Color SidebarBg      = Color.FromArgb(37,  99,  235);  // #2563EB blue-600
        public static readonly Color SidebarBg2     = Color.FromArgb(29,  78,  216);  // #1D4ED8 blue-700
        public static readonly Color SidebarLogoBg  = Color.FromArgb(30,  86,  214);  // slightly darker logo row
        public static readonly Color SidebarText    = Color.FromArgb(219, 234, 254);  // #DBEAFE blue-100 muted
        public static readonly Color SidebarActive  = Color.White;                     // white left accent bar
        public static readonly Color SidebarActiveBg= Color.FromArgb(35,  255, 255, 255); // 14% white tint for active
        public static readonly Color SidebarHover   = Color.FromArgb(20,  255, 255, 255); // 8% white hover
        public static readonly Color SidebarSep     = Color.FromArgb(55,  255, 255, 255); // 22% white separator

        // ── Fonts ──────────────────────────────────────────────────
        public static readonly Font FontTitle  = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static readonly Font FontHeader = new Font("Segoe UI", 14f, FontStyle.Bold);
        public static readonly Font FontLabel  = new Font("Segoe UI",  9f, FontStyle.Regular);
        public static readonly Font FontBold   = new Font("Segoe UI",  9f, FontStyle.Bold);
        public static readonly Font FontSmall  = new Font("Segoe UI",  8f, FontStyle.Regular);
        public static readonly Font FontButton = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FontInput  = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static readonly Font FontNav    = new Font("Segoe UI", 10f, FontStyle.Regular);

        // ── Sizes ──────────────────────────────────────────────────
        public const int NavWidth     = 220;
        public const int TopBarHeight = 60;
        public const int CardRadius   = 8;
        public const int ButtonHeight = 40;
        public const int InputHeight  = 38;

        // ── DataGrid ───────────────────────────────────────────────
        public static void StyleDataGrid(DataGridView dgv)
        {
            dgv.BackgroundColor  = Surface;
            dgv.GridColor        = Border;
            dgv.BorderStyle      = BorderStyle.None;

            dgv.DefaultCellStyle.BackColor          = Surface;
            dgv.DefaultCellStyle.ForeColor          = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Primary;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font               = FontLabel;
            dgv.DefaultCellStyle.Padding            = new Padding(4, 0, 4, 0);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font      = FontBold;
            dgv.ColumnHeadersBorderStyle  = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersHeight       = 40;
            dgv.RowTemplate.Height        = 36;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            dgv.EnableHeadersVisualStyles  = false;
            dgv.RowHeadersVisible          = false;
            dgv.SelectionMode              = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly                   = true;
            dgv.AllowUserToAddRows         = false;
            dgv.AllowUserToDeleteRows      = false;
            dgv.AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.None;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        // ── Helpers ────────────────────────────────────────────────
        public static Button CreateButton(string text, Color? bg = null, int width = 120, int height = 38)
        {
            return new Button
            {
                Text      = text,
                BackColor = bg ?? Primary,
                ForeColor = Color.White,
                Font      = FontButton,
                Width     = width,
                Height    = height,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        public static TextBox CreateInput(int width = 220, int height = 38)
        {
            return new TextBox
            {
                BackColor   = InputBg,
                ForeColor   = TextPrimary,
                Font        = FontInput,
                Width       = width,
                Height      = height,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static Label CreateLabel(string text, Font font = null, Color? color = null)
        {
            return new Label
            {
                Text      = text,
                Font      = font ?? FontLabel,
                ForeColor = color ?? TextPrimary,
                AutoSize  = true
            };
        }

        public static Panel CreateCard(int width, int height)
        {
            return new Panel { Width = width, Height = height, BackColor = CardBg };
        }

        public static Color StatusColor(string status) => status switch
        {
            "Active"    => Success,
            "Delivered" => Success,
            "Approved"  => Success,
            "Ready"     => Accent,
            "Pending"   => Warning,
            "Inactive"  => TextMuted,
            "Cancelled" => Danger,
            "Rejected"  => Danger,
            "Expired"   => Danger,
            "Disabled"  => Danger,
            _           => TextMuted
        };
    }
}
