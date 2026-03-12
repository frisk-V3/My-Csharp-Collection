using System;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

static class Win32
{
    public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
    public const int WS_VISIBLE = 0x10000000;

    public const int WM_DESTROY = 0x0002;
    public const int WM_PAINT = 0x000F;
    public const int WM_TIMER = 0x0113;
    public const int WM_KEYDOWN = 0x0100;

    public const int COLOR_WINDOW = 5;
    public const int IDT_TIMER1 = 1;

    public const int VK_SPACE = 0x20;
    public const int VK_UP = 0x26;
    public const int VK_DOWN = 0x28;
    public const int VK_R = 0x52;

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public int fErase;
        public int rcPaint_left;
        public int rcPaint_top;
        public int rcPaint_right;
        public int rcPaint_bottom;
        public int fRestore;
        public int fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    public static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    [DllImport("user32.dll")]
    public static extern bool SetTimer(IntPtr hWnd, int nIDEvent, uint uElapse, IntPtr lpTimerFunc);

    [DllImport("user32.dll")]
    public static extern bool KillTimer(IntPtr hWnd, int uIDEvent);

    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
}

static class ClockCore
{
    private static DateTime _serverTime = DateTime.MinValue;
    private static DateTime _lastFetch = DateTime.MinValue;
    private const int TAI_OFFSET = 37;

    public static DateTime GetJST()
    {
        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
        catch
        {
            return DateTime.UtcNow.AddHours(9);
        }
    }

    public static DateTime GetUTC()
    {
        return DateTime.UtcNow;
    }

    public static DateTime GetLocal()
    {
        return DateTime.Now;
    }

    public static DateTime GetTAI()
    {
        return DateTime.UtcNow.AddSeconds(TAI_OFFSET);
    }

    public static DateTime GetServer()
    {
        if (_serverTime == DateTime.MinValue)
            return GetLocal();
        return _serverTime;
    }

    public static double GetDiffMs()
    {
        return (GetLocal() - GetServer()).TotalMilliseconds;
    }

    public static void MaybeUpdateServer()
    {
        if ((DateTime.UtcNow - _lastFetch).TotalSeconds < 60)
            return;

        _lastFetch = DateTime.UtcNow;

        try
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string json = wc.DownloadString("https://www.timeapi.io/api/v1/time/current/zone?timeZone=Asia/Tokyo");

                string key = "\"currentLocalTime\":\"";
                int idx = json.IndexOf(key);
                if (idx >= 0)
                {
                    int start = idx + key.Length;
                    int end = json.IndexOf('"', start);
                    if (end > start)
                    {
                        string iso = json.Substring(start, end - start);
                        DateTime parsed;
                        if (DateTime.TryParse(iso, out parsed))
                        {
                            _serverTime = parsed;
                        }
                    }
                }
            }
        }
        catch
        {
            // 無視
        }
    }
}

static class TimerCore
{
    public static TimeSpan Remaining = TimeSpan.Zero;
    public static bool Running = false;

    public static void Tick(double ms)
    {
        if (!Running) return;
        Remaining = Remaining - TimeSpan.FromMilliseconds(ms);
        if (Remaining <= TimeSpan.Zero)
        {
            Remaining = TimeSpan.Zero;
            Running = false;
            MessageBox.Show("タイマー終了", "Timer");
        }
    }

    public static void AddMinutes(int minutes)
    {
        Remaining = Remaining + TimeSpan.FromMinutes(minutes);
        if (Remaining < TimeSpan.Zero) Remaining = TimeSpan.Zero;
    }

    public static void Reset()
    {
        Remaining = TimeSpan.Zero;
        Running = false;
    }
}

static class ClockApp
{
    static IntPtr _hwnd;
    static Win32.WNDCLASS _wc;
    static WndProcDelegate _wndProc;

    const int TIMER_INTERVAL_MS = 50;

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [STAThread]
    public static void Main()
    {
        _wndProc = new WndProcDelegate(WndProc);
        IntPtr hInstance = Marshal.GetHINSTANCE(typeof(ClockApp).Module);

        _wc = new Win32.WNDCLASS();
        _wc.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc);
        _wc.hInstance = hInstance;
        _wc.hbrBackground = (IntPtr)(Win32.COLOR_WINDOW + 1);
        _wc.lpszClassName = "ClockWindow";

        if (Win32.RegisterClass(ref _wc) == 0)
        {
            MessageBox.Show("RegisterClass failed");
            return;
        }

        _hwnd = Win32.CreateWindowEx(
            0,
            _wc.lpszClassName,
            "Clock+Timer",
            Win32.WS_OVERLAPPEDWINDOW | Win32.WS_VISIBLE,
            100, 100, 800, 320,
            IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

        Win32.SetTimer(_hwnd, Win32.IDT_TIMER1, (uint)TIMER_INTERVAL_MS, IntPtr.Zero);

        Win32.MSG msg;
        while (Win32.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
        {
            Win32.TranslateMessage(ref msg);
            Win32.DispatchMessage(ref msg);
        }
    }

    static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == Win32.WM_TIMER)
        {
            ClockCore.MaybeUpdateServer();
            TimerCore.Tick(TIMER_INTERVAL_MS);
            Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
            return IntPtr.Zero;
        }

        if (msg == Win32.WM_KEYDOWN)
        {
            int vk = (int)wParam;
            if (vk == Win32.VK_SPACE)
            {
                TimerCore.Running = !TimerCore.Running;
            }
            else if (vk == Win32.VK_UP)
            {
                TimerCore.AddMinutes(1);
            }
            else if (vk == Win32.VK_DOWN)
            {
                TimerCore.AddMinutes(-1);
            }
            else if (vk == Win32.VK_R)
            {
                TimerCore.Reset();
            }
            return IntPtr.Zero;
        }

