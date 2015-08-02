using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ImageItemCollection GetImageSources(ref string html)
        {
            return sourceParser.ParseAll(ref html);
        }
    }

    interface ISourceParser
    {
        ImageItemCollection ParseAll(ref string html);
    }

    class SourceParser : ISourceParser
    {
        public string Resize(string url)
        {
            string filename = url.Split('/').Last();
            
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            Regex sizeRegex = new Regex("[-_]?[0-9]{3,}[_xX][0-9]{3,}\\.");

            if (sizeRegex.IsMatch(filename))
            {
                string sizeStr = sizeRegex.Match(filename).Value;
                return url.Replace(sizeStr, ".");
            }
            return null;
        }

        public ImageItemCollection ParseAll(ref string html)
        {
            ImageItem item;
            ImageItemCollection list = null;
            MatchCollection mc = FirstStep(ref html);

            if (mc.Count < 1)
                return null;

            List<string> urls = SecondStep(mc);
            list = new ImageItemCollection();

            foreach(string m in urls)
            {
                item = new ImageItem(m);
                list.Add(item);
                string resized = Resize(m);
                if (resized != null)
                {
                    item = new ImageItem(resized);
                    list.Add(item);
                }
            }

            if (list.Count < 1)
                return null;

            return list;
        }
        
        private MatchCollection FirstStep(ref string html)
        {
            Regex first = new Regex("(src=[\"']?|;)(http|https)(://)([^<>\"'?&=]*?)[<>\"'?&=]");
            
            return first.Matches(html);
        }

        private List<string> SecondStep(MatchCollection mc)
        {
            List<string> list = new List<string>();

            foreach(Match m in mc)
            {
                string origin = m.Value;
                string first = "";
                for(int i = 2; i < m.Groups.Count; i++)
                {
                    first += m.Groups[i];
                }
                string extension = first.Split('.').Last();
                switch (extension.ToLower())
                {
                    case "js":
                    case "html":
                    case "htm":
                    case "php":
                    case "asp":
                    case "jsp":
                    case "aspx":
                    case "svg":
                    case "css":
                        break;
                    default:
                        list.Add(first);
                        break;
                }
            }

            return list;
        }
    }
}