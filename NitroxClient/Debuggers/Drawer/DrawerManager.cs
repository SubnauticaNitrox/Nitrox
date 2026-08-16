using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Debuggers.Drawer.Nitrox;
using NitroxClient.Debuggers.Drawer.Subnautica;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxClient.Debuggers.Drawer.UnityUI;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer;

/// <summary>
///     Registers known drawers into lookup dictionaries that are searched when <see cref="TryDraw{T}" /> and
///     <see cref="TryDrawEditor{T}" /> are called.
/// </summary>
internal sealed class DrawerManager
{
    private readonly Dictionary<Type, IDrawer<object>> drawers;
    private readonly Dictionary<Type, IEditorDrawer<object>> editorDrawers;

    public DrawerManager(IEnumerable<IDrawer<object>> drawers, IEnumerable<IEditorDrawer<object>> editorDrawers)
    {
        this.drawers = drawers.ToDictionary(drawer => drawer.GetType(), drawer => drawer);
        this.editorDrawers = editorDrawers.ToDictionary(drawer => drawer.GetType(), drawer => drawer);
    }

    /// <summary>
    ///     Tries to draw the item given its type. If item is null, returns false and does nothing.
    /// </summary>
    /// <returns>True if a drawer is known for the given item type.</returns>
    public bool TryDraw<T>(T item)
    {
        if (item == null)
        {
            return false;
        }
        if (!drawers.TryGetValue(item.GetType(), out IDrawer<object> drawer))
        {
            return false;
        }
        drawer.Draw(item);
        return true;
    }

    /// <summary>
    ///     Tries to draw the editor given the type of item. If item is null, returns false and does nothing.
    /// </summary>
    /// <param name="item">Item to draw the editor for.</param>
    /// <param name="result">Changed result from the editor.</param>
    /// <returns>True if an editor is known for the given item type.</returns>
    public bool TryDrawEditor<T>(T item, out T result)
    {
        if (item == null)
        {
            result = default;
            return false;
        }
        if (!editorDrawers.TryGetValue(item.GetType(), out IEditorDrawer<object> drawer))
        {
            result = default;
            return false;
        }
        result = (T)drawer.Draw(item);
        return true;
    }
}
