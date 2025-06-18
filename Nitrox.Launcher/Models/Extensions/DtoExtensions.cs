using System;
using System.Web;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;

namespace Nitrox.Launcher.Models.Extensions;

internal static class DtoExtensions
{
    public static NitroxBlog FromDtoToLauncher(this NitroxBlogService.BlogPost blogPost, Bitmap image) => new(HttpUtility.HtmlDecode(blogPost.Title.Rendered), DateOnly.FromDateTime(blogPost.Released.DateTime), blogPost.PostUrl, image);
}
