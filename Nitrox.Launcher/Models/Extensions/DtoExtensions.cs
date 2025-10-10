using System;
using System.Text;
using System.Web;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;

namespace Nitrox.Launcher.Models.Extensions;

internal static class DtoExtensions
{
    public static NitroxBlog FromDtoToLauncher(this NitroxBlogService.BlogPost blogPost, Bitmap image) => new(HttpUtility.HtmlDecode(blogPost.Title.Rendered), DateOnly.FromDateTime(blogPost.Released.DateTime), blogPost.PostUrl, image);
    
    public static NitroxChangelog FromDtoToLauncher(this NitroxWebsiteApiService.ChangeLog changeLog, StringBuilder? buffer = null)
    {
        buffer ??= new StringBuilder();
        buffer.Clear();
        foreach (string patchNote in changeLog.PatchNotes)
        {
            if ((GetAsHeaderOrNull(patchNote, '-') ?? GetAsHeaderOrNull(patchNote, '#')) is {} header)
            {
                buffer.AppendLine(header);
            }
            else
            {
                buffer.AppendLine($"• {patchNote}");
            }
        }
        return new NitroxChangelog(changeLog.Version, changeLog.Released.DateTime, string.Join(Environment.NewLine, buffer.ToString()));

        static string? GetAsHeaderOrNull(string line, char prefix)
        {
            if (line.StartsWith(prefix))
            {
                return $"\n[b][u]{line.TrimStart(prefix, ' ')}[/u][/b]";
            }
            return null;
        }
    }
}
