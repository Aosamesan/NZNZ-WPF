using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NZNZWPF
{
    class WindowResizer
    {
        #region private member field
        HwndSource hwndSource;
        private const int WM_SYSCOMMAND = 0x112;
        Window activeWin;
        #endregion

        #region Import SendMessage
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region ResizeDirection Enum
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
        #endregion

        #region Constructor
        public WindowResizer(Window window)
        {
            activeWin = window as Window;

            activeWin.SourceInitialized += (sender, e) =>
            {
                hwndSource = PresentationSource.FromVisual(sender as Visual) as HwndSource;
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            };
        }
        #endregion

        #region Debug
        [Conditional("DEBUG")]
        private void DebuggingMessage(int msg)
        {
            Debug.WriteLine("WndProc messages: " + msg.ToString());
        }
        #endregion

        #region private member method
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            DebuggingMessage(msg);

            if (msg == WM_SYSCOMMAND)
            {
                DebuggingMessage(msg);
            }

            return IntPtr.Zero;
        }

        private void resizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        #endregion

        #region public member method
        public void ResizeWindow(object sender)
        {
            Rectangle clickedRectangle = sender as Rectangle;

            switch (clickedRectangle.Name)
            {
                case "top":
                    activeWin.Cursor = Cursors.ScrollN;
                    resizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    activeWin.Cursor = Cursors.ScrollS;
                    resizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    activeWin.Cursor = Cursors.ScrollW;
                    resizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    activeWin.Cursor = Cursors.ScrollE;
                    resizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    activeWin.Cursor = Cursors.ScrollNW;
                    resizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    activeWin.Cursor = Cursors.ScrollNE;
                    resizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    activeWin.Cursor = Cursors.ScrollSW;
                    resizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    activeWin.Cursor = Cursors.ScrollSE;
                    resizeWindow(ResizeDirection.BottomRight);
                    break;
                default:
                    break;
            }

            activeWin.WindowState = WindowState.Normal;
        }

        public void DisplayResizeCursor(object sender)
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


        public void ResetCursor()
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                activeWin.Cursor = Cursors.Arrow;
            }
        }
        #endregion
    }
}
