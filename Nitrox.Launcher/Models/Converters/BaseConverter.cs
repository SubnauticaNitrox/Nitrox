using System;
using Avalonia.Markup.Xaml;

namespace Nitrox.Launcher.Models.Converters;

public abstract class BaseConverter<TSelf> : MarkupExtension where TSelf : BaseConverter<TSelf>, new()
{
    public static readonly TSelf Instance = new();

    public sealed override object ProvideValue(IServiceProvider serviceProvider) => Instance;
}

