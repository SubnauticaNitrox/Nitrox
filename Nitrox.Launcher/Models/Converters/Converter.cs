using System;
using Avalonia.Markup.Xaml;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     A converter base class that provides itself as value to the XAML compiler.
/// </summary>
public abstract class Converter<TSelf> : MarkupExtension
    where TSelf : Converter<TSelf>, new()
{
    private static TSelf Instance { get; } = new();
    public sealed override object ProvideValue(IServiceProvider serviceProvider) => Instance;
}
