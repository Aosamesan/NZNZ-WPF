using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml;

namespace NZNZWPF
{
    public class FavoriteCollection : ObservableCollection<FavoriteItem>
    {
        private FavoriteXML favXML;

        public FavoriteCollection() : base()
        {
            // Dummy Data
            favXML = new FavoriteXML();

            var list = favXML.GetLinks();

            foreach(FavoriteItem item in list)
            {
                base.Add(item);
            }
        }

        public new void Add(FavoriteItem item)
        {
            foreach(var t in Items)
            {
                if (t.URL == item.URL)
                    return;
            }

            favXML.AddLink(item.Title, item.URL);
            base.Add(item);
        }

        public new void Remove(FavoriteItem item)
        {
            favXML.DeleteLink(item.URL);
            base.Remove(item);
        }
    }

    public class FavoriteXML
    {
        private static readonly string FileName;
        private static readonly string RootName;
        private static readonly string LinkName;
        private static readonly string TitleAttribute;
        private static readonly string URLAttribute;
        private XmlDocument doc;
        private XmlNode rootNode;

        static FavoriteXML()
        {
            FileName = "fav.nico.niconi";
            RootName = "FavLinks";
            LinkName = "Link";
            TitleAttribute = "Title";
            URLAttribute = "Url";
        }

        public FavoriteXML()
        {
            doc = new XmlDocument();

            try
            {
                doc.Load(FileName);
                rootNode = doc.DocumentElement;
            }
            catch(System.IO.FileNotFoundException e)
            {
                XmlDeclaration declare = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(declare);
                rootNode = doc.CreateElement(RootName);
                doc.AppendChild(rootNode);
            }
        }

        public void AddLink(string title, string url)
        {
            XmlNode link = doc.CreateElement(LinkName);
            XmlAttribute titleAttr = doc.CreateAttribute(TitleAttribute);
            titleAttr.Value = title;
            XmlAttribute urlAttr = doc.CreateAttribute(URLAttribute);
            urlAttr.Value = url;
            link.Attributes.Append(titleAttr);
            link.Attributes.Append(urlAttr);
            rootNode.AppendChild(link);
        }

        public void DeleteLink(string url)
        {
            if(rootNode.ChildNodes.Count > 0)
            {
                foreach(XmlNode node in rootNode.ChildNodes)
                {
                    if(node.Name == LinkName)
                    {
                        if(node.Attributes[URLAttribute].Value == url)
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }
            }
        }

        public List<FavoriteItem> GetLinks()
        {
            List<FavoriteItem> list = new List<FavoriteItem>();
            FavoriteItem item;

            if (rootNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    if (node.Name == LinkName)
                    {
                        item = new FavoriteItem(node.Attributes[TitleAttribute].Value, node.Attributes[URLAttribute].Value);
                        list.Add(item);
                    }
                }
            }

            return list;
        }

        ~FavoriteXML()
        {
            doc.Save(FileName);
        }
    }

    public class FavoriteItem : INotifyPropertyChanged
    {
        private string title;
        private string url;
        private Uri uri;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public string URL
        {
            get { return url; }
            set
            {
                url = value;
                URI = new Uri(url, UriKind.RelativeOrAbsolute);
                OnPropertyChanged("URL");
            }
        }

        public Uri URI
        {
            get { return uri; }
            private set
            {
                uri = value;
                OnPropertyChanged("URI");
            }
        }

        public FavoriteItem(string title, string url)
        {
            Title = title;
            URL = url;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
