using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Utils;

namespace Nitrox.Launcher.Models.Converters;

public sealed class BoolToIconConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// String that will be outputted if the input boolean value is <c>true</c>
    /// </summary>
    public string True { get; set; }

    /// <summary>
    /// String that will be outputted if the input boolean value is <c>false</c>
    /// </summary>
    public string False { get; set; }

    /// <summary>
    /// Decides if the converter will inverse the input boolean value before computing the output
    /// </summary>
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool @bool)
        {
            return null;
        }

        if (Invert)
        {
            @bool = !@bool;
        }

        return AssetHelper.GetAssetFromStream(@bool ? True : False, static stream => new Bitmap(stream));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
