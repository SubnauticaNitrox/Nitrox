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
public class MultiDataTemplate : AvaloniaList<DataTemplate>, IRecyclingDataTemplate
{
    [Content]
    [UsedImplicitly]
    public List<DataTemplate> Content { get; set; } = new();

    private readonly Dictionary<Type, Control> typeToControlCache = [];

    public bool Match(object data) => GetTemplateForType(data?.GetType()) != null;

    public Control Build(object data, Control existing)
    {
        Type type = data?.GetType();
        if (type != null && typeToControlCache.TryGetValue(type, out Control control))
        {
            return control;
        }
        Control build = GetTemplateForType(type)?.Build(data);
        if (type != null && build != null)
        {
            typeToControlCache[type] = build;
        }

        return build ?? existing;
    }

    public Control Build(object data) => GetTemplateForType(data.GetType())?.Build(data) ?? new TextBlock { Text = "" };

    private IDataTemplate GetTemplateForType(Type type)
    {
        if (type == null)
        {
            return null;
        }
        foreach (DataTemplate template in Content)
        {
            if (template.DataType?.IsAssignableTo(type) ?? false)
            {
                return template;
            }
        }
        return null;
    }
}
