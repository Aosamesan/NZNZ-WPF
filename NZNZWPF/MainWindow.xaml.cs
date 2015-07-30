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
using System.Windows.Media.Animation;
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


        private Func<string> GetURL;
        private Func<double> GetProgressBarValue;
        private Action<double> SetProgressBarValue;
        private Action ClearList;
        private Func<string, bool> Contains;
        private Action<ImageItem> InsertItem;

        private DoubleAnimation appearAnimation;
        private DoubleAnimation disappearAnimation;

        private ImageItemCollection cacheCollection;

        #region Prevent new Window
        static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        #endregion

        public MainWindow()
        {
            SetRegistry();
            InitializeComponent();
            MainWindowLoad();
        }

        public void MainWindowLoad()
        {
            #region Hide Script Error
            MainWebBrowser.Navigated += (sender, e) => HideScriptErrors(sender as WebBrowser, true);
            #endregion

            SetMenuButtons();

            sourceParser = new SourceParser();
            imageParser = new ImageParser(sourceParser);

            resizer = new WindowResizer(this);

            MainWebBrowser.Navigated += onNavigated;

            MainWebBrowser.LoadCompleted += MyWebBrowser_LoadCompleted;

            collection = Resources["ImageItemsKey"] as ImageItemCollection;
            cacheCollection = new ImageItemCollection();


            #region MethodInvoker
            GetURL = new Func<string>(() => { return URLTextBox.Text; });
            GetProgressBarValue = new Func<double>(() => { return ParseProgressBar.Value; });
            SetProgressBarValue = new Action<double>((v) => { ParseProgressBar.Value = v; });

            ClearList = new Action(() =>
            {
                collection.Clear();
                cacheCollection.Clear();
            });
            Contains = new Func<string, bool>((url) => { return collection.Contains(url); });
            InsertItem = new Action<ImageItem>((item) => collection.Add(item));
            #endregion


            #region Animation
            appearAnimation = new DoubleAnimation();
            appearAnimation.BeginTime = TimeSpan.FromSeconds(0.5);
            appearAnimation.From = 1;
            appearAnimation.To = 0.5;
            appearAnimation.Duration = new Duration(TimeSpan.FromSeconds(2.5));
            appearAnimation.AutoReverse = true;
            #endregion
        }

        private void ParseStart(object obj)
        {
            if (!(obj is bool))
                return;
            bool isAppend = (bool)obj;

            using(WebClientWithTimeOut request = new WebClientWithTimeOut(3000))
            {
                using (StreamReader sr = new StreamReader(request.OpenRead(Dispatcher.Invoke(GetURL))))
                {
                    string html = sr.ReadToEnd();

                    Dispatcher.Invoke(SetProgressBarValue, new object[] { 5 });
                    var list = imageParser.GetImageSources(ref html);
                    Dispatcher.Invoke(SetProgressBarValue, new object[] { 50 });

                    if (list != null && list.Count > 0)
                    {
                        if(!isAppend)
                            Dispatcher.Invoke(ClearList);

                        double d = list.Count;
                        int i = 0;

                        foreach (var item in list)
                        {
                            d += 50 * Convert.ToDouble(i++) / d;
                            if (!(bool)Dispatcher.Invoke(Contains, new object[] { item.ToString() }))
                                Dispatcher.Invoke(InsertItem, new object[] { item });
                            double now = Dispatcher.Invoke(GetProgressBarValue);
                            Dispatcher.Invoke(SetProgressBarValue, new object[] { now + d });
                        }
                    }
                }
            }

            Dispatcher.Invoke(SetProgressBarValue, new object[] { 100 });
            Thread.Sleep(500);
            Dispatcher.Invoke(SetProgressBarValue, new object[] { 0 });
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

        #region Prevent New Window
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }

        void MyWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            IServiceProvider serviceProvider = (IServiceProvider)MainWebBrowser.Document;
            Guid serviceGuid = SID_SWebBrowserApp;
            Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;
            SHDocVw.IWebBrowser2 myWebBrowser2 = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);
            SHDocVw.DWebBrowserEvents_Event wbEvents = (SHDocVw.DWebBrowserEvents_Event)myWebBrowser2;
            wbEvents.NewWindow += new SHDocVw.DWebBrowserEvents_NewWindowEventHandler(OnWebBrowserNewWindow);
        }



        void OnWebBrowserNewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed)
        {
            Processed = true;

            MainWebBrowser.Navigate(URL);
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
                    MessageBox.Show(e.Message);
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
                ImageSource bi;
                if (!cacheCollection.Contains(item))
                {
                    bi = new BitmapImage();
                    (bi as BitmapImage).BeginInit();
                    (bi as BitmapImage).UriSource = new Uri(item, UriKind.Absolute);
                    (bi as BitmapImage).EndInit();
                    
                }
                else
                {
                    bi = cacheCollection.GetByURL(item);
                }
                ImageView.Source = bi;

                // ImageView.BeginAnimation(OpacityProperty, appearAnimation);
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

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(ParseStart));

            thread.Start(false);
        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(ParseStart));

            thread.Start(true);
        }
    }
}