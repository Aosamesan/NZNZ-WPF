using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NZNZWPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region For Resize
        HwndSource hwndSource;
        private const int WM_SYSCOMMAND = 0x112;
        Window activeWin;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            #region For Resize
            activeWin = this as Window;

            activeWin.SourceInitialized += (sender, e) =>
            {
                hwndSource = PresentationSource.FromVisual(sender as Visual) as HwndSource;
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            };
            #endregion

            #region Hide Script Error
            MainWebBrowser.Navigated += (sender, e) => HideScriptErrors(sender as WebBrowser, true);
            #endregion

            SetMenuButtons();
        }

        private void SetMenuButtons()
        {
            MinimizeButton.Click += (sender, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (sender, e) => WindowState =
                WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (sender, e) => Close();
        }
        
        #region For Window Resize
        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Debug.WriteLine("WndProc messages: " + msg.ToString());
            //
            // Check incoming window system messages
            //
            if (msg == WM_SYSCOMMAND)
            {
                Debug.WriteLine("WndProc messages: " + msg.ToString());
            }

            return IntPtr.Zero;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        public void resizeWindow(object sender)
        {
            Rectangle clickedRectangle = sender as Rectangle;

            switch (clickedRectangle.Name)
            {
                case "top":
                    activeWin.Cursor = Cursors.ScrollN;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    activeWin.Cursor = Cursors.ScrollS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    activeWin.Cursor = Cursors.ScrollW;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    activeWin.Cursor = Cursors.ScrollE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    activeWin.Cursor = Cursors.ScrollNW;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    activeWin.Cursor = Cursors.ScrollNE;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    activeWin.Cursor = Cursors.ScrollSW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    activeWin.Cursor = Cursors.ScrollSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                default:
                    break;
            }

            WindowState = WindowState.Normal;
        }


        public void displayResizeCursor(object sender)
        {

            Rectangle clickedRectangle = sender as Rectangle;

            switch (clickedRectangle.Name)
            {
                case "top":
                    activeWin.Cursor = Cursors.ScrollN;
                    break;
                case "bottom":
                    activeWin.Cursor = Cursors.ScrollS;
                    break;
                case "left":
                    activeWin.Cursor = Cursors.ScrollW;
                    break;
                case "right":
                    activeWin.Cursor = Cursors.ScrollE;
                    break;
                case "topLeft":
                    activeWin.Cursor = Cursors.ScrollNW;
                    break;
                case "topRight":
                    activeWin.Cursor = Cursors.ScrollNE;
                    break;
                case "bottomLeft":
                    activeWin.Cursor = Cursors.ScrollSW;
                    break;
                case "bottomRight":
                    activeWin.Cursor = Cursors.ScrollSE;
                    break;
                default:
                    break;
            }
            
        }

        public void resetCursor()
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                activeWin.Cursor = Cursors.Arrow;
            }
        }

        private void ResizeDrag(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
                resizeWindow(sender);
        }
        
        private void ResizeMouseEnter(object sender, MouseEventArgs e)
        {
            displayResizeCursor(sender);
        }

        private void ResizeMouseLeave(object sender, MouseEventArgs e)
        {
            resetCursor();
        }
        
        private void ResizeMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                activeWin.Cursor = Cursors.Arrow;
            }
        }
        #endregion


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ?
                    WindowState.Normal : WindowState.Maximized;
            }
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        #region Suppress Script Error for WebBrowser
        public void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
        #endregion

        private void FavButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Favorties");
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            Uri dest = null;
            try
            {
                string url = URLTextBox.Text;
                if (!url.Contains("http://") || !url.Contains("https://"))
                    url = "https://" + url;
                dest = new Uri(url);
            }
            catch (Exception ex)
            {
                dest = new Uri("http://google.com");
            }
            MainWebBrowser.Navigate(dest);
        }
    }
}