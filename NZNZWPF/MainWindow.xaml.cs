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
using System.Media;
using System.IO.Compression;


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
        private Action ClearList;
        private Func<string, bool> Contains;
        private Action<ImageItem> InsertItem;

        private DoubleAnimation appearAnimation;
        
        private FavoriteCollection favoriteColleciton;

        private static readonly string FileFilter =
            @"JPG File(*.jpg,*.jpeg)|*jpg,*jpeg|GIF File(*.gif)|*.gif|PNG File(*.png)|*.png|Bitmap File(*.bmp)|*.bmp";
        private static string DirectoryLocation;

        #region Prevent new Window
        static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        #endregion

        static MainWindow()
        {
            DirectoryLocation = null;
        }

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


            MainWebBrowser.Navigating += onNavigating;
            MainWebBrowser.Navigated += onNavigated;
            MainWebBrowser.LoadCompleted += MyWebBrowser_LoadCompleted;

            collection = Resources["ImageItemsKey"] as ImageItemCollection;

            #region MethodInvoker
            GetURL = new Func<string>(() => { return URLTextBox.Text; });

            ClearList = new Action(() =>
            {
                collection.Clear();
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

            favoriteColleciton = Application.Current.Resources["FavoriteCollection"] as FavoriteCollection;
            if (favoriteColleciton == null)
                throw new NullReferenceException();
        }

        private void onNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (ParseButton.IsEnabled)
                ParseButton.IsEnabled = false;
            if (AppendButton.IsEnabled)
                AppendButton.IsEnabled = false;
        }

        private void ParseStart(object obj)
        {
            if (!(obj is bool))
                return;
            bool isAppend = (bool)obj;

            try {
                using (WebClientWithTimeOut request = new WebClientWithTimeOut(3000))
                {
                    using (StreamReader sr = new StreamReader(request.OpenRead(Dispatcher.Invoke(GetURL))))
                    {
                        string html = sr.ReadToEnd();
                        html = html.Replace("\\\\", "\\");

                        var list = imageParser.GetImageSources(ref html);

                        if (list?.Count > 0)
                        {
                            if (!isAppend)
                                Dispatcher.Invoke(ClearList);

                            foreach (var item in list)
                            {
                                if (!(bool)Dispatcher.Invoke(Contains, new object[] { item.ToString() }))
                                    Dispatcher.Invoke(InsertItem, new object[] { item });
                            }
                        }
                    }
                }

                SystemSounds.Beep.Play();
            }
            catch(Exception e)
            {

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

            ParseButton.IsEnabled = true;
            AppendButton.IsEnabled = true;
        }



        void OnWebBrowserNewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed)
        {
            Processed = true;

            MainWebBrowser.Navigate(URL);
        }
        #endregion

        private void FavButton_Click(object sender, RoutedEventArgs e)
        {
            FavortiesDialog dlg = new FavortiesDialog();

            if(dlg.ShowDialog() == true)
            {
                MainWebBrowser.Navigate(dlg.SelectedItem.URL);
            }
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
            try
            {
                MainWebBrowser.Navigate(url);
            }
            catch (Exception e)
            {
                url = "http://google.com/search?q=" + HttpUtility.UrlEncode(url);
            }
            finally
            {
                MainWebBrowser.Navigate(url);
            }
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
                    MessageBox.Show("레지스트리가 등록되었습니다. 관리자 권한 해제 후 다시 실행해주세요.");
                }
                catch (System.Security.SecurityException e)
                {
                    MessageBox.Show("관리자 권한으로 한 번 실행해 주세요.");
                }
                Close();
            }
        }

        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListView.SelectedItem == null)
                return;

            var item = ImageListView.SelectedItem as ImageItem;

            if(item != null)
            {
                ImageView.Source = item.OriginImage;
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
            ParseStart(false);
        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {
            ParseStart(true);
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            ImageItem item = ImageListView.SelectedItem as ImageItem;

            if(item != null)
            {
                Clipboard.SetImage(item.OriginImage);
            }
            else
            {
                MessageBox.Show("NULL");
            }
        }

        private void DelClick(object sender, RoutedEventArgs e)
        {
            List<string> selectedURLs;

            if (ImageListView.SelectedItems?.Count < 1)
                return;

            selectedURLs = new List<string>();
            
            foreach(ImageItem item in ImageListView.SelectedItems)
            {
                selectedURLs.Add(item.ToString());
            }

            foreach(var url in selectedURLs)
            {
                ImageItem item = collection.FindByURL(url);
                collection.Remove(item);
            }
        }

        private void DelLessSize(double width, double height, bool isAnd = true)
        {
            List<string> urls = new List<string>();
            Func<ImageItem, bool> comparer;

            if (isAnd)
            {
                comparer = (item) => item.OriginImage.Width < width && item.OriginImage.Height < height;
            }
            else
            {
                comparer = (item) => item.OriginImage.Width < width || item.OriginImage.Height < height;
            }
            foreach (ImageItem item in ImageListView.Items)
            {
                if (comparer(item))
                    urls.Add(item.URL);
            }

            foreach (var url in urls)
            {
                ImageItem item = collection.FindByURL(url);
                collection.Remove(item);
            }
        }

        private void SetDirectoryLocation(string filePath)
        {
            string parentPath = Directory.GetParent(filePath).FullName;
            if (DirectoryLocation != null || DirectoryLocation != parentPath)
                DirectoryLocation = parentPath;
        }

        private void SaveOneButton_Click(object sender, RoutedEventArgs e)
        {
            ImageItem item = ImageListView.SelectedItem as ImageItem;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = FileFilter;
            dlg.InitialDirectory = DirectoryLocation == null ? "%USERPROFILE%" : DirectoryLocation; 
            dlg.FileName = item.FileName;

            if(dlg.ShowDialog() == true)
            {
                SaveImageFile(item, dlg.FileName);
            }
        }

        private void SaveImageFile(ImageItem item,string filePath)
        {
            string extension = filePath.Split('.').Last();

            try
            {
                using(MemoryStream ms = SaveImageToStream(item, extension))
                {
                    using(FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        byte[] bytes = ms.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch(FileFormatException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            SetDirectoryLocation(filePath);
            
            
        }

        private MemoryStream SaveImageToStream(ImageItem item, string extension)
        {
            BitmapEncoder encoder = null;
            MemoryStream ms;

            switch (extension.ToUpper())
            {
                case "JPG":
                case "JPEG":
                    encoder = new JpegBitmapEncoder();
                    break;
                case "GIF":
                    encoder = new GifBitmapEncoder();
                    break;
                case "PNG":
                    encoder = new PngBitmapEncoder();
                    break;
                case "BMP":
                    encoder = new BmpBitmapEncoder();
                    break;
                default:
                    throw new FileFormatException("Unknown Extension");
            }
            
            encoder.Frames.Add(BitmapFrame.Create(item.OriginImage));
            ms = new MemoryStream();
            encoder.Save(ms);

            return ms;
        }

        private void SaveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListView.SelectedItems?.Count < 2)
                return;

            var c = ImageListView.SelectedItems;
            System.Windows.Forms.FolderBrowserDialog fdlg = new System.Windows.Forms.FolderBrowserDialog();
            fdlg.SelectedPath = DirectoryLocation == null ? "%USERPROFILE%" : DirectoryLocation;

            if(fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolder = fdlg.SelectedPath;

                foreach(ImageItem item in c)
                {
                    SaveImageFile(item, selectedFolder + "\\" + item.FileName);
                }
            }
        }

        private void SaveSelectedZipButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListView.SelectedItems == null ||
                ImageListView.SelectedItems.Count < 2)
                return;

            ZipArchive zipArchive;
            SaveFileDialog dlg = new SaveFileDialog();
            var c = ImageListView.SelectedItems;

            dlg.Filter = "Zip File(*.zip)|*.zip";
            dlg.InitialDirectory = DirectoryLocation == null ? "%USERPROFILE%" : DirectoryLocation;

            if (dlg.ShowDialog() == true)
            {
                string filepath = dlg.FileName;

                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    zipArchive = new ZipArchive(fs, ZipArchiveMode.Create);
                    
                    foreach(ImageItem item in c)
                    {
                        var entry = zipArchive.CreateEntry(item.FileName);
                        string extension = item.FileName.Split('.').Last();

                        using(Stream stream = entry.Open())
                        {
                            using(MemoryStream ms = SaveImageToStream(item, extension))
                            {
                                byte[] bytes = ms.ToArray();
                                stream.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }
                }
            }

        }

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListView.Items?.Count < 2)
                return;

            var c = ImageListView.Items;
            System.Windows.Forms.FolderBrowserDialog fdlg = new System.Windows.Forms.FolderBrowserDialog();
            fdlg.SelectedPath = DirectoryLocation == null ? "%USERPROFILE%" : DirectoryLocation;

            if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolder = fdlg.SelectedPath;

                foreach (ImageItem item in c)
                {
                    SaveImageFile(item, selectedFolder + "\\" + item.FileName);
                }
            }
        }

        private void SaveAllZipButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListView.Items?.Count < 2)
                return;

            ZipArchive zipArchive;
            SaveFileDialog dlg = new SaveFileDialog();
            var c = ImageListView.Items;

            dlg.Filter = "Zip File(*.zip)|*.zip";
            dlg.InitialDirectory = DirectoryLocation == null ? "%USERPROFILE%" : DirectoryLocation;

            if (dlg.ShowDialog() == true)
            {
                string filepath = dlg.FileName;

                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    zipArchive = new ZipArchive(fs, ZipArchiveMode.Create);

                    foreach (ImageItem item in c)
                    {
                        var entry = zipArchive.CreateEntry(item.FileName);
                        string extension = item.FileName.Split('.').Last();

                        using (Stream stream = entry.Open())
                        {
                            using (MemoryStream ms = SaveImageToStream(item, extension))
                            {
                                byte[] bytes = ms.ToArray();
                                stream.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }
                }
            }
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            SizeDialog dlg = new SizeDialog();

            if(dlg.ShowDialog() == true)
            {
                DelLessSize(dlg.InputWidth, dlg.InputHeight, dlg.IsAnd);
            }
        }

        private void AddFavButton_Click(object sender, RoutedEventArgs e)
        {
            FavoriteAddDialog dlg = new FavoriteAddDialog();

            if(dlg.ShowDialog(((dynamic)MainWebBrowser.Document).Title, MainWebBrowser.Source.ToString()) == true)
            {
                FavoriteItem item = new FavoriteItem(dlg.FavTitle, dlg.FavURL);
                favoriteColleciton.Add(item);
            }
        }

        private void NotImageDelButton_Click(object sender, RoutedEventArgs e)
        {
            DelLessSize(2, 2, false);
        }

        private void ImageListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ImageItem item = ImageListView.SelectedItem as ImageItem;

            if(item != null)
            {
                ImageFile dlg = new ImageFile();

                if(dlg.ShowDialog(item) == true)
                {
                    if (!collection.IsUniqueFileName(item))
                    {
                        MessageBox.Show("이름이 중복되었습니다 : " + item.FileName);
                        item.FileName = dlg.OriginFileName;
                    }
                }
            }

        }

        private void ItemNumbering_Click(object sender, RoutedEventArgs e)
        {
            collection.FileNameNumbering(false);
        }
    }
}