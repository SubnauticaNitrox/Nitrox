using System.Collections.Generic;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher;

public static class Extensions
{
    public static IEnumerable<IVisual> GetNitroxSelected(this IVisual root)
    {
        if (root is AvaloniaObject obj && obj.GetValue(NitroxAttached.SelectedProperty))
        {
            yield return root;
        }
        foreach (IVisual child in root.VisualChildren)
        {
            foreach (IVisual item in GetNitroxSelected(child))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<IVisual> GetNitroxSelected(this IList<IVisual> visuals)
    {
        foreach (IVisual child in visuals)
        {
            foreach (IVisual item in GetNitroxSelected(child))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<TControl> FindControls<TControl>(this IEnumerable<IVisual> visuals)
    {
        foreach (IVisual visual in visuals)
        {
            if (visual is TControl control)
            {
                yield return control;
            }
            foreach (TControl child in visual.VisualChildren.FindControls<TControl>())
            {
                yield return child;
            }
        }
    }
    
    public static IEnumerable<TControl> FindControls<TControl>(this IEnumerable<ILogical> visuals)
    {
        foreach (ILogical visual in visuals)
        {
            if (visual is TControl control)
            {
                yield return control;
            }
            foreach (TControl child in visual.LogicalChildren.FindControls<TControl>())
            {
                yield return child;
            }
        }
    }
}
