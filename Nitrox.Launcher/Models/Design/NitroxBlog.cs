using System;
using Avalonia.Media.Imaging;

namespace Nitrox.Launcher.Models.Design;

public record NitroxBlog(string Title, DateOnly Date, string Url, Bitmap Image);
