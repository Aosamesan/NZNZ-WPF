using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mshtml;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NZNZWPF
{
    class ImageParser
    {
        ISourceParser sourceParser;

        public ImageParser(ISourceParser parser)
        {
            sourceParser = parser;
        }

        public List<string> GetImageSources(IHTMLDocument2 doc)
        {
            List<string> list = null;

            if (doc.images.length > 0)
            {
                list = new List<string>();

                foreach(HTMLImg img in doc.images)
                {
                    string tmp = sourceParser.Parse(img.src);

                    if (!string.IsNullOrWhiteSpace(tmp))
                        list.Add(tmp);
                }
            }
            
            return list;
        }

        public ImageItemCollection GetImageSources(ref string html)
        {
            return sourceParser.ParseAll(ref html);
        }
    }

    interface ISourceParser
    {
        string Parse(string url);
        ImageItemCollection ParseAll(ref string html);
    }

    class SourceParser : ISourceParser
    {
        public string Parse(string url)
        {
            string filename = url.Split('/').Last();

            System.Windows.MessageBox.Show(url + "\n" + filename);
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            Regex second = new Regex("(http|https)(://)([^<>\"'?&]*?)([-_]?[m0-9]{1,}[xX][0-9]*?)($|\\.jpg|\\.png|\\.bmp|\\.jpeg|\\.gif)");

            System.Windows.MessageBox.Show(url);
            Match m = second.Match(url);

            if (string.IsNullOrWhiteSpace(m.Value))
                return url;

            string result = "";

            for (int i = 1; i < 7; i++)
                if(i != 4)
                    result += m.Groups[i];
            return result;
        }

        public bool IsImageURL(string url)
        {
            bool result = true;
            if (url == null)
                return false;

            try
            {
                BitmapImage image = new BitmapImage(new Uri(url));
                
            }
            catch(Exception e)
            {
            }

            return result;
        }

        public ImageItemCollection ParseAll(ref string html)
        {
            ImageItemCollection list = null;
            List<string> mc = FirstStep(ref html);

            if (mc.Count < 1)
                return null;

            list = new ImageItemCollection();

            foreach(string m in mc)
            {
                IsImageURL("Http://www.naver.com/");
                string src = Parse(m);
                if (!IsImageURL(src))
                    continue;
                ImageItem item = new ImageItem(src);
                list.Add(item);
            }

            if (list.Count < 1)
                return null;

            return list;
        }
        
        private List<string> FirstStep(ref string html)
        {
            Regex first = new Regex("(<.*?)(http|https)(://)([^<>?&\"']*)(/[a-zA-Z0-9-_]*?)(\\.jpg|\\.png|\\.bmp|\\.jpeg|\\.gif|\\s)[<>?&\"'](.*?>)");
            MatchCollection matches = first.Matches(html);
            Regex second = new Regex("(http|https)(://)([^<>?&\"']*)(/[a-zA-Z0-9-_]*?)(\\.jpg|\\.png|\\.bmp|\\.jpeg|\\.gif|\\s)$");

            List<string> list = new List<string>();
            foreach(Match m in matches)
            {
                string filename = "";
                for(int i = 2; i < m.Groups.Count - 1; i++)
                {
                    filename += m.Groups[i];
                }

                if (string.IsNullOrWhiteSpace(filename))
                    continue;
                if (!second.IsMatch(filename))
                    continue;

                string extension = filename.Split('.').Last();

                switch (extension.ToUpper())
                {
                    case "HTML":
                    case "CSS":
                    case "JS":
                    case "JSP":
                        break;
                    default:
                        list.Add(filename);
                        break;
                }
            }

            return list;
        }
    }


}
