using System;
using Avalonia.Media.Imaging;

namespace Nitrox.Launcher.Models.Design;

public sealed record NitroxBlog(string Title, DateOnly Date, string Url, Bitmap Image)
{
    public NitroxBlog() : this("", default, "", null)
    {

    }
}
