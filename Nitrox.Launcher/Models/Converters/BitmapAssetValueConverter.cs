using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Path = System.IO.Path;

namespace Nitrox.Launcher.Models.Converters;

public class BitmapAssetValueConverter : Converter<BitmapAssetValueConverter>, IValueConverter
{
    private static readonly string assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new Exception("Unable to get Assembly name");
    private static readonly Dictionary<string, Bitmap> assetCache = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        if (value is not string rawUri || !targetType.IsAssignableFrom(typeof(Bitmap)))
        {
            throw new NotSupportedException();
        }
        if (assetCache.TryGetValue(rawUri, out Bitmap bitmap))
        {
            return bitmap;
        }
        Uri uri = rawUri.StartsWith("avares://") ? new Uri(rawUri) : new Uri($"avares://{assemblyName}{rawUri}");
        if (!AssetLoader.Exists(uri) && !Avalonia.Controls.Design.IsDesignMode)
        {
            return null;
        }
        // In design mode, resource aren't yet embedded.
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            bitmap = TryLoadFromLocalFileSystem(rawUri);
        }

        bitmap ??= new Bitmap(AssetLoader.Open(uri));
        assetCache.Add(rawUri, bitmap);
        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

    private Bitmap TryLoadFromLocalFileSystem(string fileUri)
    {
        string targetedProject = Path.GetDirectoryName(Environment.GetCommandLineArgs().FirstOrDefault(part => !part.Contains("Designer", StringComparison.Ordinal) && part.EndsWith("dll", StringComparison.OrdinalIgnoreCase) && File.Exists(part)));
        while (targetedProject != null && !Directory.EnumerateFileSystemEntries(targetedProject, "*.csproj", SearchOption.TopDirectoryOnly).Any())
        {
            targetedProject = Path.GetDirectoryName(targetedProject);
        }
        if (targetedProject == null)
        {
            return null;
        }
        while (fileUri.StartsWith('/') | fileUri.StartsWith('\\'))
        {
            fileUri = fileUri.Substring(1);
        }
        return new(Path.Combine(targetedProject, fileUri));
    }
}
