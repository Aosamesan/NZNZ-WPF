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
using System.ComponentModel;
using System.Threading;
using System.Xml;
using System.Web;
using System.Net;
using System.Net.Http;
using Microsoft.Win32;
using mshtml;
using System.Text.RegularExpressions;
using System.IO;

namespace NZNZWPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowResizer resizer;
        private ISourceParser sourceParser;
        private ImageParser imageParser;
        private ImageItemCollection collection;
        
        public MainWindow()
        {
            SetRegistry();
            InitializeComponent();

            #region Hide Script Error
            MainWebBrowser.Navigated += (sender, e) => HideScriptErrors(sender as WebBrowser, true);
            #endregion

            SetMenuButtons();

            sourceParser = new SourceParser();
            imageParser = new ImageParser(sourceParser);

            resizer = new WindowResizer(this);

            MainWebBrowser.Navigated += onNavigated;

            // onLoadComplete의 내용은 Parse버튼으로 옮겨야함
            MainWebBrowser.LoadCompleted += onLoadComplete;

            collection = Resources["ImageItemsKey"] as ImageItemCollection;
        }

        private void onLoadComplete(object sender, EventArgs e)
        {
            WebClientWithTimeOut request = new WebClientWithTimeOut(3000);
            StreamReader sr = new StreamReader(request.OpenRead(URLTextBox.Text));
            string html = sr.ReadToEnd();

            var list = imageParser.GetImageSources(ref html);

            if (list == null)
                return;

            collection.Clear();

            foreach (var item in list)
            {
                if (!collection.Contains(item))
                    collection.Add(item);
            }
        }

        private void onNavigated(object sender, NavigationEventArgs e)
        {
            SetNavButtonEnabled();

            URLTextBox.Text = MainWebBrowser.Source.ToString();
        }

        private void SetNavButtonEnabled()
        {
            PrevButton.IsEnabled = MainWebBrowser.CanGoBack;
            NextButton.IsEnabled = MainWebBrowser.CanGoForward;
        }

        private void SetMenuButtons()
        {
            MinimizeButton.Click += (sender, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (sender, e) => WindowState =
                WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (sender, e) => Close();
        }
        
        #region For Window Resize

        private void ResizeDrag(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            if(e.LeftButton == MouseButtonState.Pressed)
                resizer.ResizeWindow(senderRect);
        }
        
        private void ResizeMouseEnter(object sender, MouseEventArgs e)
        {
            resizer.DisplayResizeCursor(sender);
        }

        private void ResizeMouseLeave(object sender, MouseEventArgs e)
        {
            resizer.ResetCursor();
        }
        
        private void ResizeMouseUp(object sender, MouseButtonEventArgs e)
        {
            resizer.ResetCursor();
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
            Navigate();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if(MainWebBrowser.CanGoBack)
                MainWebBrowser.GoBack();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWebBrowser.CanGoForward)
                MainWebBrowser.GoForward();
        }

        private void URLTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Navigate();
        }

        private void Navigate()
        {
            string url = URLTextBox.Text;
            if(!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                url = "http://google.com/search?q=" + HttpUtility.UrlEncode(url);
                MessageBox.Show(url.ToString());
            }
            
            MainWebBrowser.Navigate(url);
        }

        private void SetRegistry()
        {
            string installKey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
            string entryLabel = System.AppDomain.CurrentDomain.FriendlyName;

            System.OperatingSystem osInfo = System.Environment.OSVersion;
            string version = osInfo.Version.Major.ToString() + '.' + osInfo.Version.Minor.ToString();
            uint editFlag = (uint)((version == "6.2") ? 0x2710 : 0x2328); // 6.2 = Windows 8 and therefore IE10

            RegistryKey existingSubKey = Registry.LocalMachine.OpenSubKey(installKey, false);

            if (existingSubKey.GetValue(entryLabel) == null)
            {
                try
                {
                    existingSubKey = Registry.LocalMachine.OpenSubKey(installKey, true); // writable key
                    existingSubKey.SetValue(entryLabel, unchecked((int)editFlag), RegistryValueKind.DWord);
                }
                catch (System.Security.SecurityException e)
                {
                }
            }
        }

        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListView.SelectedItem == null)
                return;

            string item = ImageListView.SelectedItem.ToString();

            if(item != null)
            {
                ImageItem imageItem = new ImageItem(item);
                ImageView.Source = imageItem.OriginImage;
            }
            else
            {
                MessageBox.Show("item is null");
            }
        }

        private void ImageView_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Image imageView = sender as Image;
            ImageSource bi = imageView.Source;

            if (imageView.ActualHeight < bi.Height && imageView.ActualWidth < bi.Width)
                imageView.Stretch = Stretch.Uniform;
            else
                imageView.Stretch = Stretch.None;
        }
    }
}