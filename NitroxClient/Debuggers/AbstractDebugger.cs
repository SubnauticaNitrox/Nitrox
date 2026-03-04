using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public abstract class AbstractDebugger
{
    public virtual bool Enabled { get; set; }

    public int WindowId { get; }

    public string? DebuggerName { get; }

    public KeyCode Hotkey { get; }

    public bool HotkeyAltRequired { get; }

    public bool HotkeyControlRequired { get; }

    public bool HotkeyShiftRequired { get; }

    public string HotkeyString { get; }

    protected GUISkinCreationOptions SkinCreationOptions { get; }

    protected float MaxHeight { get; }

    protected DebuggerTab? ActiveTab { get; set; }

    private readonly Dictionary<string, DebuggerTab> tabs = [];
    private Rect windowRect;

    protected AbstractDebugger(int windowId, float desiredWidth, string? debuggerName = null, KeyCode hotkey = KeyCode.None, bool control = false, bool shift = false, bool alt = false, GUISkinCreationOptions skinOptions = GUISkinCreationOptions.DEFAULT, float maxHeight = 1000f)
    {
        WindowId = windowId;
        MaxHeight = maxHeight;

        if (desiredWidth < 200)
        {
            desiredWidth = 200;
        }

        windowRect = new Rect(Screen.width / 2 - (desiredWidth / 2), Screen.height * 0.1f, desiredWidth, Math.Min(Screen.height * 0.8f, maxHeight)); // Default position in center of screen.

        Hotkey = hotkey;
        HotkeyAltRequired = alt;
        HotkeyShiftRequired = shift;
        HotkeyControlRequired = control;
        HotkeyString = Hotkey == KeyCode.None ? "None" : $"{(HotkeyControlRequired ? "CTRL+" : "")}{(HotkeyAltRequired ? "ALT+" : "")}{(HotkeyShiftRequired ? "SHIFT+" : "")}{Hotkey}";
        SkinCreationOptions = skinOptions;

        if (string.IsNullOrEmpty(debuggerName))
        {
            string name = GetType().Name;
            DebuggerName = name.Substring(0, name.IndexOf("Debugger", StringComparison.Ordinal));
        }
        else
        {
            DebuggerName = debuggerName;
        }
    }

    protected DebuggerTab AddTab(string name, Action render)
    {
        DebuggerTab tab = new(name, render);
        tabs.Add(tab.Name, tab);
        return tab;
    }

    protected Optional<DebuggerTab> GetTab([NotNull] string? name)
    {
        Validate.NotNull(name);

        tabs.TryGetValue(name, out DebuggerTab tab);
        return Optional.OfNullable(tab);
    }

    /// <summary>
    /// Call this inside a <see cref="MonoBehaviour.OnGUI"/> method.
    /// </summary>
    public virtual void OnGUI()
    {
        if (!Enabled)
        {
            return;
        }

        GUISkin skin = GetSkin();
        GUISkinUtils.RenderWithSkin(skin, () =>
        {
            windowRect = GUILayout.Window(WindowId, windowRect, RenderInternal, $"[DEBUGGER] {DebuggerName}", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        });
    }

    /// <summary>
    /// Optionally adjust the skin that is used during render.
    /// </summary>
    /// <remarks>
    /// Set <see cref="SkinCreationOptions"/> on <see cref="GUISkinCreationOptions.UNITYCOPY"/> or <see cref="GUISkinCreationOptions.DERIVEDCOPY"/> in constructor before using this method.
    /// </remarks>
    /// <param name="skin">Skin that is being used during <see cref="Render(int)"/>.</param>
    protected virtual void OnSetSkin(GUISkin skin)
    {
    }

    /// <summary>
    /// Optionally use a custom render solution for the debugger by overriding this method.
    /// </summary>
    protected virtual void Render()
    {
        ActiveTab?.Render();
    }

    /// <summary>
    /// Gets (a copy of) a skin specified by <see cref="GUISkinCreationOptions"/>.
    /// </summary>
    /// <returns>A reference to an existing or copied skin.</returns>
    private GUISkin GetSkin()
    {
        GUISkin skin = GUI.skin;
        string skinName = GetSkinName();
        switch (SkinCreationOptions)
        {
            case GUISkinCreationOptions.DEFAULT:
                skin = GUISkinUtils.RegisterDerivedOnce("debuggers.default", s =>
                {
                    SetBaseStyle(s);
                    OnSetSkinImpl(s);
                });
                break;

            case GUISkinCreationOptions.UNITYCOPY:
                skin = GUISkinUtils.RegisterDerivedOnce(skinName, OnSetSkinImpl);
                break;

            case GUISkinCreationOptions.DERIVEDCOPY:
                GUISkin baseSkin = GUISkinUtils.RegisterDerivedOnce("debuggers.default", SetBaseStyle);
                skin = GUISkinUtils.RegisterDerivedOnce(skinName, OnSetSkinImpl, baseSkin);
                break;
        }

        return skin;
    }

    private string GetSkinName()
    {
        string name = GetType().Name;
        return $"debuggers.{name.Substring(0, name.IndexOf("Debugger")).ToLowerInvariant()}";
    }

    private void OnSetSkinImpl(GUISkin skin)
    {
        if (SkinCreationOptions == GUISkinCreationOptions.DEFAULT)
        {
            Enabled = false;
            throw new NotSupportedException($"Cannot change {nameof(GUISkin)} for {GetType().FullName} when accessing the default skin. Change {nameof(SkinCreationOptions)} to something else than {nameof(GUISkinCreationOptions.DEFAULT)}.");
        }

        OnSetSkin(skin);
    }

    private void RenderInternal(int windowId)
    {
        using (new GUILayout.HorizontalScope("Box"))
        {
            if (tabs.Count == 1)
            {
                GUILayout.Label(tabs.First().Key, "tabActive");
            }
            else
            {
                foreach (DebuggerTab tab in tabs.Values)
                {
                    if (GUILayout.Button(tab.Name, ActiveTab == tab ? "tabActive" : "tab"))
                    {
                        ActiveTab = tab;
                    }
                }
            }
        }

        Render();
        GUI.DragWindow();
    }

    private void SetBaseStyle(GUISkin skin)
    {
        skin.label.alignment = TextAnchor.MiddleLeft;
        skin.label.margin = new RectOffset();
        skin.label.padding = new RectOffset();

        skin.SetCustomStyle("header", skin.label, s =>
        {
            s.margin.top = 10;
            s.margin.bottom = 10;
            s.alignment = TextAnchor.MiddleCenter;
            s.fontSize = 16;
            s.fontStyle = FontStyle.Bold;
        });

        skin.SetCustomStyle("tab", skin.button, s =>
        {
            s.fontSize = 16;
            s.margin = new RectOffset(5, 5, 5, 5);
        });

        skin.SetCustomStyle("tabActive", skin.button, s =>
        {
            s.fontStyle = FontStyle.Bold;
            s.fontSize = 16;
        });
    }

    public virtual void ResetWindowPosition()
    {
        // Reset position of debuggers because SN sometimes throws the windows from planet 4546B
        windowRect = new Rect(Screen.width / 2f - (windowRect.width / 2), Screen.height / 2f - (windowRect.height / 2), windowRect.width, Math.Min(Screen.height * 0.8f, MaxHeight));
    }

    public enum GUISkinCreationOptions
    {
        /// <summary>
        /// Uses the NitroxDebug skin.
        /// </summary>
        DEFAULT,

        /// <summary>
        /// Creates a copy of the default Unity IMGUI skin and sets the copied skin as render skin.
        /// </summary>
        UNITYCOPY,

        /// <summary>
        /// Creates a copy based on the NitroxDebug skin and sets the copied skin as render skin.
        /// </summary>
        DERIVEDCOPY
    }

    public sealed class DebuggerTab
    {
        public string Name { get; }

        public Action Render { get; }

        public DebuggerTab(string name, Action render)
        {
            Validate.NotNull(name, $"Expected a name for the {nameof(DebuggerTab)}");
            Validate.NotNull(render, $"Expected an action for the {nameof(DebuggerTab)}");

            Name = name;
            Render = render;
        }
    }
}
