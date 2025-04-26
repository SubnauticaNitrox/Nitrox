using System;
using System.Collections.Generic;
using NitroxClient.Debuggers.Drawer.Nitrox;
using NitroxClient.Debuggers.Drawer.Subnautica;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxClient.Debuggers.Drawer.UnityUI;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer;

/// <summary>
///     Registers known drawers into lookup dictionaries that are searched when <see cref="TryDraw{T}" /> and
///     <see cref="TryDrawEditor{T}" /> are called.
/// </summary>
public class DrawerManager
{
    private readonly Dictionary<Type, IDrawer<object>> drawers = new();
    private readonly Dictionary<Type, IEditorDrawer<object>> editorDrawers = new();

    public DrawerManager(SceneDebugger sceneDebugger)
    {
        Validate.NotNull(sceneDebugger);

        ColorDrawer colorDrawer = new();
        UnityEventDrawer unityEventDrawer = new();
        SelectableDrawer selectableDrawer = new(sceneDebugger, colorDrawer);
        VectorDrawer vectorDrawer = new();
        RectDrawer rectDrawer = new();
        LayoutGroupDrawer layoutGroupDrawer = new(rectDrawer);
        MaterialDrawer materialDrawer = new();
        ImageDrawer imageDrawer = new(colorDrawer, materialDrawer, rectDrawer);
        NitroxEntityDrawer nitroxEntityDrawer = new();

        AddDrawer<NitroxEntityDrawer, NitroxEntity>(nitroxEntityDrawer);
        AddDrawer<NitroxEntityDrawer, NitroxId>(nitroxEntityDrawer);
        AddDrawer<FMODAssetDrawer, FMODAsset>();
        AddDrawer<UWEEventDrawer, UWE.Event<float>>();
        AddDrawer<UWEEventDrawer, UWE.Event<PowerRelay>>();
        AddDrawer<AspectRatioFitterDrawer, AspectRatioFitter>();
        AddDrawer<ButtonDrawer, Button>(new(selectableDrawer, unityEventDrawer));
        AddDrawer<CanvasDrawer, Canvas>(new(sceneDebugger));
        AddDrawer<CanvasGroupDrawer, CanvasGroup>();
        AddDrawer<CanvasRendererDrawer, CanvasRenderer>();
        AddDrawer<CanvasScalerDrawer, CanvasScaler>(new(vectorDrawer));
        AddDrawer<ContentSizeFitterDrawer, ContentSizeFitter>();
        AddDrawer<DropdownDrawer, Dropdown>(new(sceneDebugger, selectableDrawer));
        AddDrawer<EventTriggerDrawer, EventTrigger>(new(sceneDebugger));
        AddDrawer<GraphicRaycasterDrawer, GraphicRaycaster>();
        AddDrawer<GridLayoutGroupDrawer, GridLayoutGroup>(new(vectorDrawer, rectDrawer));
        AddDrawer<LayoutGroupDrawer, HorizontalLayoutGroup>(layoutGroupDrawer);
        AddDrawer<ImageDrawer, Image>(imageDrawer);
        AddDrawer<ImageDrawer, RawImage>(imageDrawer);
        AddDrawer<LayoutGroupDrawer, VerticalLayoutGroup>(layoutGroupDrawer);
        AddDrawer<MaskDrawer, Mask>();
        AddDrawer<RectTransformDrawer, RectTransform>(new(vectorDrawer));
        AddDrawer<ScrollbarDrawer, Scrollbar>(new(sceneDebugger, selectableDrawer));
        AddDrawer<ScrollRectDrawer, ScrollRect>(new(sceneDebugger));
        AddDrawer<SelectableDrawer, Selectable>(selectableDrawer);
        AddDrawer<SliderDrawer, Slider>(new(sceneDebugger, selectableDrawer));
        AddDrawer<TextDrawer, Text>(new(colorDrawer, materialDrawer));
        AddDrawer<ToggleDrawer, Toggle>(new(sceneDebugger, selectableDrawer, unityEventDrawer));
        AddDrawer<ToggleGroupDrawer, ToggleGroup>();
        AddDrawer<RigidbodyDrawer, Rigidbody>(new(vectorDrawer));
        AddDrawer<TransformDrawer, Transform>(new(sceneDebugger, vectorDrawer));
        AddDrawer<UnityEventDrawer, UnityEvent>(unityEventDrawer);
        AddDrawer<UnityEventDrawer, UnityEvent<bool>>(unityEventDrawer);
        AddDrawer<AnimatorDrawer, Animator>();
        AddDrawer<CharacterControllerDrawer, CharacterController>(new(vectorDrawer));

        AddEditor<VectorDrawer, Vector2>(vectorDrawer);
        AddEditor<VectorDrawer, Vector3>(vectorDrawer);
        AddEditor<VectorDrawer, Vector4>(vectorDrawer);
        AddEditor<VectorDrawer, Quaternion>(vectorDrawer);
        AddEditor<VectorDrawer, Int3>(vectorDrawer);
        AddEditor<VectorDrawer, NitroxVector3>(vectorDrawer);
        AddEditor<VectorDrawer, NitroxVector4>(vectorDrawer);
        AddEditor<ColorDrawer, Color>(colorDrawer);
        AddEditor<ColorDrawer, Color32>(colorDrawer);
        AddEditor<MaterialDrawer, Material>(materialDrawer);
        AddEditor<RectDrawer, Rect>(rectDrawer);
        AddEditor<RectDrawer, RectOffset>(rectDrawer);
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

    private void AddDrawer<TDrawer, TDrawable>(TDrawer drawer) where TDrawer : IDrawer<TDrawable>
    {
        drawers.Add(typeof(TDrawable), new DrawerWrapper<TDrawable>(drawer));
    }

    private void AddDrawer<TDrawer, TDrawable>() where TDrawer : IDrawer<TDrawable>, new()
    {
        drawers.Add(typeof(TDrawable), new DrawerWrapper<TDrawable>(new TDrawer()));
    }

    private void AddEditor<TDrawer, TDrawable>(TDrawer drawer) where TDrawer : IEditorDrawer<TDrawable>
    {
        editorDrawers.Add(typeof(TDrawable), new EditorDrawerWrapper<TDrawable>(drawer));
    }

    private class DrawerWrapper<T>(IDrawer<T> inner) : IDrawer<object>
    {
        public void Draw(object target) => inner.Draw((T)target);
    }

    private class EditorDrawerWrapper<T>(IEditorDrawer<T> inner) : IEditorDrawer<object>
    {
        public object Draw(object target) => inner.Draw((T)target);
    }
}
