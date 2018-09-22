using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Scribd_Document_Downloader
{
    public class Scribd
    {
        private String source;
        private String id;
        private String title;
        private List<String> pageLink;
        public Scribd(string url){
            source = GetSource(url);
            id = GetID(source);
            title = GetTitle(source);
            pageLink = GetLink(source, id);
        }

        private  String GetSource(String url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            req.Method = "GET";
            String source;
            using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                source = reader.ReadToEnd();
            }
            return source;
        }

        private  String GetID(String source)
        {
            return source.Split(new[] { "docManager.assetPrefix = \"" }, StringSplitOptions.None)[1].Split('\"')[0];
        }

        private  String GetTitle(String source)
        {
            return source.Split(new[] { "<title>" }, StringSplitOptions.None)[1].Split(new[] { "</title>" }, StringSplitOptions.None)[0];
        }

        private  List<String> GetLink(String source, String id)
        {
            List<string> pageLink = new List<string>();
            var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (Match m in linkParser.Matches(source))
                if (m.ToString().Contains(id + "/pages/") || m.ToString().Contains(id + "/images/"))
                {
                    string s = m.ToString();
                    s = s.Replace("\">", "");
                    s = s.Replace("</div>", "");
                    s = s.Replace("</div", "");
                    s = s.Replace("jsonp", "jpg");
                    s = s.Replace("pages", "images");
                    pageLink.Add(s);
                }
            return pageLink;
        }

        public void Start() {
            Download(pageLink, id, title);
        }

        private  bool Download(List<String> pageLink, String id, String title)
        {
            try
            {
                WebClient wc = new WebClient();
                System.IO.Directory.CreateDirectory(id);
                for (int i = 0; i < pageLink.Count; i++)
                {
                    Console.WriteLine($"Downloading {i + 1} pages of {pageLink.Count}");
                    wc.DownloadFile(pageLink[i], $"{id}/{(i + 1)}.jpg");
                }

                Console.WriteLine("Merging images in a pdf");
                PdfDocument document = new PdfDocument();
                for (int i = 0; i < pageLink.Count; i++)
                {
                    PdfPage page = document.AddPage();
                    page.Orientation = PageOrientation.Portrait;
                    using (XImage img = XImage.FromFile($"{id}/{(i + 1)}.jpg"))
                    {
                        page.Width = img.PixelWidth;
                        page.Height = img.PixelHeight;
                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        gfx.DrawImage(img, 0, 0, img.PixelWidth, img.PixelHeight);
                    }
                    File.Delete($"{id}/{(i + 1)}.jpg");
                }
                document.Save($"{title}.pdf");
                Directory.Delete(id);
                Console.WriteLine($"Done {Directory.GetCurrentDirectory().ToString() + "\\" + title}.pdf");
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
