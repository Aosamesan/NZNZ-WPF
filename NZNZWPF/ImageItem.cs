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

            return image;
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
        public bool Contains(string url)
        {
            foreach (var item in Items)
                if (item.URL == url)
                    return true;
            return false;
        }
    }
}
