using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmartMed.UI
{
    public static class WinApiHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        private const uint EM_SETCUEBANNER = 0x1501;

        public static void SetPlaceholder(this TextBox txt, string hint)
        {
            if (txt.IsHandleCreated)
                SendMessage(txt.Handle, EM_SETCUEBANNER, (IntPtr)1, hint);
            else
                txt.HandleCreated += (s, e) => SendMessage(txt.Handle, EM_SETCUEBANNER, (IntPtr)1, hint);
        }
    }
}
