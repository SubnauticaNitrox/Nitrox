using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_SetTabs_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.SetTabs(default));

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
            // Array index must be fixed so that the callback is executed with its precise value
            int arrayIndex = array.Length - i - 1;
            int tabIndex = customTabs.Count - i - 1;

            string tabIconAssetName = customTabs[tabIndex].TabIconAssetName;
            if (!nitroxTabManager.TryGetTabSprite(tabIconAssetName, out Atlas.Sprite sprite))
            {
                nitroxTabManager.SetSpriteLoadedCallback(tabIconAssetName, callbackSprite => AssignSprite(__instance.toolbar, arrayIndex, callbackSprite));
                // Take the fallback icon from another tab
                sprite = SpriteManager.Get(SpriteManager.Group.Tab, $"Tab{customTabs[tabIndex].FallbackTabIcon}");
            }
            array[arrayIndex] = sprite;
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
