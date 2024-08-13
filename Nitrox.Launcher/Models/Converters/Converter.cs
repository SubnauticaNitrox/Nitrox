using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     A converter base class that provides itself as value to the XAML compiler.
/// </summary>
public abstract class Converter<TSelf> : MarkupExtension, IValueConverter
    where TSelf : Converter<TSelf>, new()
{
    private static TSelf Instance { get; } = new();

    public sealed override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
