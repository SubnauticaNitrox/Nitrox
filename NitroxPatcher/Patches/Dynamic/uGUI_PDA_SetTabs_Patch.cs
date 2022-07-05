using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_SetTabs_Patch : NitroxPatch, IDynamicPatch
{
    private readonly static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.SetTabs(default));

    public static bool Prefix(uGUI_PDA __instance, List<PDATab> tabs)
    {
        int num = (tabs != null) ? tabs.Count : 0;
        Atlas.Sprite[] array = new Atlas.Sprite[num];
        __instance.currentTabs.Clear();
        for (int i = 0; i < num; i++)
        {
            PDATab item = tabs[i];
            array[i] = SpriteManager.Get(SpriteManager.Group.Tab, string.Format("Tab{0}", item.ToString()));
            
            __instance.currentTabs.Add(item);
        }

        NitroxPDATabManager nitroxTabManager = Resolve<NitroxPDATabManager>();
        List<NitroxPDATab> customTabs = new(nitroxTabManager.CustomTabs.Values);
        for (int i = 0; i < customTabs.Count; i++)
        {
            string tabIconAssetName = customTabs[customTabs.Count - i - 1].TabIconAssetName;
            if (!nitroxTabManager.TryGetTabSprite(tabIconAssetName, out Atlas.Sprite sprite))
            {
                nitroxTabManager.SetSpriteLoadedCallback(tabIconAssetName, callbackSprite => AssignSprite(__instance.toolbar, array.Length - i - 1, callbackSprite));
                sprite = new Atlas.Sprite(new Texture2D(100, 100));
            }
            array[array.Length - i - 1] = sprite;
        }

        uGUI_Toolbar uGUI_Toolbar = __instance.toolbar;
        object[] content = array;
        uGUI_Toolbar.Initialize(__instance, content, null, 15);
        __instance.CacheToolbarTooltips();
        return false;
    }

    private static void AssignSprite(uGUI_Toolbar toolbar, int index, Atlas.Sprite sprite)
    {
        toolbar.icons[index].SetForegroundSprite(sprite);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
