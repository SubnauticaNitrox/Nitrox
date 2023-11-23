﻿using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Provide Subnautica with the new PDA tabs' names
/// </summary>
public sealed partial class uGUI_PDA_CacheToolbarTooltips_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.CacheToolbarTooltips());

    public static void Postfix(uGUI_PDA __instance)
    {
        // Modify the latest tooltips of the list, which are the ones for the newly created tab
        List<NitroxPDATab> customTabs = new(Resolve<NitroxPDATabManager>().CustomTabs.Values);
        for (int i = 0; i < customTabs.Count; i++)
        {
            /* considering a list like: [a,b,c,d,e,f] (toolbarTooltips)
             * We want to modify only the n (customTabs.Count) last elements to replace with
             * the elements from the list [u,v,w] (customTabs) in the right order so that we have [a,b,c,u,v,w].
             * we start from the element at the end of (toolbarTooltips) and replace it with the last element of (customTabs)
             */
            // We verify that the index is valid (if the toolbarTooltips list is empty)
            if (__instance.toolbarTooltips.Count - i - 1 < 0 || __instance.toolbarTooltips.Count - i - 1 >= __instance.toolbarTooltips.Count)
            {
                continue;
            }
            string toolbarTooltip = customTabs[customTabs.Count - i - 1].ToolbarTip;
            StringBuilder stringBuilder = new();
            TooltipFactory.Label(toolbarTooltip, stringBuilder);
            __instance.toolbarTooltips[__instance.toolbarTooltips.Count - i - 1] = stringBuilder.ToString();
        }
    }
}
