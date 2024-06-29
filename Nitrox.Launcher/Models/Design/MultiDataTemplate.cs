extern alias JB;
using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using JB::JetBrains.Annotations;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Selects a <see cref="DataTemplate" /> based on its <see cref="DataTemplate.DataType" />.
/// </summary>
public class MultiDataTemplate : AvaloniaList<DataTemplate>, IDataTemplate
{
    [Content]
    [UsedImplicitly]
    public List<DataTemplate> Content { get; set; } = new();

    public bool Match(object data)
    {
        foreach (DataTemplate template in Content)
        {
            if (template.DataType?.IsInstanceOfType(data) ?? false)
            {
                return true;
            }
        }

        return false;
    }

    public Control Build(object param)
    {
        foreach (DataTemplate template in Content)
        {
            if (template.DataType?.IsInstanceOfType(param) ?? false)
            {
                return template.Build(param);
            }
        }

        return new TextBlock() { Text = "" };
    }
}
