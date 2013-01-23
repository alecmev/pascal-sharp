using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using WinInterop = System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Globalization;

namespace PascalSharp
{
    public partial class MainWindow : Window
    {
        private PascalGraphics graphics;

        private Point at = new Point(200, 200);
        private Size radiuses = new Size(100, 100);
        private double alpha = 0;
        private bool isOpening = true;
        private long prevTicks;
        private long newTicks;
        private long delta;
        private double ratio;
        private double rotate;
        private bool isRendering = false;

        private ObservableCollection<double> values;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += new EventHandler(WindowSourceInitialized);
            //graphics = new PascalGraphics(MainCanvas, (Storyboard)FindResource("MainStoryboard"), 2);
            graphics = new PascalGraphics(MainCanvas);
            graphics.Color = PascalColors.Yellow;
            graphics.LineStyle = PascalLineStyles.Solid;
            graphics.LineThickness = PascalLineThicknesses.ThickWidth;

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!isRendering) return;

            prevTicks = newTicks;
            newTicks = Stopwatch.GetTimestamp();
            delta = newTicks - prevTicks;
            ratio = 1000d * delta / TimeSpan.TicksPerSecond;

            /*rotate -= ratio / 4;

            if (isOpening)
            {
                alpha += ratio;
                if (alpha > 45d)
                {
                    isOpening = false;
                    alpha = 2d * 45d - alpha;
                }
            }
            else
            {
                alpha -= ratio;
                if (alpha < 0d)
                {
                    isOpening = true;
                    alpha = -alpha;
                }
            }

            KeyValuePair<Point, Point> ellipseEnds = graphics.Ellipse(at, alpha + rotate, rotate + 360d - alpha, radiuses);
            graphics.Line(at, ellipseEnds.Key);
            graphics.Line(at, ellipseEnds.Value);*/

