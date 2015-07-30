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

namespace NZNZWPF
{
    class ImageItem : INotifyPropertyChanged
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

        public ImageItem(string url)
        {
            URL = url;
            FileName = URL.Split('/').Last();
        }


        private BitmapImage imageFromUrl(string url)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
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


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    class ImageItemCollection : ObservableCollection<ImageItem>
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

        public bool Contains(string url)
        {
            foreach (var item in Items)
                if (item.URL == url)
                    return true;
            return false;
        }
    }
}