        if (msg == Win32.WM_PAINT)
        {
            Draw(hWnd);
            return IntPtr.Zero;
        }

        if (msg == Win32.WM_DESTROY)
        {
            Win32.KillTimer(hWnd, Win32.IDT_TIMER1);
            Win32.PostQuitMessage(0);
            return IntPtr.Zero;
        }

        return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    static void DrawAnalog(Graphics g, Rectangle rect, DateTime now)
    {
        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;
        int radius = Math.Min(rect.Width, rect.Height) / 2 - 10;

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using (Pen circlePen = new Pen(Color.White, 2))
        {
            g.DrawEllipse(circlePen, cx - radius, cy - radius, radius * 2, radius * 2);
        }

        for (int i = 0; i < 12; i++)
        {
            double angle = (Math.PI / 6.0) * i - Math.PI / 2.0;
            int r1 = radius - 10;
            int r2 = radius - 2;
            int x1 = cx + (int)(Math.Cos(angle) * r1);
            int y1 = cy + (int)(Math.Sin(angle) * r1);
            int x2 = cx + (int)(Math.Cos(angle) * r2);
            int y2 = cy + (int)(Math.Sin(angle) * r2);
            using (Pen p = new Pen(Color.Gray, 2))
            {
                g.DrawLine(p, x1, y1, x2, y2);
            }
        }

        double sec = now.Second + now.Millisecond / 1000.0;
        double min = now.Minute + sec / 60.0;
        double hour = (now.Hour % 12) + min / 60.0;

        double angHour = (Math.PI / 6.0) * hour - Math.PI / 2.0;
        double angMin = (Math.PI / 30.0) * min - Math.PI / 2.0;
        double angSec = (Math.PI / 30.0) * sec - Math.PI / 2.0;

        int rh = (int)(radius * 0.5);
        int rm = (int)(radius * 0.75);
        int rs = (int)(radius * 0.85);

        int hx = cx + (int)(Math.Cos(angHour) * rh);
        int hy = cy + (int)(Math.Sin(angHour) * rh);
        int mx = cx + (int)(Math.Cos(angMin) * rm);
        int my = cy + (int)(Math.Sin(angMin) * rm);
        int sx = cx + (int)(Math.Cos(angSec) * rs);
        int sy = cy + (int)(Math.Sin(angSec) * rs);

        using (Pen pH = new Pen(Color.White, 4))
        using (Pen pM = new Pen(Color.White, 3))
        using (Pen pS = new Pen(Color.Red, 2))
        {
            g.DrawLine(pH, cx, cy, hx, hy);
            g.DrawLine(pM, cx, cy, mx, my);
            g.DrawLine(pS, cx, cy, sx, sy);
        }

        using (Brush b = new SolidBrush(Color.White))
        {
            g.FillEllipse(b, cx - 4, cy - 4, 8, 8);
        }
    }

    static void Draw(IntPtr hWnd)
    {
        Win32.PAINTSTRUCT ps = new Win32.PAINTSTRUCT();
        ps.rgbReserved = new byte[32];

        IntPtr hdc = Win32.BeginPaint(hWnd, out ps);
        if (hdc == IntPtr.Zero) return;

        Win32.RECT rc;
        Win32.GetClientRect(hWnd, out rc);
        int width = rc.right - rc.left;
        int height = rc.bottom - rc.top;

        using (Bitmap back = new Bitmap(width, height))
        using (Graphics g = Graphics.FromImage(back))
        {
            g.Clear(Color.Black);

            DateTime now = ClockCore.GetJST();
            Rectangle analogRect = new Rectangle(10, 10, height - 20, height - 20);
            DrawAnalog(g, analogRect, now);

            Font small = new Font("Consolas", 12);
            Brush white = Brushes.White;
            Brush yellow = Brushes.Yellow;
            Brush magenta = Brushes.Magenta;
            Brush cyan = Brushes.Cyan;
            Brush green = Brushes.Lime;

            int x = analogRect.Right + 20;
            int y = 10;
            string fmt = "yyyy-MM-dd HH:mm:ss.fff";

            g.DrawString("JST   : " + ClockCore.GetJST().ToString(fmt), small, white, x, y); y += 20;
            g.DrawString("UTC   : " + ClockCore.GetUTC().ToString(fmt), small, white, x, y); y += 20;
            g.DrawString("TAI   : " + ClockCore.GetTAI().ToString(fmt) + " (UTC+37s)", small, magenta, x, y); y += 20;
            g.DrawString("Local : " + ClockCore.GetLocal().ToString(fmt), small, white, x, y); y += 20;
            g.DrawString("Server: " + ClockCore.GetServer().ToString(fmt), small, yellow, x, y); y += 20;

            double diff = ClockCore.GetDiffMs();
            string diffStr = string.Format("Diff(Local-Server): {0:+0.000;-0.000;0.000} ms", diff);
            g.DrawString(diffStr, small, green, x, y); y += 30;

            TimeSpan rem = TimerCore.Remaining;
            string timerStr = string.Format("Timer: {0:00}:{1:00}:{2:00}.{3:000}",
                rem.Hours, rem.Minutes, rem.Seconds, rem.Milliseconds);
            g.DrawString(timerStr, small, cyan, x, y); y += 20;

            string help1 = "↑: +1min  ↓: -1min  Space: Start/Stop  R: Reset";
            g.DrawString(help1, small, white, x, y);

            using (Graphics gScreen = Graphics.FromHdc(hdc))
            {
                gScreen.DrawImageUnscaled(back, 0, 0);
            }
        }

        Win32.EndPaint(hWnd, ref ps);
    }
}