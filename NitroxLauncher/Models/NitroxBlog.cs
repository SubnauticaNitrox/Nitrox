using System;
using System.Net;

namespace NitroxLauncher.Models
{
    [Serializable]
    internal class NitroxBlog
    {
        public string Title { get; }

        public DateTime Date { get; }

        public string Url { get; }

        public string Image { get; }

        protected NitroxBlog()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NitroxBlog(string title, DateTime date, string url, string image)
        {
            Title = WebUtility.HtmlDecode(title);
            Date = date;
            Url = url;
            Image = image;
        }

        public override string ToString()
        {
            return $"[{nameof(NitroxBlog)} - Title: {Title}, Date: {Date}, Url: {Url}, Image: {Image}]";
        }
    }
}
