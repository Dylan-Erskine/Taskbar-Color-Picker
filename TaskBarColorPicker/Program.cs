using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskbarColorPicker
{
    static class Program
    {
        public static NotifyIcon colorPickerIcon;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            colorPickerIcon = new NotifyIcon();
            colorPickerIcon.Icon = Properties.Resources.picker;
            colorPickerIcon.Text = "Pick Color";
            colorPickerIcon.Visible = true;
            colorPickerIcon.MouseClick += new MouseEventHandler(colorPickerIconClicked);

            Application.Run();
        }

        static List<BlockInteraction> blockerForms = new List<BlockInteraction>();

        private static void colorPickerIconClicked(object sender, MouseEventArgs e)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                Rectangle r = new Rectangle();
                r = Rectangle.Union(r, s.Bounds);

                BlockInteraction blockForm = new BlockInteraction();
                blockForm.Top = r.Top;
                blockForm.Left = r.Left;
                blockForm.Width = r.Width;
                blockForm.Height = r.Height;
                blockForm.Show();

                blockerForms.Add(blockForm);
            }

            MouseHook.Start();
            MouseHook.MouseAction += mouseEvent;
        }

        private static Thread mouseThread;

        private static void mouseEvent(object sender, MouseHook.RawMouseEventArgs e)
        {
            if(e.Message == MouseHook.MouseMessages.WM_LBUTTONDOWN)
            {
                blockerForms.ForEach(b => b.Close());

                MouseHook.Stop();
                MouseHook.MouseAction -= mouseEvent;
                colorPickerIcon.Icon = Properties.Resources.picker;

                Thread.Sleep(500);

                

                Color selectedColor = GetColorAt(e.Point.x, e.Point.y);
                string hexColor = ColorTranslator.ToHtml(Color.FromArgb(selectedColor.ToArgb()));

                Clipboard.SetText(hexColor);
            } else if(e.Message == MouseHook.MouseMessages.WM_MOUSEMOVE)
            {
                if(mouseThread != null && mouseThread.IsAlive)
                {
                    return;
                }

                mouseThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    setIconToColor(GetColorAt(e.Point.x, e.Point.y));
                });

                mouseThread.Start();
            }
        }

        private static void setIconToColor(Color color)
        {
            Bitmap colorImage = new Bitmap(48, 48);
            Graphics colorImageGraphics = Graphics.FromImage(colorImage);

            SolidBrush newBrush = new SolidBrush(color);

            colorImageGraphics.FillRectangle(newBrush, 0, 0, 48, 48);

            IntPtr Hicon = colorImage.GetHicon();
            Icon myIcon = Icon.FromHandle(Hicon);

            colorPickerIcon.Icon = myIcon;

            DestroyIcon(Hicon);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }
    }
}
