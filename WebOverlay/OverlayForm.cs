using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebOverlay
{
    public partial class OverlayForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Size
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }        

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        private string targetProcessName = "ffxiv_dx11";
        private Size size;

        public OverlayForm()
        {
            InitializeComponent();

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1 second
            timer.Tick += (sender, args) =>
            {
                monitorTimer_Tick(sender, args);
            };
            timer.Start();
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.Lime; // Key color
            this.TransparencyKey = Color.Lime;
            this.Opacity = 0.5; // Optional opacity
            

            // Hide from Alt-Tab
            int exStyle = (int)GetWindowLong(this.Handle, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW; // Hides from Alt+Tab
            SetWindowLong(this.Handle, GWL_EXSTYLE, (IntPtr)exStyle);

            // Click through
            exStyle = (int)GetWindowLong(this.Handle, GWL_EXSTYLE);
            exStyle |= WS_EX_TRANSPARENT;
            SetWindowLong(this.Handle, GWL_EXSTYLE, (IntPtr)exStyle);

            SetSize();
            WebView();

        }

        public async void WebView()
        {
            var webView = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = DockStyle.Fill,
                Size = size
            };
            this.Controls.Add(webView);
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate("https://hypno.nimja.com/visual/139");

            webView.NavigationCompleted += delegate
             {
                 webView.ExecuteScriptAsync("var introStartLink = document.getElementById('intro-start');introStartLink.click();");
             };
        }

        private void monitorTimer_Tick(object sender, EventArgs e)
        {
            var foregroundWindow = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(foregroundWindow, out pid);
            var proc = Process.GetProcessById((int)pid);

            this.Visible = proc.ProcessName.Equals(targetProcessName, StringComparison.OrdinalIgnoreCase);                       
        }

        private void SetSize()
        {
            Process[] processes = Process.GetProcessesByName(targetProcessName);
            IntPtr hWnd = processes[0].MainWindowHandle;

            RECT rect;
            if (GetWindowRect(hWnd, out rect))
            {
                size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            this.Location = new Point(rect.Left, rect.Top);
            this.Size = size;
        }
    }
}
