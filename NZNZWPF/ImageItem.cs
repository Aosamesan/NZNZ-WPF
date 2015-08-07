using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;

namespace NZNZWPF
{
    public class ImageItem : INotifyPropertyChanged
    {
        private string url;
        private string filename;
        private BitmapImage originImage;

        public string URL
        {

            get { return url; }
            set
            {
                url = value;
                FileName = URL.Split('/').Last();
                OnPropertyChanged("URL");

                OriginImage = imageFromUrl(URL);
            }
        }

        public BitmapImage OriginImage
        {
            get { return originImage; }
            private set
            {
                originImage = value;
                OnPropertyChanged("OriginImage");
                OnPropertyChanged("Width");
                OnPropertyChanged("Height");
            }
        }

        public double Width
        {
            get
            {
                return OriginImage.Width;
            }
        }

        public double Height
        {
            get
            {
                return OriginImage.Height;
            }
        }

        public string FileName
        {
            get { return filename; }
            set
            {
                filename = value;
                OnPropertyChanged("FileName");
            }
        }

        public ImageItem()
        {

        }

        public ImageItem(string url)
        {
            URL = url;
            if (string.IsNullOrWhiteSpace(URL.Split('.').Last()))
            {
                FileName += ".png";
            }
        }


        private BitmapImage imageFromUrl(string url)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
            image.DownloadCompleted += (sender, e) => OnPropertyChanged("OriginImage");
            image.EndInit();

            BitmapImage local = image.Clone();

            return local;
        }

        public override string ToString()
        {
            return URL;
        }

        public override bool Equals(object obj)
        {
            ImageItem item = obj as ImageItem;
            
            if (item == null)
                return false;
            return item.URL == URL;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class ImageItemCollection : ObservableCollection<ImageItem>
    {
        private int minWidth;
        private int minHeight;

        public ImageItemCollection()
        {
            MinWidth = 300;
            MinHeight = 300;
        }

        public ImageItemCollection(int minWidth = 300, int minHeight = 300)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
        }

        public int MinWidth
        {
            get { return minWidth; }
            set
            {
                if (value > 100)
                {
                    minWidth = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("MinWidth"));
                }
            }
        }

        public int MinHeight
        {
            get { return minHeight; }
            set
            {
                if (value > 100)
                {
                    minHeight = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("MinHeight"));
                }
            }
        }

        public ImageSource GetByURL(string url)
        {
            foreach (ImageItem item in Items)
                if (item.URL == url)
                    return item.OriginImage;
            return null;
        }

        public ImageItem FindByURL(string url)
        {
            foreach (ImageItem item in Items)
                if (item.URL == url)
                    return item;
            return null;
        }

        public bool Contains(string url)
        {
            foreach (var item in Items)
                if (item.URL == url)
                    return true;
            return false;
        }

        public new void Add(ImageItem item)
        {
            if (!Contains(item))
                base.Add(item);
        }

        public bool IsUniqueFileName(ImageItem selectedItem)
        {
            foreach (var item in Items)
                if (item.FileName == selectedItem.FileName && item != selectedItem)
                    return false;

            return true;
        }

        public void FileNameNumbering(bool isZeroFill, string prefix = "", string postfix = "")
        {
            int count = Items.Count;
            int digits = Convert.ToInt32(Math.Ceiling(Math.Log10(count)));
            string format = null;
            string extension = null;
            int num = 1;

            if (isZeroFill)
                format = "{0}{1:D" + digits + "}{2}.{3}";
            else
                format = "{0}{1}{2}.{3}";

            MessageBox.Show(digits.ToString());
            foreach(var item in Items)
            {
                extension = item.FileName.Split('.').Last();
                item.FileName = string.Format(format, prefix, num++, postfix, extension);
            }
        }
    }
}