            //graphics.Render(true);
        }

        private byte[] MACToByteArray(string tmpMAC)
        {
            byte[] tmpBytes = BitConverter.GetBytes(UInt64.Parse(tmpMAC, NumberStyles.HexNumber));
            byte[] resultBytes = new byte[6];
            Array.Reverse(tmpBytes);
            Array.Copy(tmpBytes, 2, resultBytes, 0, 6);
            return resultBytes;
        }

        private void Render()
        {
            newTicks = Stopwatch.GetTimestamp();
            isRendering = !isRendering;

            List<double> numbers = new List<double>();
            double sum = 0;
            Random random = new Random();
            for (int i = 0; i < 8; ++i)
            {
                numbers.Add(random.NextDouble());
                sum += numbers[numbers.Count - 1];
            }
            numbers.Sort();
            double ratio = graphics.Width / sum, currentX = 0, currentDelta;
            foreach (double number in numbers)
            {
                currentDelta = number * ratio;
                graphics.Color = Color.FromRgb((byte)random.Next(), (byte)random.Next(), (byte)random.Next());
                graphics.Rectangle(new Point(currentX, 0), new Point(currentX + currentDelta, graphics.Height));
                currentX += currentDelta;
            }

            Point center = new Point(graphics.Width / 2, graphics.Height / 2);
            double radius = 128;
            graphics.Color = Colors.Black;
            for (int i = 0; i < 6; ++i)
            {
                graphics.Line(
                    new Point(center.X + radius * Math.Cos(graphics.ToRadians((i) * 60)), center.Y + radius * Math.Sin(graphics.ToRadians((i) * 60))),
                    new Point(center.X + radius * Math.Cos(graphics.ToRadians((i - 1) * 60)), center.Y + radius * Math.Sin(graphics.ToRadians((i - 1) * 60)))
                    );
            }
            for (int i = 0; i < 6; ++i)
            {
                graphics.Line(
                    new Point(center.X + radius * Math.Cos(graphics.ToRadians((i) * 60 + 30)), center.Y + radius * Math.Sin(graphics.ToRadians((i) * 60 + 30))),
                    new Point(center.X + radius * Math.Cos(graphics.ToRadians((i - 1) * 60 + 30)), center.Y + radius * Math.Sin(graphics.ToRadians((i - 1) * 60 + 30)))
                    );
            }

            //graphics.Graph(Algebra, new Rect(-80, -40, 160, 80));

            /*List<byte> buffer = new List<byte>();
            buffer.AddRange(MACToByteArray("FFFFFFFFFFFF"));
            byte[] computerMAC = MACToByteArray("6CF049567E90");
            for (int i = 0; i < 16; ++i) buffer.AddRange(computerMAC);
            (new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)).SendTo(buffer.ToArray(), new IPEndPoint(IPAddress.Parse("77.93.17.69"), 9));*/

            /*int numbers = 6;
            double maxValue = 6;
            double offset = 24;
            double realWidth = graphics.Width - offset * 2;
            double realHeight = graphics.Height - offset * 2;
            double heightRatio = realHeight / maxValue;
            double widthRatio = realWidth / numbers;

            for (int i = 1; i <= numbers; ++i)
            {
                DoIt(new Point(offset + widthRatio * (i - 1), realHeight + offset), heightRatio * i);
            }*/

            /*for (int i = 1; i < 4; ++i)
            {
                for (int j = 1; j < 4; ++j)
                {
                    graphics.Color = PascalColors.White;
                    graphics.Rectangle(new Point(i * 200, j * 200), new Point(i * 200 + 100, j * 200 + 50));
                    graphics.Color = PascalColors.Blue;
                    graphics.Rectangle(new Point(i * 200 + 20, j * 200), new Point(i * 200 + 30, j * 200 + 50));
                    graphics.Rectangle(new Point(i * 200, j * 200 + 20), new Point(i * 200 + 100, j * 200 + 30));
                }
            }

            graphics.Color = PascalColors.White;
            graphics.Ellipse(new Point(900, 200), 0, 359, new Size(100, 100));
            graphics.Color = PascalColors.Blue;
            Point tmpPoint;
            for (int i = 0; i < 360; i += 30)
            {
                tmpPoint = new Point(895 + Math.Cos(graphics.ToRadians(i)) * 120, 195 + Math.Sin(graphics.ToRadians(i)) * 120);
                graphics.Rectangle(tmpPoint, new Point(tmpPoint.X + 10, tmpPoint.Y + 10));
            }*/

            /*values = new ObservableCollection<double>();
            Random tmpRandom = new Random();
            for (int i = tmpRandom.Next(24) + 8; i > 0; --i) values.Add(tmpRandom.NextDouble());

            double distance = (graphics.Width - 48) / (values.Count - 1);
            double realHeight = graphics.Height - 48;
            double currentX = 24;
            double currentY = graphics.Height - 24;
            bool first = true;

            Point topLeft;
            Point bottomRight;

            foreach (double value in values)
            {
                topLeft = new Point(currentX - 4, currentY - value * realHeight);
                bottomRight = new Point(currentX + 4, currentY);

                graphics.Rectangle(topLeft, bottomRight);
                graphics.Text(value.ToString("0.00"), new Point(topLeft.X, topLeft.Y - 16));
                if (!first) graphics.LineTo(new Point(currentX, topLeft.Y));
                else
                {
                    first = false;
                    graphics.MoveTo(new Point(currentX, topLeft.Y));
                }
                currentX += distance;
            }*/

            graphics.Render(false);

            //graphics.Rectangle(new Point(10, 10), new Point(500, 500));
            //graphics.Ellipse(new Point(200, 200), 0, 270, new Size(150, 100));
            //graphics.Graph(Algebra, new Rect(-20, -1.5, 40, 3));
        }

        public double? Algebra(double x)
        {
            if (x == 0) return 0;
            double tmp = (Math.Sin(x));
            double tmp2 = 100d / x;
            tmp += tmp2;
            //if (tmp > 10000) tmp = 10;
            //else if (tmp < -100) tmp = -10;
            return tmp;
            //return x * x;
            //return (Math.Sin(x));
        }

        private void DoIt(Point at, double c)
        {
            graphics.MoveTo(new Point(at.X - 8, at.Y));
            graphics.LineTo(new Point(at.X, at.Y - c));
            graphics.LineTo(new Point(at.X + 8, at.Y));
            graphics.LineTo(new Point(at.X - 8, at.Y));

            /*graphics.MoveTo(at);

            graphics.LineStyle = PascalLineStyles.Solid;
            graphics.LineRelative(new Point(-30 * c, -40 * c));
            Point A = new Point(graphics.Position.X, graphics.Position.Y);
            graphics.LineRelative(new Point(20 * c, -60 * c));
            Point B = new Point(graphics.Position.X, graphics.Position.Y);
            graphics.LineRelative(new Point(60 * c, 10 * c));
            Point C = new Point(graphics.Position.X, graphics.Position.Y);
            graphics.LineRelative(new Point(-20 * c, 60 * c));
            Point D = new Point(graphics.Position.X, graphics.Position.Y);
            graphics.LineTo(at);
            graphics.Line(D, A);

            graphics.LineStyle = PascalLineStyles.Dotted;
            graphics.Line(B, at);
            graphics.Line(C, at);*/
        }

        private void Clear()
        {
            graphics.ClearCanvas();
        }

        private void Close()
        {
            Application.Current.Shutdown();
        }

        private void RenderClick(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RenderCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Render();
        }

        private void ClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Clear();
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        // Maximize-related

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        private static System.IntPtr WindowProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [DllImport("User32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
    }

    public static class Commands
    {
        public static readonly RoutedUICommand Render = new RoutedUICommand();
        public static readonly RoutedUICommand Clear = new RoutedUICommand();
        public static readonly RoutedUICommand Close = new RoutedUICommand();
    }

    // Maximize-related

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

        public RECT rcMonitor = new RECT();

        public RECT rcWork = new RECT();

        public int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public static readonly RECT Empty = new RECT();

        public int Width
        {
            get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
        }

        public int Height
        {
            get { return bottom - top; }
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(RECT rcSrc)
        {
            this.left = rcSrc.left;
            this.top = rcSrc.top;
            this.right = rcSrc.right;
            this.bottom = rcSrc.bottom;
        }

        public bool IsEmpty
        {
            get
            {
                return left >= right || top >= bottom;
            }
        }

        public override string ToString()
        {
            if (this == RECT.Empty) { return "RECT {Empty}"; }
            return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rect)) { return false; }
            return (this == (RECT)obj);
        }

        public override int GetHashCode()
        {
            return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
        }

        public static bool operator ==(RECT rect1, RECT rect2)
        {
            return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
        }

        public static bool operator !=(RECT rect1, RECT rect2)
        {
            return !(rect1 == rect2);
        }
    }
}
