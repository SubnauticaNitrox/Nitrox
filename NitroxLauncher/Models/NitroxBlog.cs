using System;

namespace NitroxLauncher.Models
{
    [Serializable]
    internal class NitroxBlog
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public string Image { get; set; }

        public string Author { get; set; }

        public string Summary { get; set; }

        protected NitroxBlog()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NitroxBlog(string title, string url, string image, string author, string summary)
        {
            Title = title;
            Url = url;
            Image = image;
            Author = author;
            Summary = summary;
        }

        public override string ToString()
        {
            return $"[NitroxBlog - Title: {Title}, Url: {Url}, Image: {Image}, Author: {Author}, Summary: {Summary}]";
        }
    }
}
