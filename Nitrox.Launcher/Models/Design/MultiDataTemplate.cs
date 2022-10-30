using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Selects a <see cref="DataTemplate"/> based on its <see cref="DataTemplate.DataType"/>.
/// </summary>
public class MultiDataTemplate : AvaloniaList<DataTemplate>, IDataTemplate
{
    [Content]
    public List<DataTemplate> Content { get; set; } = new();
    
    public bool Match(object data)
    {
        if (data is not INavigationItem navItem)
        {
            return false;
        }
        foreach (DataTemplate template in Content)
        {
            if (template.DataType.IsInstanceOfType(data))
            {
                return true;
            }
        }

        return false;
    }

    public IControl Build(object param)
    {
        foreach (DataTemplate template in Content)
        {
            if (template.DataType.IsInstanceOfType(param))
            {
                return template.Build(param);
            }
        }

        return new TextBlock { Text = param.ToString() };
    }
}
