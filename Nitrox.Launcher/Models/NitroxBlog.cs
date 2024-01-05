using System;
using System.Net;
using Avalonia.Media.Imaging;

namespace Nitrox.Launcher.Models;

[Serializable]
public class NitroxBlog
{
    public string Title { get; }

    public DateTime Date { get; }

    public string Url { get; }

    public Bitmap Image { get; }

    protected NitroxBlog()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public NitroxBlog(string title, DateTime date, string url, Bitmap image)
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